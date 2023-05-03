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
public enum ShaderSceneType
{
    [Description("Sphere")]
    Sphere,
    [Description("Box")]
    Box,
    [Description("Torus")]
    Torus
}

[Serializable]
public enum LightCombinationMode
{
    [Description("Average")]
    Average,
    [Description("Sum")]
    Sum
}

[Serializable]
public class FractalParams : ICloneable
{
    public FractalParams()
    {
    }

    public FractalParams(List<Light> lights)
    {
        Lights = lights;
    }

    public Size ImageSize { get; set; } = new(400, 400);
    public Size DisplaySize { get; set; } = new(400, 400);
    public Palette Palette { get; set; } = new(256);
    public float FromX { get; set; } = -1.0f;
    public float ToX { get; set; } = 1.0f;
    public float FromY { get; set; } = -1.0f;
    public float ToY { get; set; } = 1.0f;
    public float FromZ { get; set; } = -1.0f;
    public float ToZ { get; set; } = 1.0f;

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
    public float AmbientPower { get; set; } = 0.5f;
    public LightCombinationMode LightComboMode { get; set; } = LightCombinationMode.Average;
    public List<Light> Lights { get; set; } = new List<Light>();
    public DisplayInfo ColorInfo { get; set; } = new();
    public bool PlainShader = false;
    public ShaderSceneType SceneType = ShaderSceneType.Sphere;
    public TransformationParams TransformParams = new TransformationParams();

    public static List<Light> MakeLights()
    {
        var light1 = new Light()
        {
            LightType = LightType.DirectionalLight,
            Position = new Vector3(0f, -0.1f, -1f),
            DiffusePower = 0.2f,
            SpecularPower = 0.05f,
            Shininess = 5
        };

        var light2 = new Light()
        {
            LightType = LightType.PointLight,
            Position = new Vector3(-0.7f, -0.7f, -0.8f),
            DiffusePower = 0.5f,
            SpecularPower = 0.3f,
            Shininess = 16
        };

        var light3 = new Light()
        {
            LightType = LightType.PointLight,
            Position = new Vector3(0.4f,0.3f, -1f),
            DiffusePower = 0.3f,
            SpecularPower = 0.1f,
            Shininess = 5
        };

        return new List<Light>() { light1, light2, light3 };
    }

    public object Clone()
    {
        var copy = (FractalParams)MemberwiseClone();
        copy.Lights = new List<Light>();
        foreach(var light in Lights )
            copy.Lights.Add( (Light)light.Clone() );
        copy.ColorInfo = (DisplayInfo)ColorInfo.Clone();
        copy.Palette = (Palette)Palette.Clone();
        copy.TransformParams = (TransformationParams)TransformParams.Clone();
        return copy;
    }
}
