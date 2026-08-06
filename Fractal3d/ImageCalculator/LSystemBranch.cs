
namespace ImageCalculator;

[Serializable]
public class LSystemBranch : ICloneable
{
    public LSystemBranch() { }
    public LSystemBranch(float relativePosition, float attenuation, float angleXDegrees, float angleYDegrees, float angleZDegrees)
    {
        RelativePostionX = relativePosition;
        Attenuation = attenuation;
        AngleXDegrees = angleXDegrees;
        AngleYDegrees = angleYDegrees;
        AngleZDegrees = angleZDegrees;
    }

    public float RelativePostionX { get; set; } = 1.0f;
    public float Attenuation { get; set; } = 1.0f;
    public float AngleXDegrees { get; set; } = 0.0f;
    public float AngleYDegrees { get; set; } = 0.0f;
    public float AngleZDegrees { get; set; } = 0.0f;

    public object Clone()
    {
        return this.MemberwiseClone();
    }

    public static List<LSystemBranch> MakeDefaultBranches()
    {
        return new List<LSystemBranch>
        {
            new LSystemBranch(1.0f, 1.0f, 0.0f, 0.0f, -30.0f),
            new LSystemBranch(1.0f, 1.0f, 0.0f, 0.0f, 30.0f)
        };
    }
}
