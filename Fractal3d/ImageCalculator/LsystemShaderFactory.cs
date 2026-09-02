namespace ImageCalculator;

using FractureCommonLib;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Reactive.Subjects;

internal record ProcessedBranch(float RelativePosition, float Attenuation, Matrix4x4 RotationMatrix);


public class LsystemShaderFactory : IDisposable
{
    private FractalParams _fractalParams = new(FractalParams.MakeLights());
    private static double _totalProgress;
    private readonly Lock _lockObject = new();
    private readonly Subject<double> _progressSubject = new();
    public IObservable<double> Progress => _progressSubject;
    private List<ProcessedBranch> _processedBranches = new();
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

    record struct PointPair(Vector3 Start, Vector3 End, float Length);

    private static Vector3 GetNewEnd(Vector3 start, Vector3 end, Matrix4x4 mat, float relativeLength, Vector3 branchEnd)
    {
        var v = (end - start) * relativeLength;
        var newEnd = branchEnd + v;

        //return RotateEnd2(branchEnd, newEnd, mat);
        var inter = Vector3.Transform((newEnd - branchEnd), mat);
        return inter + branchEnd;
    }

    internal float EstimateDistanceComposite(Vector3 p)
    {
        var start = new Vector3(0.0f, 0.9f, 0.0f);
        var end = new Vector3(0.0f, 0.2f, 0.0f);

        var length = (end - start).Length();

        // Use capacity hints to reduce reallocations
        int estimatedCapacity = (int)Math.Pow(_processedBranches.Count, Math.Min(_fractalParams.Iterations, 5));
        var pts = new List<PointPair>(estimatedCapacity)
        {
            new PointPair(start, end, length)
        };
        
        float distance = float.MaxValue;

        // NEED TO FIX LENGTH AND ATTENUATION
        for (int i = 0; i < _fractalParams.Iterations; i++)
        {
            // Reuse capacity from previous iteration
            var pts2 = new List<PointPair>(pts.Count * _processedBranches.Count);

            foreach (var pt in pts)
            {
                distance = Math.Min(distance, sdCapsule(p, pt.Start, pt.End, _fractalParams.LSystemRadius));
                if(distance < _fractalParams.MinRayDistance)
                    return distance;

                foreach (var branch in _processedBranches)
                {
                    var newLength = pt.Length * branch.Attenuation;
                    var branchStart = (pt.End - pt.Start) * branch.RelativePosition + pt.Start;
                   
                    var newEnd = GetNewEnd(pt.Start, pt.End, branch.RotationMatrix, newLength, branchStart);
                    pts2.Add(new PointPair(branchStart, newEnd, newLength));
                }
            }

            pts = pts2;

            // Safety check: prevent memory explosion
            if (pts.Count > 100000)
            {
                break; // Or implement LOD reduction
            }
        }
        return distance;
    }

    // Capsule / Line
    public static float sdCapsule(Vector3 p, Vector3 a, Vector3 b, float r)
    {
        Vector3 pa = p - a;
        Vector3 ba = b - a;
        //        float h = (float)Math.Clamp(Vector3.Dot(pa, ba) / Vector3.Dot(ba, ba), 0.0, 1.0);
        float baLengthSqInv = 1f / Vector3.Dot(ba, ba);
        float h = Math.Clamp(Vector3.Dot(pa, ba) * baLengthSqInv, 0f, 1f);
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
            float distance = EstimateDistanceComposite(transformedPt);

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

    RawLightedImage CombineContainers(IList<PixelContainer> containers)
    {
        var size = _fractalParams.ImageSize;
        var raw = new RawLightedImage(size.Width, size.Height, _fractalParams.Palette.NumberOfColors);
        raw.LightingOnZeroIndex = _fractalParams.LightingOnZeroIndex;

        foreach (var container in containers)
        {
            var pixels = container.PixelValues;
            var lighting = container.Lighting;

            raw.SetBlock(pixels, lighting, container.FromWidth, container.ToWidth, container.Height, container.Depth);
        }

        return raw;
    }


    private void CalculateImageNew(PixelContainer raw, CancellationToken cancelToken, double progress)
    {
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
      //  _distanceEstimator = (Vector3 p) => EstimateDistanceComposite(p);

        for (var x = raw.FromWidth; x <= raw.ToWidth; ++x)
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

                Func<Vector3, float> distanceEstimator = (Vector3 p) => this.EstimateDistanceComposite(p);
                var normal = NormalCalculator.CalculateNormal(distanceEstimator, _fractalParams.NormalDistance, transformedPt);

                var lighting = (hit) ? LightUtil.GetPointLight(transformedLights, _fractalParams.LightComboMode, transformedPt, transViewPos, normal) :
                    new Lighting();

                var depth = (hit) ? (palette.NumberOfColors - 1) : 0;

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

    public async Task<FractalResult> CreateShaderAsync(FractalParams fractalParams, double startProgress, double sumProgress, CancellationToken cancelToken)
    {
        var watch = Stopwatch.StartNew();
        var size = fractalParams.ImageSize;
        _totalProgress = startProgress;
        _fractalParams = fractalParams;

        if (cancelToken.IsCancellationRequested)
            return new FractalResult();

        _progressSubject.OnNext(startProgress);

        int numberOfContainers = size.Width / 40;
        var containers = CreateContainers(size, fractalParams.Palette.NumberOfColors, numberOfContainers);
        var fractionProgress = sumProgress / numberOfContainers;

        _processedBranches = ProcessBranches(fractalParams.LSystemBranches);

        await Task.Run(() => Parallel.ForEach(containers,
            container => CalculateImageNew(container, cancelToken, fractionProgress)),
            cancelToken);

        if (cancelToken.IsCancellationRequested)
            return new FractalResult();

        _progressSubject.OnNext(startProgress + sumProgress);

        var raw = CombineContainers(containers);

        watch.Stop();

        return new FractalResult()
        {
            Params = (FractalParams)fractalParams.Clone(),
            Image = raw,
            Time = watch.ElapsedMilliseconds
        };
    }
}
