using System.Numerics;

namespace ImageCalculator;

public class Lighting
{
    public Vector3 Diffuse { get; set; } = new Vector3(0f, 0f, 0f);
    public Vector3 Specular { get; set; } = new Vector3(0f, 0f, 0f);
}
