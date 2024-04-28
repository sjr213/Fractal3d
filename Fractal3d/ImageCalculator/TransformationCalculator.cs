using System.Drawing.Drawing2D;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Security.Cryptography.X509Certificates;

namespace ImageCalculator;

public static class TransformationCalculator
{
    public const float Pi = 3.141592654f;

    public static float ConvertToRadians(float fDegrees)
    {
        return fDegrees * (Pi / 180.0f);
    }

    public static Matrix4x4 CreateTransformationMatrix(TransformationParams transParams)
    {
        var translation = Matrix4x4.CreateTranslation(transParams.TranslateX, transParams.TranslateY, transParams.TranslateZ);
        var rotX = Matrix4x4.CreateRotationX(ConvertToRadians(transParams.RotateX));
        var rotY = Matrix4x4.CreateRotationY(ConvertToRadians(transParams.RotateY));
        var rotZ = Matrix4x4.CreateRotationZ(ConvertToRadians(transParams.RotateZ));
        var scale = Matrix4x4.CreateScale(transParams.ScaleX, transParams.ScaleY, transParams.ScaleZ);

        var transformation = scale * rotX * rotY * rotZ * translation;

        return transformation;
    }

    public static Matrix4x4 CreateInvertedTransformationMatrix(TransformationParams transParams)
    {
        var t = CreateTransformationMatrix(transParams);

        return (Matrix4x4.Invert(t, out var inverted) ? inverted : t);
    }

    // According to http://www.codinglabs.net/article_world_view_projection_matrix.aspx
    // we need this but this is column vector notation. MS code is row vector notation
    public static Vector4 Multiply(Matrix4x4 m, Vector4 v)
    {
        return new Vector4(
            (m.M11 * v.X) + (m.M12 * v.Y) + (m.M13 * v.Z) + (m.M14 * v.W),
            (m.M21 * v.X) + (m.M22 * v.Y) + (m.M23 * v.Z) + (m.M24 * v.Z),
            (m.M31 * v.X) + (m.M32 * v.Y) + (m.M33 * v.Z) + (m.M34 * v.Z),
            (m.M41 * v.X) + (m.M42 * v.Y) + (m.M43 * v.Z) + (m.M44 * v.W)
        );
    }

    public static Vector3 Transform(Matrix4x4 matrix, Vector3 pos)
    {
        Vector4 v = new Vector4(pos, 1.0f);

        // If this is wrong use Vector4.Transform(Vector4 vector, Matrix4x4 matrix)
        // var newPos = Multiply(matrix, v);
        var newPos = Vector4.Transform(v, matrix);

        return newPos.XYZ();
    }
}