﻿using FractureCommonLib;
using System.Diagnostics;
using System.Numerics;
using System.Reactive.Subjects;
using static ImageCalculator.ParallelFractalFactory;
using static ImageCalculator.QuatMath2;


namespace ImageCalculator
{
    public class ParallelCraneRayMarchFactory : IDisposable
    {
        private readonly Subject<double> _progressSubject = new();
        private static double _totalProgress;
        private readonly object _lockObject = new();
        private CalculationIntersectionDelegate _nextCycle = IterateIntersectionSquared;
        private NormEstimateDelegate _normEstimate = NormEstimateSquared;

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

        RawLightedImage CombineContainers(FractalParams fractalParams, IList<PixelContainer> containers)
        {
            var size = fractalParams.ImageSize;
            var raw = new RawLightedImage(size.Width, size.Height, fractalParams.Palette.NumberOfColors);
            raw.LightingOnZeroIndex = fractalParams.LightingOnZeroIndex;

            foreach (var container in containers)
            {
                var pixels = container.PixelValues;
                var lighting = container.Lighting;

                raw.SetBlock(pixels, lighting, container.FromWidth, container.ToWidth, container.Height, container.Depth);
            }

            return raw;
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

                    var transformedPt = TransformationCalculator.Transform(transformMatrix, startPt);
                    var transformedDir = TransformationCalculator.Transform(transformMatrix, direction);

                    transformedPt = IntersectSphere(transformedPt, transformedDir, fractalParams.Bailout);

                    var distance = RayMarchQJulia(ref transformedPt, transformedDir, fractalParams, _nextCycle);

                    if(float.IsNaN(distance))
                        distance = 0.0f;
                    
                    distance = (distance - fractalParams.MinStretchDistance) / (fractalParams.MaxStretchDistance - fractalParams.MinStretchDistance);

                    if (distance < 0.0f)
                        distance = 0.0f;

                    if (distance > 1.0f)
                        distance = 1.0f;

                    Vector3 normal = _normEstimate(transformedPt, fractalParams.C4, fractalParams.Iterations, fractalParams.NormalDistance);

                    var lighting = LightUtil.GetPointLight(transformedLights, fractalParams.LightComboMode, transformedPt, transViewPos, normal);

                    var depth = (int)(distance * (palette.NumberOfColors - 1));

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

        public async Task<FractalResult> CreateFractalAsync(FractalParams fractalParams, double startProgress, double sumProgress, CancellationToken cancelToken)
        {
            var watch = Stopwatch.StartNew();
            var size = fractalParams.ImageSize;
            _totalProgress = startProgress;

            _nextCycle = GetCalculationDelegate(fractalParams.QuatEquation);
            _normEstimate = GetNormEstimateDelegate(fractalParams.QuatEquation);

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
}
