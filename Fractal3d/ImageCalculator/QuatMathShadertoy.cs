using System.Numerics;
using static ImageCalculator.QuatMath2;

namespace ImageCalculator
{
    public static class QuatMathShadertoy
    {
        public delegate float MapDelegate(Vector3 z, Vector4 c, int maxIterations, float escapeThreshold);

        public static Vector4 qconj(Vector4 q)
        {
            return new Vector4(q.X, -q.Y, -q.Z, -q.W);
        }

        // for Dot use System.Numerics.Vector4.Dot

        // For normalize use System.Numerics.Vector4.Normalize

        public static float qlength2(Vector4 q)
        {
            return Vector4.Dot(q, q);
        }

        // just like QuatSq in QuatMath2
        public static Vector4 qsqr(Vector4 v)
        {
            var vs = new Vector4();

            vs.X = v.X * v.X - v.Y * v.Y - v.Z * v.Z - v.W * v.W;
            vs.Y = 2 * v.X * v.Y;
            vs.Z = 2 * v.X * v.Z;
            vs.W = 2 * v.X * v.W;

            return vs;
        }

        public static Vector2 XY(this Vector4 v)
        {
            return new Vector2(v.X, v.Y);
        }

        public static Vector2 YX(this Vector4 v)
        {
            return new Vector2(v.Y, v.X);
        }

        public static Vector2 XZ(this Vector4 v)
        {
            return new Vector2(v.X, v.Z);
        }

        public static Vector2 ZX(this Vector4 v)
        {
            return new Vector2(v.Z, v.X);
        }

        public static Vector2 XW(this Vector4 v)
        {
            return new Vector2(v.X, v.W);
        }

        public static Vector2 WX(this Vector4 v)
        {
            return new Vector2(v.W, v.X);
        }


        public static Vector3 CalcNormal3(Vector3 p, Vector4 c)
        {
            const int numIterations = 11;
            Vector4 z = new Vector4(p.X, p.Y, p.Z, 0.0f);

            // identity derivative
            Vector4 J0 = new Vector4(1, 0, 0, 0);
            Vector4 J1 = new Vector4(0, 1, 0, 0);
            Vector4 J2 = new Vector4(0, 0, 1, 0);

            for (int i = 0; i < numIterations; i++)
            {
                Vector4 cz = qconj(z);

                // chain rule of jacobians (removed the 2 factor)
                J0 = new Vector4(Vector4.Dot(J0, cz), Vector2.Dot(J0.XY(), z.YX()), Vector2.Dot(J0.XZ(), z.ZX()), Vector2.Dot(J0.XW(), z.WX()));
                J1 = new Vector4(Vector4.Dot(J1, cz), Vector2.Dot(J1.XY(), z.YX()), Vector2.Dot(J1.XZ(), z.ZX()), Vector2.Dot(J1.XW(), z.WX()));
                J2 = new Vector4(Vector4.Dot(J2, cz), Vector2.Dot(J2.XY(), z.YX()), Vector2.Dot(J2.XZ(), z.ZX()), Vector2.Dot(J2.XW(), z.WX()));

                // z -> z2 + c
                z = qsqr(z) + c;

                if (qlength2(z) > 4.0) break;
            }

            Vector3 v = new Vector3(Vector4.Dot(J0, z),
                           Vector4.Dot(J1, z),
                           Vector4.Dot(J2, z));

            return Vector3.Normalize(v);
        }

        // Somewhat similar to IterateIntersectionSquared()
        public static float MapSq(Vector3 z, Vector4 c, int maxIterations, float escapeThreshold)
        {
            Vector4 z4 = new Vector4(z, 0.0f);
            float md2 = 1.0f;
            float mz2 = Vector4.Dot(z4, z4);
            float n = 1.0f;

            for(int i = 0; i < maxIterations; i++)
            {
                md2 *= 4.0f * mz2;
                z4 = qsqr(z4) + c;
                mz2 = Vector4.Dot(z4, z4);
                if (mz2 > escapeThreshold)
                {
                    break;
                }
                n += 1.0f;
            }

            return (float) (0.25 *  Math.Sqrt(mz2/md2) * Math.Log(mz2));
        }

        public static MapDelegate GetMapDelegate(QuaternionEquationType equationType)
        {
            switch (equationType)
            {
                case QuaternionEquationType.Q_Squared:
                    return MapSq;
                case QuaternionEquationType.Q_Cubed:
                    throw new NotImplementedException("MapDelegate not implemented for QuaternionEquationType.Q_Cubed");
                case QuaternionEquationType.Q_CubedZZ2:
                    throw new NotImplementedException("MapDelegate not implemented for QuaternionEquationType.Q_CubedZZ2");
                case QuaternionEquationType.Q_CubedZ2Z:
                    throw new NotImplementedException("MapDelegate not implemented for QuaternionEquationType.Q_CubedZ2Z");
                default:
                    throw new ArgumentException("Unknown Quaternion equation");
            }
        }

        // Finds the intersection of a ray with origin rO and direction rD with the quaternion Julia set specified by the quaternion constant c.
        // Shadertoy version of IntersectQJuliaST taken from softshadow()
        public static float IntersectQJuliaST(ref Vector3 startPt, Vector3 direction, FractalParams fractalParams, MapDelegate mapDelegate)
        {
            float res = 1.0f;
            float t = 0.001f;
            for(int i = 0; i < 64; i++)
            {
                float h = mapDelegate(startPt + direction * t, fractalParams.C4, fractalParams.Iterations, fractalParams.EscapeThreshold);
                res = Math.Min(res, 64.0f * h / t);
                if(res < 0.001f)
                {
                    break;
                }
                t += Math.Clamp(h, 0.01f, 0.5f);
            }
            return Math.Clamp(res, 0.0f, 1.0f);
        }

    }
}
