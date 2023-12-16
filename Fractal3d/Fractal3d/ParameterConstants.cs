namespace Fractal3d;

public static class ParameterConstants
{
    public const float FloatTolerance = 0.00001f;

    public const int MinImage = 10;
    public const int MaxImage = 10000;

    public const float MinFromTo = -10000f;
    public const float MaxFromTo = 10000f;

    public const float MinBailout = 0.01f;
    public const float MaxBailout = 1e9f;

    public const int MinMaxRaySteps = 1;
    public const int MaxMaxRaySteps = 1000000;

    public const float MinMinRayDistance = 1e-6f;
    public const float MaxMinRayDistance = 1f;

    public const float MinStepDivisor = 1f;
    public const float MaxStepDivisor = 1e11f;

    public const int MinIterations = 2;
    public const int MaxIterations = 1000000;

    public const float MinDistance = 0.0001f;
    public const float MaxDistance = 10000f;

    public const float MinNormalDistance = 0.00001f;
    public const float MaxNormalDistance = 1000f;

    public const float MinFloatColor = 0f;
    public const float MaxFloatColor = 1f;

    public const float MinPower = 0f;
    public const float MaxPower = 2f;

    public const float MinShininess = 1f;
    public const float MaxShininess = 128f;

    public const int MinPaletteColors = 2;
    public const int MaxPaletteColors = 100000;

    public const float MinTranslation = -100.0f;
    public const float MaxTranslation = 100.0f;

    public const float MinRotation = -360.0f;
    public const float MaxRotation = 360.0f;

    public const float MinScale = 1e-5f;
    public const float MaxScale = 1e+5f;

    public const int MinConstantCStep = 1;
    public const int MaxConstantCStep = 100;
}

