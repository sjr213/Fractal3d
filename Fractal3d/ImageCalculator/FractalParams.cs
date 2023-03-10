namespace ImageCalculator;

using FractureCommonLib;
using System.ComponentModel;
using System.Drawing;
using System.Numerics;

[Serializable]
public enum QuaternionEquationType
{
    [Description("Quat Squared")]
    Q_Squared,
    [Description("Quat Cubed")]
    Q_Cubed,
    [Description("Ingles Cubed")]
    Q_InglesCubed
}

[Serializable]
public class FractalParams : ICloneable
{
    public Size ImageSize { get; set; } = new(400, 400);
    public Size DisplaySize { get; set; } = new(400, 400);
    public Palette Palette { get; set; } = new(256);
    public float FromX { get; set; } = -1.0f;
    public float ToX { get; set; } = 1.0f;
    public float FromY { get; set; } = -1.0f;
    public float ToY { get; set; } = 1.0f;

    public float Bailout { get; set; } = 2.0f;
    public int Iterations { get; set; } = 256;
    public Vector4 C4 { get; set; } = new (0.1f, -0.3f, 0.2f, 0.7f);

    public int MaxRaySteps { get; set; } = 100;
    public float MinRayDistance { get; set; } = 0.0001f;
    public float Distance { get; set; } = 0.1f;
    public float MaxDistance { get; set; } = 10.0f;
    public float StepDivisor { get; set; } = 10.0f;
    public QuaternionEquationType QuatEquation = QuaternionEquationType.Q_Squared;
    public bool AimToOrigin { get; set; } = false;

    public float NormalDistance { get; set; } = 0.01f;
    public PointLight Light {get; set;} = new ();
    public DisplayInfo ColorInfo { get; set; } = new();

    public object Clone()
    {
        var copy = (FractalParams)MemberwiseClone();
        copy.Light = (PointLight)Light.Clone();
        copy.ColorInfo = (DisplayInfo)ColorInfo.Clone();
        copy.Palette = (Palette)Palette.Clone();    
        return copy;
    }
}
