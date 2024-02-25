using FractureCommonLib;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Reactive.Subjects;

namespace ImageCalculator;

public class ParallelCraneShaderFactory : IDisposable
{
    private readonly Subject<double> _progressSubject = new();
    private static double _totalProgress;
    private readonly object _lockObject = new();

    public IObservable<double> Progress => _progressSubject;
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



    private void CalculateImageNew(PixelContainer raw, FractalParams fractalParams, CancellationToken cancelToken, double progress)
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

        for (var x = raw.FromWidth; x <= raw.ToWidth; ++x)
        {
            for (var y = 0; y < size.Height; ++y)
            {
                var fx = x * xRange + left;
                var fy = y * yRange + bottom;

                var from = new Vector3(fx, fy, fromZ);

                var to = (fractalParams.AimToOrigin) ? new Vector3(0.0f, 0.0f, toZ) : new Vector3(fx, fy, toZ);

                var startPt = from + fractalParams.Distance * to;

                var direction = to - from;

                //              var distance = RayMarch(fractalParams, startPt, direction, transformMatrix, out var outPt);
                float epsilon = 0.01f;
                // This doesn't take into account the transformation matrix
                var distance = QuatMath2.IntersectQJulia(ref startPt, direction, fractalParams.C4, fractalParams.Iterations, epsilon);

                if (distance < 0.0f)
                    distance = 0.0f;

                if (distance > epsilon)
                    distance = epsilon;

                var transformedPt = TransformationCalculator.Transform(transformMatrix, startPt);
                var normal = QuatMath2.NormEstimate(startPt, fractalParams.C4, fractalParams.Iterations);
                var lighting = LightUtil.GetPointLight(transformedLights, fractalParams.LightComboMode, transformedPt, transViewPos, normal);

                //          var transformedPt = TransformationCalculator.Transform(transformMatrix, outPt);
                //          float DistanceEstimator(Vector3 pt) => EstimateDistance(pt, fractalParams);
                //          var normal = NormalCalculator.CalculateNormal(DistanceEstimator, fractalParams.NormalDistance, transformedPt);
                //         var lighting = LightUtil.GetPointLight(transformedLights, fractalParams.LightComboMode, transformedPt, transViewPos, normal);

                var depth = (int)(distance/epsilon * (palette.NumberOfColors - 1));

                // need a new raw image that stores Vector3
                var light = lighting.Diffuse + lighting.Specular;
                raw.SetPixel(x, y, depth, light);
            }

            if (cancelToken.IsCancellationRequested)
                return;
        }

        lock (_lockObject)
        {
            _totalProgress += progress;
        }
        _progressSubject.OnNext(_totalProgress);
    }

    public static IList<PixelContainer> CreateContainers(Size size, int depth, int numberOfContainers)
    {
        var containers = new ConcurrentBag<PixelContainer>();

        while (size.Width / numberOfContainers < 3)
        {
            numberOfContainers--;
        }

        if (numberOfContainers == 0)
            numberOfContainers = 1;

        int containerWidth = size.Width / numberOfContainers;

        for (int i = 0; i < numberOfContainers; ++i)
        {
            int fromWidth = i * containerWidth;
            if (i == numberOfContainers - 1)
            {
                containers.Add(new PixelContainer(fromWidth, size.Width - 1, size.Height, depth));
            }
            else
            {
                containers.Add(new PixelContainer(fromWidth, fromWidth + containerWidth - 1, size.Height, depth));
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

            raw.SetBlock(pixels, lighting, container.FromWidth, container.ToWidth, container.Height, container.Depth);
        }

        return raw;
    }

    public async Task<FractalResult> CreateFractalAsync(FractalParams fractalParams, double startProgress, double sumProgress, CancellationToken cancelToken)
    {
        var watch = Stopwatch.StartNew();
        var size = fractalParams.ImageSize;
        _totalProgress = startProgress;

        if (cancelToken.IsCancellationRequested)
            return new FractalResult();

        _progressSubject.OnNext(startProgress);

        int numberOfContainers = size.Width / 40;
        var containers = CreateContainers(size, fractalParams.Palette.NumberOfColors, numberOfContainers);
        var fractionProgress = sumProgress / numberOfContainers;

        await Task.Run(() => Parallel.ForEach(containers,
            container => CalculateImageNew(container, fractalParams, cancelToken, fractionProgress)),
            cancelToken);

        if (cancelToken.IsCancellationRequested)
            return new FractalResult();

        _progressSubject.OnNext(startProgress + sumProgress);

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
