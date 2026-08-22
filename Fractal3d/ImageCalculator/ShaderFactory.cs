namespace ImageCalculator;

//using ABI.System.Numerics;
using FractureCommonLib;
using System.Numerics;
using System.Reactive.Subjects;

public static class MatrixProvider
{
    public static Matrix4x4 GetLeftMatrix()
    {
        return TransformationCalculator.CreateRotationMatrix(0, 0, -30.0f);
    }

    public static Matrix4x4 GetRightMatrix()
    {
        return TransformationCalculator.CreateRotationMatrix(0, 0, 30.0f);
    }

}

public class ShaderFactory : IDisposable
{
    private FractalParams _fractalParams = new(FractalParams.MakeLights());
    private readonly Subject<double> _progressSubject = new();
    public IObservable<double> Progress => _progressSubject;
    private Func<Vector3, float> _distanceEstimator = EstimateDistanceSphere;

    private bool _isDisposed;

    public void Dispose()
    {
        Dispose(true);

        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed)
            return;

        if (disposing)
        {
            _progressSubject.Dispose();
        }

        _isDisposed = true;
    }

    public static float EstimateDistanceSphere(Vector3 p)
    {
        // sphere at origin 0,0,0 of radius 0.4
        // (p-circleOrigin).Length - radius
        return p.Length() - 0.4f;
    }

    public static float EstimateDistanceBox(Vector3 p)
    {
        p -= new Vector3(1f, 0.25f, 0.1f);  // this moves the box to the right
        var box = new Vector3(0.3f, 0.25f, 0.1f);
        var q = Vector3.Abs(p) - box;
        return (Vector3.Max(q, Vector3.Zero)).Length() + Math.Min(Math.Max(Math.Max(q.Y, q.Z), q.X), 0.0f);
    }

    public static Vector3 MaxVec(Vector3 a, float b)
    {
        return new Vector3(Math.Max(a.X, b), Math.Max(a.Y, b), Math.Max(a.Z, b));
    }

    public static float EstimateDistanceBox2(Vector3 p)
    {
        var b = new Vector3(0.5f, 0.25f, 0.1f);
        Vector3 q = Vector3.Abs(p) - b;
        var l = MaxVec(q, 0.0f).Length();
        return (l + (float) Math.Min(Math.Max(q.X, Math.Max(q.Y, q.Z)), 0.0));
    }

    public static float EstimateDistanceTorus(Vector3 p)
    {
        Vector2 t = new Vector2(0.25f, 0.05f);
        Vector2 pxz = new Vector2(p.X, p.Z);
        Vector2 q = new Vector2(pxz.Length() - t.X, p.Y);
        return q.Length() - t.Y;
    }

    public static float EstimateDistanceCapsule(Vector3 p)
    {
        Vector3 a = new Vector3(-0.25f, 0.25f, 0.1f);
        Vector3 b = new Vector3(0.25f, -0.25f, -0.1f);
        float r = 0.1f;
        return sdCapsule(p, a, b, r);
    }

    public static Vector3 RotateEnd(Vector3 start, Vector3 end, float angle)
    {
        var cos = (float)Math.Cos(angle*Math.PI/180.0);
        var sin = (float)Math.Sin(angle*Math.PI/180.0);

        var x = start.X + (end.X - start.X) * cos - (end.Y - start.Y) * sin;
        var y = start.Y + (end.X - start.X) * sin + (end.Y - start.Y) * cos;

        return new Vector3(x, y, end.Z);
    }

    public static Vector3 RotateEnd2(Vector3 start, Vector3 end, Matrix4x4 mat)
    {
        var dif = end - start;
        var inter = TransformationCalculator.Transform(mat, dif);
        return inter + start;
    }

    record struct PointPair(Vector3 Start, Vector3 End);

    public static float EstimateDistanceCompositeOld(Vector3 p)
    {
        var start = new Vector3(0.0f, 0.9f, 0.0f);
        var end = new Vector3(0.0f, 0.2f, 0.0f);
        float r = 0.1f;

        var pts = new List<PointPair>
        {
            new PointPair(start, end)
        };
        var height = (start - end).Length();
        float distance = float.MaxValue;
        var leftMatrix = MatrixProvider.GetLeftMatrix();
        var rightMatrix = MatrixProvider.GetRightMatrix();

        for (int i = 0; i < 4; i++)
        {
            height = height *= 0.7f;
            var pts2 = new List<PointPair>();

            foreach (var pt in pts.ToList())
            {
                distance = Math.Min(distance, sdCapsule(p, pt.Start, pt.End, r));
                var start2 = pt.End;
                var end2 = pt.End;
                end2.Y -= height;
                var left = RotateEnd2(start2, end2, leftMatrix);
                var right = RotateEnd2(start2, end2, rightMatrix);

                pts2.Add(new PointPair(start2, left));
                pts2.Add(new PointPair(start2, right));
            }
            height *= 0.7f;
            pts = pts2;
        }
        return distance;
    }

    private static Vector3 GetNewEnd(Vector3 start, Vector3 end, Matrix4x4 mat, float relativeLength)
    {
        var v = (end - start) * relativeLength;
        var newEnd = end + v;
        return RotateEnd2(end, newEnd, mat);
    }

    public static float EstimateDistanceComposite(Vector3 p)
    {
        var start = new Vector3(0.0f, 0.9f, 0.0f);
        var end = new Vector3(0.0f, 0.2f, 0.0f);
        float r = 0.1f;
        float attenuation = 1.05f;

        var pts = new List<PointPair>
        {
            new PointPair(start, end)
        };
        var length = (end-start).Length();
        float distance = float.MaxValue;
        var leftMatrix = MatrixProvider.GetLeftMatrix();
        var rightMatrix = MatrixProvider.GetRightMatrix();

        for (int i = 0; i < 4; i++)
        {
            length = length *= attenuation;
            var pts2 = new List<PointPair>();

            foreach (var pt in pts.ToList())
            {
                distance = Math.Min(distance, sdCapsule(p, pt.Start, pt.End, r));
                
                var left = GetNewEnd(pt.Start, pt.End, leftMatrix, length);
                var right = GetNewEnd(pt.Start, pt.End, rightMatrix, length);   

                pts2.Add(new PointPair(pt.End, left));
                pts2.Add(new PointPair(pt.End, right));
            }
            pts = pts2;
        }
        return distance;
    }

    // Capsule / Line
    public static float sdCapsule(Vector3 p, Vector3 a, Vector3 b, float r)
    {
        Vector3 pa = p - a;
        Vector3 ba = b - a;
        float h = (float)Math.Clamp(Vector3.Dot(pa, ba) / Vector3.Dot(ba, ba), 0.0, 1.0);
        return (pa - ba * h).Length() - r;
    }

    private bool RayMarch(Vector3 startPt, Vector3 direction, Matrix4x4 transformMatrix, out Vector3 pt)
    {
        float totalDistance = 0.0f;
        int steps;
        bool hit = false;

        pt = startPt;

        for (steps = 0; steps < _fractalParams.MaxRaySteps; steps++)
        {
            pt = totalDistance * direction + startPt;
            var transformedPt = TransformationCalculator.Transform(transformMatrix, pt);
            float distance = _distanceEstimator(transformedPt);
            
            if (distance < _fractalParams.MinRayDistance)
            {
                hit = true;
                break;
            }

            if (totalDistance > _fractalParams.MaxDistance)
                break;

            totalDistance += distance;
        }

        return hit;
    }


    private void CalculateImageNew(RawLightedImage raw, double startProgress, double sumProgress, CancellationToken cancelToken)
    {
        _progressSubject.OnNext(startProgress);

        var size = _fractalParams.ImageSize;
        var palette = _fractalParams.Palette;
        
         
        float left = Math.Min(_fractalParams.FromX, _fractalParams.ToX);
        float right = Math.Max(_fractalParams.FromX, _fractalParams.ToX);
        float bottom = Math.Min(_fractalParams.FromY, _fractalParams.ToY);
        float top = Math.Max(_fractalParams.FromY, _fractalParams.ToY);

        float fromZ = _fractalParams.FromZ;
        float toZ = _fractalParams.ToZ;

        float xRange = (right - left) / size.Width;
        float yRange = (top - bottom) / size.Height;

        var viewPos = new Vector3(0, 0, fromZ);

        var transformMatrix = TransformationCalculator.CreateInvertedTransformationMatrix(_fractalParams.TransformParams);
        var transformedLights = LightUtil.TransformLights(_fractalParams.Lights, transformMatrix);
        var transViewPos = TransformationCalculator.Transform(transformMatrix, viewPos);

        for (var x = 0; x < size.Width; ++x)
        {
            for (var y = 0; y < size.Height; ++y)
            {
                var fx = x * xRange + left;
                var fy = y * yRange + bottom;

                var from = new Vector3(fx, fy, fromZ);

                var to = (_fractalParams.AimToOrigin) ? new Vector3(0.0f, 0.0f, toZ) : new Vector3(fx, fy, toZ);

                var startPt = from + _fractalParams.Distance * to;

                var direction = to - from;

                var hit = RayMarch(startPt, direction, transformMatrix, out var outPt);

                var transformedPt = TransformationCalculator.Transform(transformMatrix, outPt);
                var normal = NormalCalculator.CalculateNormal(_distanceEstimator, _fractalParams.NormalDistance, transformedPt);

                var lighting = (hit) ? LightUtil.GetPointLight(transformedLights, _fractalParams.LightComboMode, transformedPt, transViewPos, normal) : 
                    new Lighting();

                var depth = (hit) ? (palette.NumberOfColors - 1) : 0;

                // need a new raw image that stores Vector3
                var light = lighting.Diffuse + lighting.Specular;
                raw.SetPixel(x, y, depth, light);
            }

            if (cancelToken.IsCancellationRequested)
                return;

            var percentDone = startProgress + sumProgress * x / size.Width;
            _progressSubject.OnNext(percentDone);
        }

        _progressSubject.OnNext(startProgress + sumProgress);
    }

    private static Func<Vector3, float> GetSceneDel(ShaderSceneType sceneType)
    {
        switch (sceneType)
        {
            case ShaderSceneType.Sphere:
                return EstimateDistanceSphere;
            case ShaderSceneType.Box:
                return EstimateDistanceBox2;
            case ShaderSceneType.Torus:
                return EstimateDistanceTorus;
            case ShaderSceneType.Capsule:
                return EstimateDistanceCapsule;
            case ShaderSceneType.Composite:
                return EstimateDistanceComposite;
            default:
                throw new ArgumentException("Unknown Scene Type");
        }
    }

    public async Task<FractalResult> CreateShaderAsync(FractalParams fractalParams, double startProgress, double sumProgress, CancellationToken cancelToken)
    {
        _fractalParams = fractalParams;
        var size = fractalParams.ImageSize;
        var raw = new RawLightedImage(size.Width, size.Height, fractalParams.Palette.NumberOfColors);
        raw.LightingOnZeroIndex = fractalParams.LightingOnZeroIndex;

        _distanceEstimator = GetSceneDel(fractalParams.SceneType);

        if (cancelToken.IsCancellationRequested)
            return new FractalResult();

        await Task.Run(() => CalculateImageNew(raw, startProgress, sumProgress, cancelToken), cancelToken);

        if (cancelToken.IsCancellationRequested)
            return new FractalResult();

        return new FractalResult()
        {
            Params = (FractalParams)fractalParams.Clone(),
            Image = raw
        };
    }
}

