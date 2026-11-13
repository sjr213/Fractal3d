namespace ImageCalculator;

using FractureCommonLib;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Reactive.Subjects;

internal record ProcessedBranch(float RelativePosition, float Attenuation, Matrix4x4 RotationMatrix);

public class LsystemShaderFactory : IDisposable
{
    private FractalParams _fractalParams = new(FractalParams.MakeLights());
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
        var inter = Vector3.Transform(v, mat);
        return inter + branchEnd;
    }

    record struct Segment(Vector3 Start, Vector3 End, float Radius);

    // Segments of all levels in level order; built once per render
    private Segment[]? _segments;

    private Segment[] BuildSegments()
    {
        var start = new Vector3(0.0f, 0.9f, 0.0f);
        var end = new Vector3(0.0f, 0.2f, 0.0f);

        var length = (end - start).Length();

        var branchCount = _processedBranches.Count;
        var branchMats = new ComputeSharp.Float4x4[branchCount];
        var branchParams = new ComputeSharp.Float2[branchCount];
        for (int b = 0; b < branchCount; b++)
        {
            branchMats[b] = GpuMethods.ToFloat4x4(_processedBranches[b].RotationMatrix);
            branchParams[b] = new ComputeSharp.Float2(_processedBranches[b].RelativePosition, _processedBranches[b].Attenuation);
        }

        // xyz = start, w = length / xyz = end
        var starts = new[] { new ComputeSharp.Float4(start.X, start.Y, start.Z, length) };
        var ends = new[] { new ComputeSharp.Float4(end.X, end.Y, end.Z, 0.0f) };

        var segments = new List<Segment>();

        // NEED TO FIX LENGTH AND ATTENUATION
        for (int i = 0; i < _fractalParams.Iterations; i++)
        {
            var radius = _fractalParams.LSystemRadius / Math.Clamp(1 + i * _fractalParams.LSystemRadiusAttenuation, 0.1f, 100f);

            for (int k = 0; k < starts.Length; k++)
            {
                var s = new Vector3(starts[k].X, starts[k].Y, starts[k].Z);
                var e = new Vector3(ends[k].X, ends[k].Y, ends[k].Z);
                segments.Add(new Segment(s, e, radius));
            }

            // Children of the last level are never measured
            if (i == _fractalParams.Iterations - 1 || branchCount == 0)
                break;

            // Safety check: prevent memory explosion
            if (starts.Length * branchCount > 100000)
                break;

            GpuMethods.ExpandLevel(starts, ends, branchMats, branchParams, out starts, out ends);
        }
        return segments.ToArray();
    }

    internal float EstimateDistanceComposite(Vector3 p)
    {
        var segments = _segments ??= BuildSegments();
        var minRayDistance = _fractalParams.MinRayDistance;

        float distance = float.MaxValue;
        for (int k = 0; k < segments.Length; k++)
        {
            ref readonly var seg = ref segments[k];
            distance = Math.Min(distance, sdCapsule(p, seg.Start, seg.End, seg.Radius));
            if (distance < minRayDistance)
                return distance;
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

    private (bool, float) RayMarch(Vector3 startPt, Vector3 direction, Matrix4x4 transformMatrix, out Vector3 pt)
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

        var fraction = hit? 1.0f - ((float)steps) / _fractalParams.MaxRaySteps : 0.0f;
        return (hit, fraction);
    }

    // Only considers distance from bottom center to in and y
    private static int GetDepth2D(int x, int y, Size size, double distance, int numberOfColors)
    {
        // Arbitary scaling factor to make color spread a little more along x-axis
        var distanceX = 1.3 * Math.Abs(x-size.Width/2); // assumes x is spread evenly across the width of the image
        var distanceY = size.Height-y; // assumes y is spread evenly across the height of the image
        return (int)(Math.Sqrt(distanceX*distanceX + distanceY*distanceY)/distance * (numberOfColors - 1));
    }

    private static int GetDepth3D(int x, int y, float z, Size size, int sizeZ, double distance, int numberOfColors)
    {
        var distanceX = 1.3 * Math.Abs(x - size.Width / 2); // assumes x is spread evenly across the width of the image
        var distanceY = size.Height - y; // assumes y is spread evenly across the height of the image  (bottom is size.Height, top is 0)
        var distanceZ = Math.Abs(1.0 - z) * sizeZ; // assumes z is spread evenly across the depth of the image, with 0.5 being the center
        return (int)(Math.Sqrt(distanceX * distanceX + distanceY * distanceY + distanceZ * distanceZ) / distance * (numberOfColors - 1));
    }

    internal void CalculateImageNew(RawLightedImage raw, double startProgress, double sumProgress, CancellationToken cancelToken)
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

        var viewPos = new Vector3(0, 0, -1.0f);

        var transformMatrix = TransformationCalculator.CreateInvertedTransformationMatrix(_fractalParams.TransformParams);
        var transformedLights = LightUtil.TransformLights(_fractalParams.Lights, transformMatrix);
        var transViewPos = TransformationCalculator.Transform(transformMatrix, viewPos);

        var pointDistance2D = Math.Sqrt(size.Width* size.Width + size.Height * size.Height);
        var depthZ = Math.Min(size.Width, size.Height); // z depth ranges from 0 to 1 but we need to scale it to be compatible with x and y for approximating distance
        var pointDistance3D = Math.Sqrt(size.Width * size.Width + size.Height * size.Height + depthZ * depthZ);

        Func<Vector3, float> distanceEstimator = EstimateDistanceComposite;

        // Build before going parallel so worker threads only read the cache
        _segments ??= BuildSegments();

        int columnsDone = 0;
        var progressLock = new object();

        Parallel.For(0, size.Width, x =>
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

                var normal = NormalCalculator.CalculateNormal(distanceEstimator, _fractalParams.NormalDistance, transformedPt);

                var lighting = (hit.Item1) ? LightUtil.GetPointLight(transformedLights, _fractalParams.LightComboMode, transformedPt, transViewPos, normal) :
                    new Lighting();

                var distance = (hit.Item2 - _fractalParams.MinStretchDistance) / (_fractalParams.MaxStretchDistance - _fractalParams.MinStretchDistance);
                distance = Math.Clamp(distance, 0.0f, 1.0f);

                int depth = 0;
                switch (_fractalParams.LSystemBranchColoringMode)
                {  
                    case BranchColoringMode.Flat:
                        depth = (hit.Item1) ? (palette.NumberOfColors - 1) : 0;  // original - Flat
                        break;
                    case BranchColoringMode.Depth:
                        depth = (hit.Item1) ? (int)(distance * (palette.NumberOfColors - 1)) : 0; // DEPTH
                        break;
                    case BranchColoringMode.Y:
                        depth = (hit.Item1) ? (int)((size.Height - y) / (float)size.Height * (palette.NumberOfColors - 1)) : 0; // Y only
                        break;
                    case BranchColoringMode.XY:
                        depth = (hit.Item1) ? GetDepth2D(x, y, size, pointDistance2D, palette.NumberOfColors) : 0;  // XY
                        break;
                    case BranchColoringMode.XYZ:
                        depth = (hit.Item1) ? GetDepth3D(x, y, distance, size, depthZ, pointDistance3D, palette.NumberOfColors) : 0; // XYZ
                        break;
                }

                // need a new raw image that stores Vector3
                var light = lighting.Diffuse + lighting.Specular;
                raw.SetPixel(x, y, depth, light);
            }

            var done = Interlocked.Increment(ref columnsDone);
            lock (progressLock)
            {
                _progressSubject.OnNext(startProgress + sumProgress * done / size.Width);
            }
        });
    }

    public async Task<FractalResult> CreateShaderAsync(FractalParams fractalParams, double startProgress, double sumProgress, CancellationToken cancelToken)
    {
        var watch = Stopwatch.StartNew();
        var size = fractalParams.ImageSize;

        var raw = new RawLightedImage(size.Width, size.Height, fractalParams.Palette.NumberOfColors);
        raw.LightingOnZeroIndex = fractalParams.LightingOnZeroIndex;

        _fractalParams = fractalParams;

        if (cancelToken.IsCancellationRequested)
            return new FractalResult();

        _progressSubject.OnNext(startProgress);

        _processedBranches = ProcessBranches(fractalParams.LSystemBranches);
        _segments = BuildSegments();

        await Task.Run(() => CalculateImageNew(raw, startProgress, sumProgress, cancelToken), cancelToken);

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
