namespace ImageCalculator;

using FractureCommonLib;
using System.Net.Http.Headers;
using System.Numerics;
using System.Reactive;
using System.Reactive.Subjects;

public class FractalFactory : IDisposable
{
    public delegate void QuatCalcDel(Vector4 Q, Vector4 C, out Vector4 Q1, out Vector4 dQ1);
     
    private FractalParams _fractalParams = new FractalParams();
    private QuatCalcDel _nextCycle = QuadMath.CalculateNextCycleSquared;
    private Subject<int> _progressSubject = new Subject<int>();

    public IObservable<int> Progress => _progressSubject;

    bool _isDisposed = false;

    public void Dispose()
    {
        Dispose(true);

        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed)
            return;

        if(disposing)
        {
            _progressSubject.Dispose();
        }

        _isDisposed = true;
    }

    private float EstimateDistance(Vector3 pt)
    {
        var z0 = new Vector4(pt, 0.2f);
        var z = new Vector4(pt, 0.2f);
        float dr = 1.0f;
        float r = z.Length();

        for (int i = 0; i < _fractalParams.Iterations; ++i)
        {
            if (r > _fractalParams.Bailout)
                break;
            Vector4 dz1;
            Vector4 z1;
            _nextCycle(z, _fractalParams.C4, out z1, out dz1);

            if (NumericExtensions.IsNan(z1))
                break;

            r = z1.Length();
            dr = dz1.Length();

            z = z1;
            z += z0;
        }

        if (r == 0.0 || dr == 0.0)
            return 0.0f;

        return ((float)Math.Log(r) * r) / dr;
    }

    private float RayMarch(Vector3 startPt, Vector3 direction, out Vector3 pt)
    {
        float totalDistance = 0.0f;
        int steps;

        float lastDistance = float.MaxValue;
        pt = startPt;

        for (steps = 0; steps < _fractalParams.MaxRaySteps; steps++)
        {
            pt = totalDistance * direction + startPt;
            float distance = EstimateDistance(pt);
            totalDistance += distance / _fractalParams.StepDivisor;               
            if (distance < _fractalParams.MinRayDistance)
                break;

            if (distance > lastDistance)        // Can have option to block distance increases
                break;

            if (totalDistance > _fractalParams.MaxDistance)
                break;

            lastDistance = distance;
        }

        return 1.0f - ((float)steps) / _fractalParams.MaxRaySteps;
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

        float z = -1.0f;
        float targetZ = 0.0f;

        float xRange = (right - left) / size.Width;
        float yRange = (top - bottom) / size.Height;

        for (int x = 0; x < size.Width; ++x)
        {
            for (int y = 0; y < size.Height; ++y)
            {
                float fx = x * xRange + left;
                float fy = y * yRange + bottom;

                Vector3 from = new Vector3(fx, fy, z);

                Vector3 to = (_fractalParams.AimToOrigin) ? new Vector3(0.0f, 0.0f, targetZ): new Vector3(fx, fy, targetZ);
                
                Vector3 startPt = from * fractalParams.Distance + to;
                Vector3 outPt = new Vector3();

                Vector3 direction = -1.0f * (to - from);

                float distance = RayMarch(startPt, direction, out outPt);

                if (distance < 0.0f)
                    distance = 0.0f;

                if (distance > 1.0f)
                    distance = 1.0f;

                var normal = NormalCalculator.CalculateNormal(EstimateDistance, fractalParams.NormalDistance, outPt);
                Lighting lighting = LightUtil.GetPointLight(fractalParams.Lights[0], outPt, viewPos, normal);

                int depth = (int)(distance * (palette.NumberOfColors-1));

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

    private static QuatCalcDel GetQuatCalcDel(QuaternionEquationType equationType)
    {
        switch(equationType)
        {
            case QuaternionEquationType.Q_Squared:
                return QuadMath.CalculateNextCycleSquared;
            case QuaternionEquationType.Q_Cubed:
                return QuadMath.CalculateNextCycleCubed;
            case QuaternionEquationType.Q_InglesCubed:
                return QuadMath.CalculateNextCycleInglesCubed;
            default:
                throw new ArgumentException("Unknown Quaternion equation");
        }
    }

    public async Task<FractalResult> CreateFractalAsync(FractalParams fractalParams, CancellationToken cancelToken)
    {
        _fractalParams = fractalParams;
        var size = fractalParams.ImageSize;
        var raw = new RawLightedImage(size.Width, size.Height, fractalParams.Palette.NumberOfColors);

        _nextCycle = GetQuatCalcDel(_fractalParams.QuatEquation);

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
