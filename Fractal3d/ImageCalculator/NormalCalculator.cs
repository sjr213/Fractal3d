using System.Numerics;

namespace ImageCalculator;

public static class NormalCalculator
{
    public static Vector3 CalculateNormal(Func<Vector3, float> estimateDistance, float distance, Vector3 p)
    {
        Vector3 xp = new Vector3(p.X + distance, p.Y, p.Z);
        float xPlus = estimateDistance(xp);

        Vector3 xm = new Vector3(p.X - distance, p.Y, p.Z);
        float xMinus = estimateDistance(xm);

        Vector3 yp = new Vector3(p.X, p.Y + distance, p.Z);
        float yPlus = estimateDistance(yp);

        Vector3 ym = new Vector3(p.X, p.Y - distance, p.Z);
        float yMinus = estimateDistance(ym);

        Vector3 zp = new Vector3(p.X, p.Y, p.Z + distance);
        float zPlus = estimateDistance(zp);

        Vector3 zm = new Vector3(p.X, p.Y, p.Z - distance);
        float zMinus = estimateDistance(zm);

        var almostNormal = new Vector3(xPlus -xMinus, yPlus - yMinus, zPlus - zMinus);

        return Vector3.Normalize(almostNormal);
    }
}
