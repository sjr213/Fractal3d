namespace ImageCalculator;

using FractureCommonLib;
using System.Numerics;
using System.Reactive.Subjects;

public class ShaderFactory : IDisposable
{
    // p is the point being tested
 //   public delegate float SceneDel(Vector3 p);

    private FractalParams _fractalParams = new();
    private readonly Subject<int> _progressSubject = new();
    public IObservable<int> Progress => _progressSubject;
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
        Vector3 box = new Vector3(0.4f, 0.2f, 0.1f);
        Vector3 q = Vector3.Abs(p) - box;
        return Vector3.Max(q, Vector3.Zero).Length() + Math.Min(Math.Max(Math.Max(q.Y, q.Z), q.X), 0.0f);
    }

    private Tuple<bool, float> RayMarch(Vector3 startPt, Vector3 direction, out Vector3 pt)
    {
        float totalDistance = 0.0f;
        int steps;
        bool hit = false;

        float lastDistance = float.MaxValue;
        pt = startPt;

        for (steps = 0; steps < _fractalParams.MaxRaySteps; steps++)
        {
            pt = totalDistance * direction + startPt;
            float distance = _distanceEstimator(pt);
            totalDistance += distance / _fractalParams.StepDivisor;
            if (distance < _fractalParams.MinRayDistance)
            {
                hit = true;
                break;
            }

            if (distance > lastDistance)        // Can have option to block distance increases
                break;

            if (totalDistance > _fractalParams.MaxDistance)
                break;

            lastDistance = distance;
        }

        return new Tuple<bool, float>(hit, 1.0f - ((float)steps) / _fractalParams.MaxRaySteps);
    }


    private void CalculateImageNew(RawLightedImage raw, FractalParams fractalParams, CancellationToken cancelToken)
    {
        _progressSubject.OnNext(0);

        var size = fractalParams.ImageSize;
        var palette = fractalParams.Palette;
        var viewPos = new Vector3(0, 0, -1.0f);

        float left = Math.Min(_fractalParams.FromX, _fractalParams.ToX);
        float right = Math.Max(_fractalParams.FromX, _fractalParams.ToX);
        float bottom = Math.Min(_fractalParams.FromY, _fractalParams.ToY);
        float top = Math.Max(_fractalParams.FromY, _fractalParams.ToY);

        float fromZ = _fractalParams.FromZ;
        float targetZ = 0.0f;

        float xRange = (right - left) / size.Width;
        float yRange = (top - bottom) / size.Height;

        for (int x = 0; x < size.Width; ++x)
        {
            for (int y = 0; y < size.Height; ++y)
            {
                float fx = x * xRange + left;
                float fy = y * yRange + bottom;

                Vector3 from = new Vector3(fx, fy, fromZ);

                Vector3 to = (_fractalParams.AimToOrigin) ? new Vector3(0.0f, 0.0f, targetZ) : new Vector3(fx, fy, targetZ);

                Vector3 startPt = from * fractalParams.Distance + to;

                Vector3 direction = -1.0f * (to - from);

                var tuple = RayMarch(startPt, direction, out var outPt);
                bool hit = tuple.Item1;
                float distance = tuple.Item2;

                if (distance < 0.0f)
                    distance = 0.0f;

                if (distance > 1.0f)
                    distance = 1.0f;

                var normal = NormalCalculator.CalculateNormal(_distanceEstimator, fractalParams.NormalDistance, outPt);

                var lighting = (hit) ? LightUtil.GetPointLight(fractalParams.Lights, fractalParams.LightComboMode, outPt, viewPos, normal) : 
                    new Lighting();

                int depth = (int)(distance * (palette.NumberOfColors - 1));

                // need a new raw image that stores Vector3
                var light = lighting.Diffuse + lighting.Specular;
                raw.SetPixel(x, y, depth, light);
            }

            if (cancelToken.IsCancellationRequested)
                return;

            int percentDone = (int)100.0 * x / size.Width;
            _progressSubject.OnNext(percentDone);
        }

        _progressSubject.OnNext(100);
    }

    private static Func<Vector3, float> GetSceneDel(ShaderSceneType sceneType)
    {
        switch (sceneType)
        {
            case ShaderSceneType.Sphere:
                return EstimateDistanceSphere;
            case ShaderSceneType.Box:
                return EstimateDistanceBox;
            default:
                throw new ArgumentException("Unknown Scene Type");
        }
    }

    public async Task<FractalResult> CreateShaderAsync(FractalParams fractalParams, CancellationToken cancelToken)
    {
        _fractalParams = fractalParams;
        var size = fractalParams.ImageSize;
        var raw = new RawLightedImage(size.Width, size.Height, fractalParams.Palette.NumberOfColors);

        _distanceEstimator = GetSceneDel(fractalParams.SceneType);

        if (cancelToken.IsCancellationRequested)
            return new FractalResult();

        await Task.Run(() => CalculateImageNew(raw, fractalParams, cancelToken), cancelToken);

        if (cancelToken.IsCancellationRequested)
            return new FractalResult();

        return new FractalResult()
        {
            Params = (FractalParams)fractalParams.Clone(),
            Image = raw
        };
    }
}

