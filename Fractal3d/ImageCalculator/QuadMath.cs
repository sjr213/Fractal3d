namespace ImageCalculator;

using System.Numerics;

public static class QuadMath
{
    public static Vector4 QuadSquareIngles(Vector4 q)
	{
        Vector3 q3 = NumericExtensions.XYZ(q);
        float dot = NumericExtensions.Dot(q3);

        return NumericExtensions.ToVector4(q.W * q.W - dot, NumericExtensions.Multiply(2.0f * q.W, q3));
    }

    // return a * ( 4.0*a.x*a.x - dot(a,a)*vec4(3.0,1.0,1.0,1.0) );
    public static Vector4 QuadCubeIngles(Vector4 q)
	{
        // The multiplication may not be correct
		return q * NumericExtensions.Subtract((4.0f * q.W * q.W), (NumericExtensions.Dot(q) * new Vector4(3.0f, 1.0f, 1.0f, 1.0f)));
	}

    public static Vector4 QuadSquare(Vector4 q)
    {
        float x = q.W * q.W - q.X * q.X - q.Y * q.Y - q.Z * q.Z;
        float y = 2 * q.W * q.X;
        float z = 2 * q.W * q.Y;
        float w = 2 * q.W * q.Z;

        return new Vector4(x, y, z, w);
    }

    public static Vector4 QuadCube(Vector4 q)
	{
			float r = -1 * (q.W * q.W) - q.X * q.X - q.Y * q.Y - q.Z * q.Z;
            float x = q.W * q.W - 3 * q.X * q.X - 3 * q.Y * q.Y - 3 * q.Z * q.Z;
			return new Vector4(x, q.X* r, q.Y* r, q.Z* r);
    }

    public static void CalculateNextCycleSquared(Vector4 Q, Vector4 C, out Vector4 Q1, out Vector4 dQ1)
    {
        dQ1 = new Vector4(2, 2, 2, 2);    // 2*dQ where dq = (1,1,1,1) I think
        Q1 = QuadSquare(Q) + C;
    }

    public static void CalculateNextCycleCubed(Vector4 Q, Vector4 C, out Vector4 Q1, out Vector4 dQ1)
	{
		dQ1 = 3 * QuadSquare(Q);
        Q1 = QuadCube(Q) + C;
	}

    public static void CalculateNextCycleInglesCubed(Vector4 Q, Vector4 C, out Vector4 Q1, out Vector4 dQ1)
	{
		Q1 = QuadCubeIngles(Q) + C;
		dQ1 = QuadSquareIngles(Q);
    }

}

