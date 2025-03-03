﻿namespace ImageCalculator;

using FractureCommonLib;
using System.Numerics;
using System.Reactive.Subjects;

public class ShaderFactory : IDisposable
{
    private FractalParams _fractalParams = new(FractalParams.MakeLights());
    private readonly Subject<double> _progressSubject = new();
    public IObservable<double> Progress => _progressSubject;
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

    /*
        float sdBox( vec3 p, vec3 b )
        {
            vec3 q = abs(p) - b;
            return length(max(q,0.0)) + min(max(q.x,max(q.y,q.z)),0.0);
        }
     */
    public static float EstimateDistanceBox(Vector3 p)
    {
        p -= new Vector3(1f, 0.25f, 0.1f);  // this moves the box to the right
        var box = new Vector3(0.3f, 0.25f, 0.1f);
        var q = Vector3.Abs(p) - box;
        return (Vector3.Max(q, Vector3.Zero)).Length() + Math.Min(Math.Max(Math.Max(q.Y, q.Z), q.X), 0.0f);
    }

    public static float EstimateDistanceTorus(Vector3 p)
    {
        Vector2 t = new Vector2(0.25f, 0.05f);
        Vector2 pxz = new Vector2(p.X, p.Z);
        Vector2 q = new Vector2(pxz.Length() - t.X, p.Y);
        return q.Length() - t.Y;
    }

    private Tuple<bool, float> RayMarch(Vector3 startPt, Vector3 direction, Matrix4x4 transformMatrix, out Vector3 pt)
    {
        float totalDistance = 0.0f;
        int steps;
        bool hit = false;

        float lastDistance = float.MaxValue;
        pt = startPt;

        for (steps = 0; steps < _fractalParams.MaxRaySteps; steps++)
        {
            pt = totalDistance * direction + startPt;
            var transformedPt = TransformationCalculator.Transform(transformMatrix, pt);
            float distance = _distanceEstimator(transformedPt);
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


    private void CalculateImageNew(RawLightedImage raw, FractalParams fractalParams, double startProgress, double sumProgress, CancellationToken cancelToken)
    {
        _progressSubject.OnNext(startProgress);

        var size = fractalParams.ImageSize;
        var palette = fractalParams.Palette;
        
         
        float left = Math.Min(_fractalParams.FromX, _fractalParams.ToX);
        float right = Math.Max(_fractalParams.FromX, _fractalParams.ToX);
        float bottom = Math.Min(_fractalParams.FromY, _fractalParams.ToY);
        float top = Math.Max(_fractalParams.FromY, _fractalParams.ToY);

        float fromZ = _fractalParams.FromZ;
        float toZ = _fractalParams.ToZ;

        float xRange = (right - left) / size.Width;
        float yRange = (top - bottom) / size.Height;

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

                var to = (_fractalParams.AimToOrigin) ? new Vector3(0.0f, 0.0f, toZ) : new Vector3(fx, fy, toZ);

                var startPt = from + fractalParams.Distance * to;

                var direction = to - from;

                var tuple = RayMarch(startPt, direction, transformMatrix, out var outPt);
                bool hit = tuple.Item1;
                float distance = tuple.Item2;

                distance = (distance - fractalParams.MinStretchDistance) / (fractalParams.MaxStretchDistance - fractalParams.MinStretchDistance);

                if (distance < 0.0f)
                    distance = 0.0f;

                if (distance > 1.0f)
                    distance = 1.0f;

                var transformedPt = TransformationCalculator.Transform(transformMatrix, outPt);
                var normal = NormalCalculator.CalculateNormal(_distanceEstimator, fractalParams.NormalDistance, transformedPt);

                var lighting = (hit) ? LightUtil.GetPointLight(transformedLights, fractalParams.LightComboMode, transformedPt, transViewPos, normal) : 
                    new Lighting();

                var depth = (hit) ? (palette.NumberOfColors - 1) : 0;

                // need a new raw image that stores Vector3
                var light = lighting.Diffuse + lighting.Specular;
                raw.SetPixel(x, y, depth, light);
            }

            if (cancelToken.IsCancellationRequested)
                return;

            var percentDone = startProgress + sumProgress * x / size.Width;
            _progressSubject.OnNext(percentDone);
        }

        _progressSubject.OnNext(startProgress + sumProgress);
    }

    private static Func<Vector3, float> GetSceneDel(ShaderSceneType sceneType)
    {
        switch (sceneType)
        {
            case ShaderSceneType.Sphere:
                return EstimateDistanceSphere;
            case ShaderSceneType.Box:
                return EstimateDistanceBox;
            case ShaderSceneType.Torus:
                return EstimateDistanceTorus;
            default:
                throw new ArgumentException("Unknown Scene Type");
        }
    }

    public async Task<FractalResult> CreateShaderAsync(FractalParams fractalParams, double startProgress, double sumProgress, CancellationToken cancelToken)
    {
        _fractalParams = fractalParams;
        var size = fractalParams.ImageSize;
        var raw = new RawLightedImage(size.Width, size.Height, fractalParams.Palette.NumberOfColors);
        raw.LightingOnZeroIndex = fractalParams.LightingOnZeroIndex;

        _distanceEstimator = GetSceneDel(fractalParams.SceneType);

        if (cancelToken.IsCancellationRequested)
            return new FractalResult();

        await Task.Run(() => CalculateImageNew(raw, fractalParams, startProgress, sumProgress, cancelToken), cancelToken);

        if (cancelToken.IsCancellationRequested)
            return new FractalResult();

        return new FractalResult()
        {
            Params = (FractalParams)fractalParams.Clone(),
            Image = raw
        };
    }
}

