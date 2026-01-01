using FractureCommonLib;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Reactive.Subjects;
using static ImageCalculator.QuatMath2;
using static ImageCalculator.QuatMathShadertoy;

namespace ImageCalculator;

public class ParallelShadertoyFactory : IDisposable
{
    private readonly Subject<double> _progressSubject = new();
    private static double _totalProgress;
    private readonly object _lockObject = new();

    public IObservable<double> Progress => _progressSubject;
    bool _isDisposed;

    private MapDelegate _nextCycle = MapSq;

    // private NormEstimateDelegate _normEstimate = NormEstimateSquared;

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

    private static Color ConvertVectorToColor(Vector3 c, int a)
    {
        c = Vector3.Clamp(c, Vector3.Zero, Vector3.One);

        return Color.FromArgb(a, (int)(c.X * 255), (int)(c.Y * 255), (int)(c.Z * 255));
    }

    private static Color ShadeColor(Color color, float shade)
    {
        var r = (int)(color.R * shade);
        var g = (int)(color.G * shade);
        var b = (int)(color.B * shade);

        return Color.FromArgb(color.A, r, g, b);
    }

    private void CalculateImageNew(ColorContainer raw, FractalParams fractalParams, double progress, CancellationToken cancelToken)
    {
        var size = fractalParams.ImageSize;

        var left = Math.Min(fractalParams.FromX, fractalParams.ToX);
        var right = Math.Max(fractalParams.FromX, fractalParams.ToX);
        var bottom = Math.Min(fractalParams.FromY, fractalParams.ToY);
        var top = Math.Max(fractalParams.FromY, fractalParams.ToY);

        var fromZ = fractalParams.FromZ;
        var toZ = fractalParams.ToZ;

        var xRange = (right - left) / size.Width;
        var yRange = (top - bottom) / size.Height;

        Color activeColor;

        var transformMatrix = TransformationCalculator.CreateInvertedTransformationMatrix(fractalParams.TransformParams);
        var transformedLights = LightUtil.TransformLights(fractalParams.Lights, transformMatrix);

        for (var x = raw.FromWidth; x <= raw.ToWidth; ++x)
        {
            for (var y = 0; y < size.Height; ++y)
            {
                activeColor = fractalParams.BackgroundColor;
                var fx = x * xRange + left;
                var fy = y * yRange + bottom;

                var from = new Vector3(fx, fy, fromZ);

                var to = fractalParams.AimToOrigin ? new Vector3(0.0f, 0.0f, toZ) : new Vector3(fx, fy, toZ);

                var startPt = from + fractalParams.Distance * to;

                var direction = to - from;
                direction = Vector3.Normalize(direction);

                var transformedPt = TransformationCalculator.Transform(transformMatrix, startPt);
                var transformedDir = TransformationCalculator.Transform(transformMatrix, direction);
                transformedPt = IntersectSphere(transformedPt, transformedDir, fractalParams.Bailout);

                var distance = IntersectQJuliaST(ref transformedPt, transformedDir, fractalParams, _nextCycle);

                if (distance < fractalParams.MinRayDistance)
                {
                    Vector3 normal = CalcNormal3(transformedPt, fractalParams.C4, fractalParams.Iterations, fractalParams.EscapeThreshold);
                    Vector3 partialColor = GetPhongLightsExpanded(transformedLights, fractalParams.LightComboMode, transformedDir, transformedPt, normal);
                    activeColor = ConvertVectorToColor(partialColor, 255);
                }

                raw.SetColor(x, y, activeColor);
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

    private static IList<ColorContainer> CreateContainers(Size size, Color background, int numberOfContainers)
    {
        var containers = new ConcurrentBag<ColorContainer>();

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
                containers.Add(new ColorContainer(fromWidth, size.Width - 1, size.Height, background));
            }
            else
            {
                containers.Add(new ColorContainer(fromWidth, fromWidth + containerWidth - 1, size.Height, background));
            }
        }

        return containers.ToList();
    }

    private static RawLightedImage CombineContainers(FractalParams fractalParams, IList<ColorContainer> containers)
    {
        var size = fractalParams.ImageSize;
        var raw = new RawLightedImage(size.Width, size.Height);

        foreach (var container in containers)
        {
            var colors = container.ColorValues;

            raw.SetBlock(colors, container.FromWidth, container.ToWidth, container.Height);
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

        _nextCycle = GetMapDelegate(fractalParams.QuatEquation);

        _progressSubject.OnNext(startProgress);

        int numberOfContainers = size.Width / 40;
        var containers = CreateContainers(size, fractalParams.BackgroundColor, numberOfContainers);
        var fractionProgress = sumProgress / numberOfContainers;

        await Task.Run(() => Parallel.ForEach(containers,
            container => CalculateImageNew(container, fractalParams, fractionProgress, cancelToken)),
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
