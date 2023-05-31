using FractureCommonLib;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Reactive.Subjects;

namespace ImageCalculator;

public class ParallelFractalFactory
{
    public delegate void CalculationDelegate(Vector4 q, Vector4 c, out Vector4 q1, out Vector4 dQ1);

    private CalculationDelegate _nextCycle = QuadMath.CalculateNextCycleSquared;
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

        if (disposing)
        {
            _progressSubject.Dispose();
        }

        _isDisposed = true;
    }

    public ParallelFractalFactory()
    {}

    private static CalculationDelegate GetCalculationDelegate(QuaternionEquationType equationType)
    {
        switch (equationType)
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

    private float EstimateDistance(Vector3 pt, FractalParams fractalParams)
    {
        var z0 = new Vector4(pt, 0.2f);
        var z = new Vector4(pt, 0.2f);
        float dr = 1.0f;
        float r = z.Length();

        for (int i = 0; i < fractalParams.Iterations; ++i)
        {
            if (r > fractalParams.Bailout)
                break;
            _nextCycle(z, fractalParams.C4, out var z1, out var dz1);

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

    private float RayMarch(FractalParams fractalParams, Vector3 startPt, Vector3 direction, Matrix4x4 transformMatrix, out Vector3 pt)
    {
        float totalDistance = 0.0f;
        int steps;

        float lastDistance = float.MaxValue;
        pt = startPt;

        for (steps = 0; steps < fractalParams.MaxRaySteps; steps++)
        {
            pt = totalDistance * direction + startPt;
            var transformedPt = TransformationCalculator.Transform(transformMatrix, pt);
            float distance = EstimateDistance(transformedPt, fractalParams);
            totalDistance += distance / fractalParams.StepDivisor;
            if (distance < fractalParams.MinRayDistance)
                break;

            if (distance > lastDistance)        // Can have option to block distance increases
                break;

            if (totalDistance > fractalParams.MaxDistance)
                break;

            lastDistance = distance;
        }

        return 1.0f - ((float)steps) / fractalParams.MaxRaySteps;
    }

    private void CalculateImageNew(PixelContainer raw, FractalParams fractalParams, CancellationToken cancelToken)
    {
        var size = fractalParams.ImageSize;
        var palette = fractalParams.Palette;

        var left = Math.Min(fractalParams.FromX, fractalParams.ToX);
        var right = Math.Max(fractalParams.FromX, fractalParams.ToX);
        var bottom = Math.Min(fractalParams.FromY, fractalParams.ToY);
        var top = Math.Max(fractalParams.FromY, fractalParams.ToY);

        var fromZ = fractalParams.FromZ;
        var toZ = fractalParams.ToZ;

        var xRange = (right - left) / size.Width;
        var yRange = (top - bottom) / size.Height;

        var viewPos = new Vector3(0, 0, fromZ);

        var transformMatrix = TransformationCalculator.CreateInvertedTransformationMatrix(fractalParams.TransformParams);
        var transformedLights = LightUtil.TransformLights(fractalParams.Lights, transformMatrix);
        var transViewPos = TransformationCalculator.Transform(transformMatrix, viewPos);

        for (var x = 0; x < size.Width; ++x)
        {
            for (var y = raw.FromHeight; y <= raw.ToHeight; ++y)
            {
                var fx = x * xRange + left;
                var fy = y * yRange + bottom;

                var from = new Vector3(fx, fy, fromZ);

                var to = (fractalParams.AimToOrigin) ? new Vector3(0.0f, 0.0f, toZ) : new Vector3(fx, fy, toZ);

                var startPt = from + fractalParams.Distance * to;

                var direction = to - from;

                var distance = RayMarch(fractalParams, startPt, direction, transformMatrix, out var outPt);

                if (distance < 0.0f)
                    distance = 0.0f;

                if (distance > 1.0f)
                    distance = 1.0f;

                var transformedPt = TransformationCalculator.Transform(transformMatrix, outPt);
                float DistanceEstimator(Vector3 pt) => EstimateDistance(pt, fractalParams);
                var normal = NormalCalculator.CalculateNormal(DistanceEstimator, fractalParams.NormalDistance, transformedPt);
                var lighting = LightUtil.GetPointLight(transformedLights, fractalParams.LightComboMode, transformedPt, transViewPos, normal);

                var depth = (int)(distance * (palette.NumberOfColors - 1));

                // need a new raw image that stores Vector3
                var light = lighting.Diffuse + lighting.Specular;
                raw.SetPixel(x, y, depth, light);
            }

            if (cancelToken.IsCancellationRequested)
                return;
        }

    }

    public static IList<PixelContainer> CreateContainers(Size size, int depth, int numberOfContainers)
    {
        var containers = new ConcurrentBag<PixelContainer>();

        while (size.Height / numberOfContainers < 3)
        {
            numberOfContainers--;
        }

        if (numberOfContainers == 0)
            numberOfContainers = 1;

        int containerHt = size.Height / numberOfContainers;

        for (int i = 0; i < numberOfContainers; ++i)
        {
            int fromHt = i * containerHt;
            if (i == numberOfContainers - 1)
            {
                containers.Add(new PixelContainer(size.Width, fromHt, size.Height - 1, depth));
            }
            else
            {
                containers.Add(new PixelContainer(size.Width, fromHt, fromHt + containerHt - 1, depth));
            }
        }

        return containers.ToList();
    }

    RawLightedImage CombineContainers(FractalParams fractalParams, IList<PixelContainer> containers)
    {
        var size = fractalParams.ImageSize;
        var raw = new RawLightedImage(size.Width, size.Height, fractalParams.Palette.NumberOfColors);

        foreach (var container in containers)
        {
            var pixels = container.PixelValues;
            var lighting = container.Lighting;

            // see if there is a way to copy the arrays more efficiently
            raw.SetBlock(pixels, lighting, container.Width, container.FromHeight, container.ToHeight, container.Depth);
        }

        return raw;
    }

    public async Task<FractalResult> CreateFractalAsync(FractalParams fractalParams, CancellationToken cancelToken)
    {
        var watch = Stopwatch.StartNew();
        var size = fractalParams.ImageSize;
        
        _nextCycle = GetCalculationDelegate(fractalParams.QuatEquation);

        if (cancelToken.IsCancellationRequested)
            return new FractalResult();

        var containers = CreateContainers(size, fractalParams.Palette.NumberOfColors, size.Height/40);

        await Task.Run(() => Parallel.ForEach(containers, container => CalculateImageNew(container, fractalParams, cancelToken)), cancelToken);

        if (cancelToken.IsCancellationRequested)
            return new FractalResult();

        var raw = CombineContainers(fractalParams, containers);

        watch.Stop();

        return new FractalResult()
        {
            Params = (FractalParams)fractalParams.Clone(),
            Image = raw,
            Time = watch.ElapsedMilliseconds
        };
    }

}

