using System.Collections.Concurrent;
using System.Drawing;

namespace ImageCalculator;

using FractureCommonLib;
using System.Diagnostics;
using System.Numerics;
using System.Reactive.Subjects;

public class FractalFactory : IDisposable
{
    public delegate void QuatCalcDel(Vector4 q, Vector4 c, out Vector4 q1, out Vector4 dQ1);
     
    private FractalParams _fractalParams = new(FractalParams.MakeLights());
    private QuatCalcDel _nextCycle = QuadMath.CalculateNextCycleSquared;
    private readonly Subject<int> _progressSubject = new();

    public IObservable<int> Progress => _progressSubject;

    bool _isDisposed;

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
            _nextCycle(z, _fractalParams.C4, out var z1, out var dz1);

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

    private float RayMarch(Vector3 startPt, Vector3 direction, Matrix4x4 transformMatrix, out Vector3 pt)
    {
        float totalDistance = 0.0f;
        int steps;

        float lastDistance = float.MaxValue;
        pt = startPt;

        for (steps = 0; steps < _fractalParams.MaxRaySteps; steps++)
        {
            pt = totalDistance * direction + startPt;
            var transformedPt = TransformationCalculator.Transform(transformMatrix, pt);
            float distance = EstimateDistance(transformedPt);
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

        var left = Math.Min(_fractalParams.FromX, _fractalParams.ToX);
        var right = Math.Max(_fractalParams.FromX, _fractalParams.ToX);
        var bottom = Math.Min(_fractalParams.FromY, _fractalParams.ToY);
        var top = Math.Max(_fractalParams.FromY, _fractalParams.ToY);

        var fromZ = _fractalParams.FromZ;
        var toZ = _fractalParams.ToZ;

        var xRange = (right - left) / size.Width;
        var yRange = (top - bottom) / size.Height;

        var viewPos = new Vector3(0, 0, fromZ);

        var transformMatrix = TransformationCalculator.CreateInvertedTransformationMatrix(fractalParams.TransformParams);
        var transformedLights = LightUtil.TransformLights(_fractalParams.Lights, transformMatrix);
        var transViewPos = TransformationCalculator.Transform(transformMatrix, viewPos);

        for (var x = 0; x < size.Width; ++x)
        {
            for (var y = 0; y < size.Height; ++y)
            {
                var fx = x * xRange + left;
                var fy = y * yRange + bottom;

                var from = new Vector3(fx, fy, fromZ);

                var to = (_fractalParams.AimToOrigin) ? new Vector3(0.0f, 0.0f, toZ): new Vector3(fx, fy, toZ);
                
                var startPt = from + fractalParams.Distance * to;

                var direction = to - from;

                var distance = RayMarch(startPt, direction, transformMatrix, out var outPt);

                if (distance < 0.0f)
                    distance = 0.0f;

                if (distance > 1.0f)
                    distance = 1.0f;

                var transformedPt = TransformationCalculator.Transform(transformMatrix, outPt);
                var normal = NormalCalculator.CalculateNormal(EstimateDistance, fractalParams.NormalDistance, transformedPt);
                var lighting = LightUtil.GetPointLight(transformedLights, fractalParams.LightComboMode, transformedPt, transViewPos, normal);

                var depth = (int)(distance * (palette.NumberOfColors-1));

                // need a new raw image that stores Vector3
                var light = lighting.Diffuse + lighting.Specular;
                raw.SetPixel(x, y, depth, light);
            }

            if (cancelToken.IsCancellationRequested)
                return;

            var percentDone = (int)100.0 * x / size.Width;
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
        var watch = Stopwatch.StartNew();
        _fractalParams = fractalParams;
        var size = fractalParams.ImageSize;
        var raw = new RawLightedImage(size.Width, size.Height, fractalParams.Palette.NumberOfColors);

        _nextCycle = GetQuatCalcDel(_fractalParams.QuatEquation);

        if (cancelToken.IsCancellationRequested)
            return new FractalResult();

        await Task.Run(() => CalculateImageNew(raw, fractalParams, cancelToken), cancelToken);

        if (cancelToken.IsCancellationRequested)
            return new FractalResult();

        watch.Stop();

        return new FractalResult()
        {
            Params = (FractalParams)fractalParams.Clone(),
            Image = raw,
            Time = watch.ElapsedMilliseconds
        };
    }

}
