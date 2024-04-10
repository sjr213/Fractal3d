namespace ImageCalculator;

using FractureCommonLib;
using System.ComponentModel;
using System.Drawing;
using System.Numerics;

[Serializable]
public enum ShaderType
{
    [Description("Fractal Shader")]
    FractalShader,
    [Description("Crane Shader")]
    CraneShader,
    [Description("Shape Shader")]
    ShapeShader,
    [Description("Crane Pixel")]
    CranePixel
}

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
    public int Iterations { get; set; } = 100;
    public Vector4 C4 { get; set; } = new (0.1f, -0.3f, 0.2f, 0.7f);

    public int MaxRaySteps { get; set; } = 10;
    public float MinRayDistance { get; set; } = 0.1f;
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
 //   public bool PlainShader = false;
    public ShaderType ShaderType { get; set; } = ShaderType.FractalShader;
    public ShaderSceneType SceneType = ShaderSceneType.Sphere;
    public TransformationParams TransformParams = new TransformationParams();

    // Crane shader fields
    public float DistanceScale { get; set; } = 1.0f;

    public float ColorBase { get; set; } = 0.1f;

    public bool RenderShadows { get; set; } = false;

    public float EscapeThreshold { get; set; } = 10.0f;



    public static List<Light> MakeLights()
    {
        var light1 = new Light()
        {
            LightType = LightType.DirectionalLight,
            Position = new Vector3(0.2f, 0.3f, -1f),
            DiffusePower = 0.5f,
            SpecularPower = 0.2f,
            Shininess = 5
        };

        var light2 = new Light()
        {
            LightType = LightType.PointLight,
            Position = new Vector3(-0.3f, -0.5f, -1f),
            DiffusePower = 0.5f,
            SpecularPower = 0.2f,
            Shininess = 5
        };

        var light3 = new Light()
        {
            LightType = LightType.PointLight,
            Position = new Vector3(0.2f,0.2f, 0.5f),
            DiffusePower = 0.5f,
            SpecularPower = 0.2f,
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
