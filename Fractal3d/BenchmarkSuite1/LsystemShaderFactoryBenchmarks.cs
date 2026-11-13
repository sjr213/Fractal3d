using System.Drawing;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using FractureCommonLib;
using ImageCalculator;

namespace BenchmarkSuite1;

[MemoryDiagnoser]
public class LsystemShaderFactoryBenchmarks
{
    private LsystemShaderFactory _factory = null!;
    private FractalParams _params = null!;

    [GlobalSetup]
    public void Setup()
    {
        _params = new FractalParams(FractalParams.MakeLights())
        {
            ImageSize = new Size(100, 100),
            Iterations = 4,
            MaxRaySteps = 10
        };

        _factory = new LsystemShaderFactory();

        var flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(LsystemShaderFactory).GetField("_fractalParams", flags)!.SetValue(_factory, _params);
        typeof(LsystemShaderFactory).GetField("_processedBranches", flags)!
            .SetValue(_factory, LsystemShaderFactory.ProcessBranches(_params.LSystemBranches));
    }

    [GlobalCleanup]
    public void Cleanup() => _factory.Dispose();

    [Benchmark]
    public RawLightedImage CalculateImageNew()
    {
        var raw = new RawLightedImage(_params.ImageSize.Width, _params.ImageSize.Height, _params.Palette.NumberOfColors);
        _factory.CalculateImageNew(raw, 0.0, 100.0, CancellationToken.None);
        return raw;
    }
}
