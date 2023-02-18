
using System.ComponentModel;
using System.Numerics;

namespace ImageCalculator;

[Serializable]
public enum LightingType
{
    [Description("Blinn-Phong")]
    BlinnPhong,
    [Description("Phong")]
    Phong,
}

[Serializable]
public class PointLight : ICloneable
{
    public LightingType LightingType { get; set; } = LightingType.BlinnPhong;
    public Vector3 Position { get; set; } = new Vector3(-0.3f, -0.5f, -1.0f);
    public Vector3 DiffuseColor { get; set; } = new Vector3(0.5f, 0.5f, 0.8f);
    public float DiffusePower { get; set; } = 0.5f;
    public Vector3 SpecularColor { get; set; } = new Vector3(0.8f, 0.2f, 0.2f);
    public float SpecularPower { get; set; } = 0.5f;
    public float Shininess = 16.0f;
    public Vector3 LightColor { get; set; } = new Vector3(1.0f, 1.0f, 1.0f);
    public float ScreenGamma { get; set; } = 2.2f;
    public float AmbientPower { get; set; } = 0.5f;

    public object Clone()
    {
        return MemberwiseClone();
    }
}
