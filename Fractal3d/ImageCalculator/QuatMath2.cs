namespace ImageCalculator;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Numerics;
using System.Runtime.CompilerServices;

// This class uses a different order in Vector4. It treats the order as x, y, z, w
// Whereas the original QuatMath orders them w, x, y, z. This effects the calculations
// Taken from Crane, K. Ray Tracing Quaternion Julia Sets on the GPU 
// https://www.cs.cmu.edu/~kmcrane/Projects/QuaternionJulia/paper.pdf
public static class QuatMath2
{
    // radius of bounding sphere for the set used to accerate intersection
 //   const float BOUNDING_RADIUS_2 = 3.0f;     

    // Any series whose points magnitude exceed this threshold are considered divergent
 //   const float ESCAPE_THRESHOLD = 1e1f;

    // Delta is used in the infinite difference approximation of the gradient (to determine normals)
    // const float DEL = 1e-4f; This was the original default distance value Fractal3D default is 0.01f

    public delegate void CalculationIntersectionDelegate(ref Vector4 q, ref Vector4 qp, Vector4 c, int maxIterations, float escapeThreshold);

    public delegate Vector3 NormEstimateDelegate(Vector3 p, Vector4 c, int maxIterations, float distance);

    public delegate Vector4 QuatCubedDelegate(Vector4 v);

    public static Vector3 YZW(this Vector4 v)
    {
        return new Vector3(v.Y, v.Z, v.W);
    }

    /* This is the original version from Crane's paper, kept for reference
    public static Vector4 QuatMult(Vector4 q1, Vector4 q2)
    {
        float x = q1.X * q2.X - q1.Y * q2.Y - q1.Z * q2.Z - q1.W * q2.W;
        var cross = Vector3.Cross(YZW(q1), YZW(q2));

        float y = q1.X * q2.Y + q2.X * q1.Y + cross.X;
        float z = q1.X * q2.Z + q2.X * q1.Z + cross.Y;
        float w = q1.X * q2.W + q2.X * q1.W + cross.Z;

        return new Vector4(x, y, z, w);
    }
    */

    // This comes from shadertoy
    public static Vector4 QuatMult(Vector4 q1, Vector4 q2)
    {
        // a.x * b.x - a.y * b.y - a.z * b.z - a.w * b.w,
        float x = q1.X * q2.X - q1.Y * q2.Y - q1.Z * q2.Z - q1.W * q2.W;

        // a.y* b.x + a.x * b.y + a.z * b.w - a.w * b.z, 
        float y = q1.Y * q2.X + q1.X * q2.Y + q1.Z * q2.W - q1.W * q2.Z;

        // a.z * b.x + a.x * b.z + a.w * b.y - a.y * b.w,
        float z = q1.Z * q2.X + q1.X * q2.Z + q1.W * q2.Y - q1.Y * q2.W;

        // a.w * b.x + a.x * b.w + a.y * b.z - a.z * b.y 
        float w = q1.W * q2.X + q1.X * q2.W + q1.Y * q2.Z - q1.Z * q2.Y;

        return new Vector4(x, y, z, w);
    }

    public static Vector4 QuatSq(Vector4 v)
    {
        var vs = new Vector4();

        vs.X = v.X * v.X - v.Y * v.Y - v.Z * v.Z - v.W * v.W;
        vs.Y = 2 * v.X * v.Y;
        vs.Z = 2 * v.X * v.Z;
        vs.W = 2 * v.X * v.W;

        return vs;
    }

    public static Vector4 QuatCubedOld(Vector4 v)
    {
        float x = v.X * v.X - 3.0f * v.Y * v.Y - 3.0f * v.Z * v.Z - 3.0f * v.W * v.W;
        float r = -1 * (v.X * v.X + v.Y * v.Y + v.Z * v.Z + v.W * v.W);

        return new Vector4(x, v.Y * r, v.Z * r, v.W * r);
    }

    // QuatCubed(Z^2)Z
    public static Vector4 QuatCubedZ2Z(Vector4 v)
    {
        var vs = new Vector4();
        vs.X = v.X * (v.X * v.X - 3.0f * v.Y * v.Y - 3.0f * v.Z * v.Z - 3.0f * v.W * v.W);
        vs.Y = 3.0f * v.X * v.X * v.Y + v.X * v.X * v.Z - v.Z * v.Z * v.Y- v.Z * v.Z * v.X;
        vs.Z = -1.0f *(v.Y * v.Y * v.Y + v.Y * v.Y * v.Z + v.Z * v.Z * v.Z + 2.0f * v.X * v.X * v.Z + v.W * v.W * v.Z);
        vs.W = v.W * (3.0f * v.X * v.X - v.Y * v.Y - v.Z * v.Z - v.W * v.W);

        return vs;
    }

    // QuatCubedZZ^2
    public static Vector4 QuatCubedZZ2(Vector4 v)
    {
        var vs = new Vector4();
        vs.X = v.X * (v.X * v.X - 3.0f * v.Y * v.Y - 3.0f * v.Z * v.Z - 3.0f * v.W * v.W);
        vs.Y = v.Y * (3.0f * v.X * v.X - v.Y * v.Y - v.Z * v.Z - v.W * v.W);
        vs.Z = v.Z * (3.0f * v.X * v.X - v.Y * v.Y - v.Z * v.Z - v.W * v.W);
        vs.W = v.W * (3.0f * v.X * v.X - v.Y * v.Y - v.Z * v.Z - v.W * v.W);

        return vs;
    }

    // Iterates the quaternion for the purposes of intersection.
    // Also produces an estimate of the derivative q, which is required for the distance estimate.
    // The quaternion c is the parameter specifying the Julia set. 
    // To estimate the derivative at q we recursively evaluate q' = 2*q*q'
    public static void IterateIntersectionSquared( ref Vector4 q, ref Vector4 qp, Vector4 c, int maxIterations, float escapeThreshold)
    {
        for(int i =0; i < maxIterations; i++)
        {
            qp = 2.0f * QuatMult(q, qp);
            q = QuatSq(q) + c;

            if( Vector4.Dot(q, q) > escapeThreshold ) 
            {
                break;
            }
        }
    }

    public static void IterateIntersectionCubedOld(ref Vector4 q, ref Vector4 qp, Vector4 c, int maxIterations, float escapeThreshold)
    {
        for (int i = 0; i < maxIterations; i++)
        {
            qp = 3.0f * QuatMult(QuatSq(q), qp);
            q = QuatCubedOld(q) + c;

            if (Vector4.Dot(q, q) > escapeThreshold)
            {
                break;
            }
        }
    }

    public static void IterateIntersectionCubedZ2Z(ref Vector4 q, ref Vector4 qp, Vector4 c, int maxIterations, float escapeThreshold)
    {
        for (int i = 0; i < maxIterations; i++)
        {
            qp = 3.0f * QuatMult(QuatSq(q), qp);
            q = QuatCubedZ2Z(q) + c;

            if (Vector4.Dot(q, q) > escapeThreshold)
            {
                break;
            }
        }
    }

    public static void IterateIntersectionCubedZZ2(ref Vector4 q, ref Vector4 qp, Vector4 c, int maxIterations, float escapeThreshold)
    {
        for (int i = 0; i < maxIterations; i++)
        {
            qp = 3.0f * QuatMult(QuatSq(q), qp);
            q = QuatCubedZZ2(q) + c;

            if (Vector4.Dot(q, q) > escapeThreshold)
            {
                break;
            }
        }
    }

    // Create a shading normal for the current point.
    public static Vector3 NormEstimateSquared(Vector3 p, Vector4 c, int maxIterations, float distance)
    {
        Vector4 qp = new Vector4(p, 0f);

        Vector4 gx1 = new Vector4(qp.X - distance, qp.Y, qp.Z, qp.W);
        Vector4 gx2 = new Vector4(qp.X + distance, qp.Y, qp.Z, qp.W);
        Vector4 gy1 = new Vector4(qp.X, qp.Y - distance, qp.Z, qp.W);
        Vector4 gy2 = new Vector4(qp.X, qp.Y + distance, qp.Z, qp.W);
        Vector4 gz1 = new Vector4(qp.X, qp.Y, qp.Z - distance, qp.W);
        Vector4 gz2 = new Vector4(qp.X, qp.Y, qp.Z + distance, qp.W);

        for(int i = 0; i < maxIterations; i++)
        {
            gx1 = QuatSq(gx1) + c;
            gx2 = QuatSq(gx2) + c;
            gy1 = QuatSq(gy1) + c;
            gy2 = QuatSq(gy2) + c;
            gz1 = QuatSq(gz1) + c;
            gz2 = QuatSq(gz2) + c;
        }

        var gradX = gx2.Length() - gx1.Length();
        var gradY = gy2.Length() - gy1.Length();
        var gradZ = gz2.Length() - gz1.Length();

        var v = new Vector3(gradX, gradY, gradZ);
        return Vector3.Normalize(v);
    }

    // Create a shading normal for the current point.
    public static Vector3 NormEstimateCubedOld(Vector3 p, Vector4 c, int maxIterations, float distance)
    {
        Vector4 qp = new Vector4(p, 0f);

        Vector4 gx1 = new Vector4(qp.X - distance, qp.Y, qp.Z, qp.W);
        Vector4 gx2 = new Vector4(qp.X + distance, qp.Y, qp.Z, qp.W);
        Vector4 gy1 = new Vector4(qp.X, qp.Y - distance, qp.Z, qp.W);
        Vector4 gy2 = new Vector4(qp.X, qp.Y + distance, qp.Z, qp.W);
        Vector4 gz1 = new Vector4(qp.X, qp.Y, qp.Z - distance, qp.W);
        Vector4 gz2 = new Vector4(qp.X, qp.Y, qp.Z + distance, qp.W);

        for (int i = 0; i < maxIterations; i++)
        {
            gx1 = QuatCubedOld(gx1) + c;
            gx2 = QuatCubedOld(gx2) + c;
            gy1 = QuatCubedOld(gy1) + c;
            gy2 = QuatCubedOld(gy2) + c;
            gz1 = QuatCubedOld(gz1) + c;
            gz2 = QuatCubedOld(gz2) + c;
        }

        var gradX = gx2.Length() - gx1.Length();
        var gradY = gy2.Length() - gy1.Length();
        var gradZ = gz2.Length() - gz1.Length();

        var v = new Vector3(gradX, gradY, gradZ);
        return Vector3.Normalize(v);
    }

    // Create a shading normal for the current point.
    public static Vector3 NormEstimateCubedOldZ2Z(Vector3 p, Vector4 c, int maxIterations, float distance)
    {
        Vector4 qp = new Vector4(p, 0f);

        Vector4 gx1 = new Vector4(qp.X - distance, qp.Y, qp.Z, qp.W);
        Vector4 gx2 = new Vector4(qp.X + distance, qp.Y, qp.Z, qp.W);
        Vector4 gy1 = new Vector4(qp.X, qp.Y - distance, qp.Z, qp.W);
        Vector4 gy2 = new Vector4(qp.X, qp.Y + distance, qp.Z, qp.W);
        Vector4 gz1 = new Vector4(qp.X, qp.Y, qp.Z - distance, qp.W);
        Vector4 gz2 = new Vector4(qp.X, qp.Y, qp.Z + distance, qp.W);

        for (int i = 0; i < maxIterations; i++)
        {
            gx1 = QuatCubedZ2Z(gx1) + c;
            gx2 = QuatCubedZ2Z(gx2) + c;
            gy1 = QuatCubedZ2Z(gy1) + c;
            gy2 = QuatCubedZ2Z(gy2) + c;
            gz1 = QuatCubedZ2Z(gz1) + c;
            gz2 = QuatCubedZ2Z(gz2) + c;
        }

        var gradX = gx2.Length() - gx1.Length();
        var gradY = gy2.Length() - gy1.Length();
        var gradZ = gz2.Length() - gz1.Length();

        var v = new Vector3(gradX, gradY, gradZ);
        return Vector3.Normalize(v);
    }

    // Create a shading normal for the current point.
    public static Vector3 NormEstimateCubedZZ2(Vector3 p, Vector4 c, int maxIterations, float distance)
    {
        Vector4 qp = new Vector4(p, 0f);

        Vector4 gx1 = new Vector4(qp.X - distance, qp.Y, qp.Z, qp.W);
        Vector4 gx2 = new Vector4(qp.X + distance, qp.Y, qp.Z, qp.W);
        Vector4 gy1 = new Vector4(qp.X, qp.Y - distance, qp.Z, qp.W);
        Vector4 gy2 = new Vector4(qp.X, qp.Y + distance, qp.Z, qp.W);
        Vector4 gz1 = new Vector4(qp.X, qp.Y, qp.Z - distance, qp.W);
        Vector4 gz2 = new Vector4(qp.X, qp.Y, qp.Z + distance, qp.W);

        for (int i = 0; i < maxIterations; i++)
        {
            gx1 = QuatCubedZZ2(gx1) + c;
            gx2 = QuatCubedZZ2(gx2) + c;
            gy1 = QuatCubedZZ2(gy1) + c;
            gy2 = QuatCubedZZ2(gy2) + c;
            gz1 = QuatCubedZZ2(gz1) + c;
            gz2 = QuatCubedZZ2(gz2) + c;
        }

        var gradX = gx2.Length() - gx1.Length();
        var gradY = gy2.Length() - gy1.Length();
        var gradZ = gz2.Length() - gz1.Length();

        var v = new Vector3(gradX, gradY, gradZ);
        return Vector3.Normalize(v);
    }

    public static CalculationIntersectionDelegate GetCalculationDelegate
        (QuaternionEquationType equationType)
    {
        switch (equationType)
        {
            case QuaternionEquationType.Q_Squared:
                return IterateIntersectionSquared;
            case QuaternionEquationType.Q_Cubed:
                return IterateIntersectionCubedOld;
            case QuaternionEquationType.Q_CubedZZ2:
                return IterateIntersectionCubedZZ2;
            case QuaternionEquationType.Q_CubedZ2Z:
                return IterateIntersectionCubedZ2Z;
            default:
                throw new ArgumentException("Unknown Quaternion equation");
        }
    }

    public static NormEstimateDelegate GetNormEstimateDelegate(QuaternionEquationType equationType)
    {
        switch (equationType)
        {
            case QuaternionEquationType.Q_Squared:
                return NormEstimateSquared;
            case QuaternionEquationType.Q_Cubed:
                return NormEstimateCubedOld;
            case QuaternionEquationType.Q_CubedZZ2:
                return NormEstimateCubedZZ2;
            case QuaternionEquationType.Q_CubedZ2Z:
                return NormEstimateCubedOldZ2Z;
            default:
                throw new ArgumentException("Unknown Quaternion equation");
        }
    }

    // Finds the intersection of a ray with origin rO and direction rD with the quaternion Julia set specified by the quaternion constant c.
    public static float IntersectQJulia(ref Vector3 startPt, Vector3 direction, FractalParams fractalParams, CalculationIntersectionDelegate calculationIntersectionDelegate)
    {
        float dist = 0;

        for(int i = 0; i < fractalParams.MaxRaySteps; i++)
        {
            Vector4 z = new Vector4(startPt, 0);
            Vector4 zp = new Vector4(1, 0, 0, 0);

            calculationIntersectionDelegate(ref z, ref zp, fractalParams.C4, fractalParams.Iterations, fractalParams.EscapeThreshold);
            float normZ = z.Length();
            dist = 0.5f * normZ * (float)Math.Log(normZ) / zp.Length();

            startPt += direction * dist;

            if (float.IsNaN(dist))
                return 1;

            if (dist < fractalParams.MinRayDistance)
                break;

            if (dist > fractalParams.MaxDistance)
                break;

            if (Vector3.Dot(startPt, startPt) > fractalParams.Bailout)
                return 1;
        }
        return dist;
    }

    public static float IntersectQJuliaForPixelShader(ref Vector3 startPt, Vector3 direction, FractalParams fractalParams, CalculationIntersectionDelegate calculationIntersectionDelegate)
    {
        float normZ = 0;

        for (int i = 0; i < fractalParams.MaxRaySteps; i++)
        {
            Vector4 z = new Vector4(startPt, 0);
            Vector4 zp = new Vector4(1, 0, 0, 0);

            calculationIntersectionDelegate(ref z, ref zp, fractalParams.C4, fractalParams.Iterations, fractalParams.EscapeThreshold);
            normZ = z.Length();
            float dist = 0.5f * normZ * (float)Math.Log(normZ) / zp.Length();

            startPt += direction * dist;

            if (float.IsNaN(dist))
                return 0;

            if (dist < fractalParams.MinRayDistance)
                break;

            if (dist > fractalParams.MaxDistance)
                break;

            if (Vector3.Dot(startPt, startPt) > fractalParams.Bailout)
                return 0;
        }

        normZ = normZ * 0.1f;
        return normZ * normZ;
    }

    public static float RayMarchQJulia(ref Vector3 startPt, Vector3 direction, FractalParams fractalParams, CalculationIntersectionDelegate calculationIntersectionDelegate)
    {
        float totalDistance = 0.0f;
        int steps;

        float lastDistance = float.MaxValue;

        for (steps = 0; steps < fractalParams.MaxRaySteps; steps++)
        {
            Vector4 z = new Vector4(startPt, 0);
            Vector4 zp = new Vector4(1, 0, 0, 0);

            calculationIntersectionDelegate(ref z, ref zp, fractalParams.C4, fractalParams.Iterations, fractalParams.EscapeThreshold);
            float normZ = z.Length();
            float dist = 0.5f * normZ * (float)Math.Log(normZ) / zp.Length();
            dist /= fractalParams.StepDivisor;

            startPt += direction * dist;

            totalDistance += dist;

            if (dist < fractalParams.MinRayDistance)
                break;

            if (dist > lastDistance)
                return 0;

            if (totalDistance > fractalParams.MaxDistance)
                break;

            if (float.IsNaN(dist))
                return 0;

            if (Vector3.Dot(startPt, startPt) > fractalParams.Bailout)
                return 0;

            lastDistance = dist;
        }

        return 1.0f - ((float)steps) / fractalParams.MaxRaySteps;
    }

    // Computes the direct illumination for the point pt with normal N due to a point light and viewer at eye.
    public static Vector3 Phong(Vector3 light, Vector3 eye, Vector3 pt, Vector3 N)
    {
        var diffuse = new Vector3(1f, 0.45f, 0.25f);
        const int specularExponent = 10;

        const float specularity = 0.45f; // amplitude of specular highlight

        Vector3 L = Vector3.Normalize(light - pt);  // find the vector to the light
        Vector3 E = Vector3.Normalize(eye - pt);    // find the vector to the eye
        float NdotL = Vector3.Dot(N, L);            // find cosine of the angle between light and normal
        Vector3 R = L - 2 * NdotL * N;              // reflected vector

        diffuse += Vector3.Abs(N) * 0.3f;           // Add some of the normal to the color to make it more interesting

        // compute the illumnation using the Phong equation
        var v3Part = diffuse * Math.Abs(NdotL);  //   Math.Max(NdotL, 0);
        float scalerPart = specularity * (float)Math.Pow(Math.Max(Vector3.Dot(E, R), 0), specularExponent);
        return new Vector3(v3Part.X + scalerPart, v3Part.Y + scalerPart, v3Part.Z + scalerPart);
    }

    // Computes the direct illumination for the point pt with normal N due to a point light and viewer at eye.
    // This version was made to use the rest of out light fields, not just Crane's.
    public static Vector3 PhongExpanded(Light light, Vector3 eye, Vector3 pt, Vector3 N)
    {
        var diffuse = light.DiffuseColor;

        Vector3 L = Vector3.Normalize(light.Position - pt);  // find the vector to the light
        Vector3 E = Vector3.Normalize(eye - pt);    // find the vector to the eye
        float NdotL = Vector3.Dot(N, L);            // find cosine of the angle between light and normal
        Vector3 R = L - 2 * NdotL * N;              // reflected vector

        if(light.UseNormalComponent)
            diffuse += Vector3.Abs(N) * light.DiffusePower;           // Add some of the normal to the color to make it more interesting
        else
            diffuse *= light.DiffusePower;

        // compute the illumnation using the Phong equation
        var v3Part = diffuse * Math.Abs(NdotL);  //   Math.Max(NdotL, 0);
        var scalerPart = light.SpecularColor * light.SpecularPower * (float)Math.Pow(Math.Max(Vector3.Dot(E, R), 0), light.Shininess/4.0f);
        return new Vector3(v3Part.X + scalerPart.X, v3Part.Y + scalerPart.Y, v3Part.Z + scalerPart.Z);
    }

    public static Vector3 GetPhongLightsExpanded(List<Light> lights, LightCombinationMode lightComboMode, Vector3 eye, Vector3 pt, Vector3 normal)
    {
        var lighting = new Vector3();

        int numberOfLights = lights.Count;
        foreach (var light in lights)
        {
            var singleLight = PhongExpanded(light, eye, pt, normal);
            if (lightComboMode == LightCombinationMode.Average)
            {
                lighting += singleLight / numberOfLights;
            }
            else
            {
                lighting += singleLight;
            }
        }

        lighting = Vector3.Clamp(lighting, Vector3.Zero, Vector3.One);

        return lighting;
    }

    // Finds the intersection of a ray with a sphere with statically defined radius BOUNDING_RADIUS centered at the origin.
    // This sphere serves as a bounding volume for the Julia set.
    public static Vector3 IntersectSphere(Vector3 rO, Vector3 rD, float boudingRadius)
    {
        float B = 2.0f * Vector3.Dot(rO, rD);
        float C = Vector3.Dot(rO, rO) - boudingRadius;
        float d = (float)Math.Sqrt(B*B - 4.0f*C);
        float t0 = (-B + d) * 0.5f;
        float t1 = (-B - d) * 0.5f;
        float t = Math.Min(t0, t1);

        rO += t * rD;

        return rO;
    }
}
