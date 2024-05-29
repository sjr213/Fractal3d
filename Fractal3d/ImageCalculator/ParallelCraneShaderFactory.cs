using FractureCommonLib;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Reactive.Subjects;
using static ImageCalculator.QuatMath2;

namespace ImageCalculator;

public class ParallelCraneShaderFactory : IDisposable
{
    private readonly Subject<double> _progressSubject = new();
    private static double _totalProgress;
    private readonly object _lockObject = new();

    public IObservable<double> Progress => _progressSubject;
    bool _isDisposed;

    private CalculationIntersectionDelegate _nextCycle = IterateIntersectionSquared;
    private NormEstimateDelegate _normEstimate = NormEstimateSquared;

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

    private static Color BackgroundColor()
    {
        return Color.FromArgb(0, 85, 85, 85);
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

        // float4 rgba 0.3, 0.3, 0.3, 0 to argb 0, 85, 85, 85
        Color backGroundColor = BackgroundColor();
        Color activeColor;

        var transformMatrix = TransformationCalculator.CreateInvertedTransformationMatrix(fractalParams.TransformParams);
        var transformedLights = LightUtil.TransformLights(fractalParams.Lights, transformMatrix);

        for (var x = raw.FromWidth; x <= raw.ToWidth; ++x)
        {
            for (var y = 0; y < size.Height; ++y)
            {
                activeColor = backGroundColor;
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

                // This doesn't take into account the transformation matrix
                var distance = IntersectQJulia(ref transformedPt, transformedDir, fractalParams, _nextCycle);

                if(distance < fractalParams.MinRayDistance)
                {
                    Vector3 normal = _normEstimate(transformedPt, fractalParams.C4, fractalParams.Iterations);
                    Vector3 partialColor = GetPhongLightsExpanded(transformedLights, fractalParams.LightComboMode, transformedDir, transformedPt, normal);
                    activeColor = ConvertVectorToColor(partialColor, 255);

                    if(fractalParams.RenderShadows)
                    {
                        // The shadow ray will start at the intersection point and go towards the point light.
                        // We initially move the ray origin a little bit along this direction so we don't mistakenly 
                        // find an intersection with the same point again.
                        Vector3 L = Vector3.Normalize(transformedLights[0].Position - transformedPt);
                        transformedPt += L * fractalParams.MinRayDistance * 2.0f;
                        var dist = IntersectQJulia(ref transformedPt, L, fractalParams, _nextCycle);

                        // Again, if our estimate of the distance to the set is small, we say there was a hit.
                        // In this case it means that the point is in a shadow and should be given a darker shade.
                        if(dist < fractalParams.MinRayDistance)
                        {
                            activeColor = ShadeColor(activeColor, 0.4f);
                        }
                    }
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

    private static IList<ColorContainer> CreateContainers(Size size, int depth, int numberOfContainers)
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
                containers.Add(new ColorContainer(fromWidth, size.Width - 1, size.Height, BackgroundColor()));
            }
            else
            {
                containers.Add(new ColorContainer(fromWidth, fromWidth + containerWidth - 1, size.Height, BackgroundColor()));
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

        _nextCycle = GetCalculationDelegate(fractalParams.QuatEquation);
        _normEstimate = GetNormEstimateDelegate(fractalParams.QuatEquation);

        _progressSubject.OnNext(startProgress);

        int numberOfContainers = size.Width / 40;
        var containers = CreateContainers(size, fractalParams.Palette.NumberOfColors, numberOfContainers);
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
