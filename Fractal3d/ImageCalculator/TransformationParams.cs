namespace ImageCalculator;

[Serializable]
public class TransformationParams : ICloneable
{
    public float TranslateX = 0.0f;
    public float TranslateY = 0.0f;
    public float TranslateZ = 0.0f;

    // These are degrees
    public float RotateX = 0.0f;
    public float RotateY = 0.0f;
    public float RotateZ = 0.0f;

    public float ScaleX = 1.0f;
    public float ScaleY = 1.0f;
    public float ScaleZ = 1.0f;

    public object Clone()
    {
        return MemberwiseClone();
    }
}

