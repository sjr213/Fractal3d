namespace ImageCalculator;

using FractureCommonLib;
using System.Numerics;
using System.Reactive.Subjects;

internal record ProcessedBranch(float RelativePosition, float Attenuation, Matrix4x4 RotationMatrix);


public class LsystemShaderFactory : IDisposable
{
    private FractalParams _fractalParams = new(FractalParams.MakeLights());
    private readonly Subject<double> _progressSubject = new();
    public IObservable<double> Progress => _progressSubject;
    private List<ProcessedBranch> _processedBranches = new();
    private Func<Vector3, float> _distanceEstimator = (Vector3 p) => EstimateDistanceComposite(p, 0, 0.0f, new List<ProcessedBranch>());
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

    internal static List<ProcessedBranch> ProcessBranches(List<LSystemBranch> branches)
    {
        var processedBranches = new List<ProcessedBranch>();
        foreach (var branch in branches)
        {
            var rotationMatrix = TransformationCalculator.CreateRotationMatrix(branch.AngleXDegrees, branch.AngleYDegrees, branch.AngleZDegrees);
            processedBranches.Add(new ProcessedBranch(branch.RelativePostionX, branch.Attenuation, rotationMatrix));
        }
        return processedBranches;
    }

    public static Vector3 RotateEnd2(Vector3 start, Vector3 end, Matrix4x4 mat)
    {
        var dif = end - start;
        var inter = TransformationCalculator.Transform(mat, dif);
        return inter + start;
    }

    record struct PointPair(Vector3 Start, Vector3 End, float Length);

    private static Vector3 GetNewEnd(Vector3 start, Vector3 end, Matrix4x4 mat, float relativeLength)
    {
        var v = (end - start) * relativeLength;
        var newEnd = end + v;
        return RotateEnd2(end, newEnd, mat);
    }

    internal static float EstimateDistanceComposite(Vector3 p, int iterations, float radius, List<ProcessedBranch> processedBranches)
    {
        var start = new Vector3(0.0f, 0.9f, 0.0f);
        var end = new Vector3(0.0f, 0.2f, 0.0f);

        var length = (end - start).Length();
        var pts = new List<PointPair>
        {
            new PointPair(start, end, length)
        };
        
        float distance = float.MaxValue;

        // NEED TO FIX LENGTH AND ATTENUATION
        for (int i = 0; i < iterations; i++)
        {
            var pts2 = new List<PointPair>();

            foreach (var pt in pts.ToList())
            {
                distance = Math.Min(distance, sdCapsule(p, pt.Start, pt.End, radius));

                foreach(var branch in processedBranches)
                {
                    var newLength = pt.Length * branch.Attenuation;
                    var newEnd = GetNewEnd(pt.Start, pt.End, branch.RotationMatrix, newLength);
                    pts2.Add(new PointPair(pt.End, newEnd, newLength));
                }
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
        _distanceEstimator = (Vector3 p) => EstimateDistanceComposite(p, _fractalParams.Iterations, _fractalParams.LSystemRadius, _processedBranches);

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

    public async Task<FractalResult> CreateShaderAsync(FractalParams fractalParams, double startProgress, double sumProgress, CancellationToken cancelToken)
    {
        _fractalParams = fractalParams;
        var size = fractalParams.ImageSize;
        var raw = new RawLightedImage(size.Width, size.Height, fractalParams.Palette.NumberOfColors);
        raw.LightingOnZeroIndex = fractalParams.LightingOnZeroIndex;

        if (cancelToken.IsCancellationRequested)
            return new FractalResult();

        _processedBranches = ProcessBranches(fractalParams.LSystemBranches);

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
