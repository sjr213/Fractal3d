
using System.ComponentModel;
using System.Numerics;

namespace ImageCalculator;

[Serializable]
public enum ReflectionType
{
    [Description("Blinn-Phong")]
    BlinnPhong,
    [Description("Phong")]
    Phong,
}

[Serializable]
public enum LightType
{
    [Description("Point")]
    PointLight,
    [Description("Directional")]
    DirectionalLight
}

[Serializable]
public class Light : ICloneable
{
    public LightType LightType { get; set; } = LightType.PointLight;
    public ReflectionType ReflectionType { get; set; } = ReflectionType.BlinnPhong;
    public Vector3 Position { get; set; } = new Vector3(-0.3f, -0.5f, -1.0f);
    public Vector3 DiffuseColor { get; set; } = new Vector3(0.5f, 0.5f, 0.8f);
    public float DiffusePower { get; set; } = 0.5f;
    public Vector3 SpecularColor { get; set; } = new Vector3(0.8f, 0.2f, 0.2f);
    public float SpecularPower { get; set; } = 0.5f;
    public float Shininess = 16.0f;

    public object Clone()
    {
        var cpy = (Light)MemberwiseClone();

        cpy.Position = new Vector3(Position.X, Position.Y, Position.Z);
        cpy.DiffuseColor = new Vector3(DiffuseColor.X, DiffuseColor.Y, DiffuseColor.Z);
        cpy.SpecularColor = new Vector3(SpecularColor.X, SpecularColor.Y, SpecularColor.Z);

        return cpy;
    }
}
