namespace ImageCalculator;

using System.Numerics;


public static class NumericExtensions
{
    public static bool IsNan(this Vector4 v)
    {
        return (Single.IsNaN(v.W) && Single.IsNaN(v.X) && Single.IsNaN(v.Y) && Single.IsNaN(v.Z));
    }

    public static Vector3 XYZ(this Vector4 v)
    {
        return new Vector3(v.X, v.Y, v.Z);
    }

    public static float Dot(this Vector3 v)
    {
        return v.X * v.X + v.Y * v.Y + v.Z * v.Z;
    }

    public static float Dot(this Vector4 v)
    {
        return v.W * v.W + v.X * v.X + v.Y * v.Y + v.Z * v.Z;
    }

    public static Vector3 Multiply(float s, Vector3 v)
    {
        return new Vector3(s * v.X, s * v.Y, s * v.Z);
    }

    public static Vector4 Multiply(float s, Vector4 v)
    {
        return new Vector4(s * v.W, s * v.X, s * v.Y, s * v.Z);
    }

    public static Vector4 ToVector4(float w, Vector3 v)
    {
        return new Vector4(w, v.X, v.Y, v.Z);
    }

    public static Vector4 Subtract(float s, Vector4 v)
    {
        return new Vector4(s-v.W, s-v.X, s-v.Y, s-v.Z);
    }
}

