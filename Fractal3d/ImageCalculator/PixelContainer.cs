using System.Diagnostics;
using System.Numerics;

namespace ImageCalculator;

public class PixelContainer
{
    // fromHeight and toHeight are the valid index values on the ends
    public PixelContainer(int fromWidth, int toWidth, int height, int depth)
    {
        if (height < 1)
        {
            Debug.Assert(false);
            height = 1;
        }

        if (toWidth - fromWidth < 1)
        {
            Debug.Assert(false);
            toWidth = fromWidth + 1;
        }

        if (depth < 1)
        {
            Debug.Assert(false);
            depth = 1;
        }

        FromWidth = fromWidth;
        ToWidth = toWidth;
        Width = toWidth - fromWidth + 1;
        Height = height;
        Depth = depth;

        PixelValues = new int[Width, Height];

        SetAllPixels(0);

        Lighting = new Vector3[Width, Height];

        SetAllLighting(new Vector3());
    }

    public int FromWidth
    {
        get;
        private set;
    }

    public int ToWidth
    { 
        get; 
        private set;
    }

    public int Width
    { get; set; }

    public int Height
    { get; set; }

    public int Depth
    { get; set; }

    public int[,] PixelValues
    { get; set; }

    public Vector3[,] Lighting
    { get; set; }

    private void SetAllPixels(int z)
    {
        for (int x = 0; x < Width; ++x)
        {
            for (int y = 0; y < Height; ++y)
                PixelValues[x, y] = z;
        }
    }

    private void SetAllLighting(Vector3 lighting)
    {
        for (int x = 0; x < Width; ++x)
        {
            for (int y = 0; y < Height; ++y)
                Lighting[x, y] = lighting;
        }
    }

    // We expect y to be from the full range
    public void SetPixel(int x, int y, int z, Vector3 light)
    {
        if (x < FromWidth || x > ToWidth || y < 0 || y >= Height || z < 0 || z >= Depth)
        {
            Debug.Assert(false);
            return;
        }

        var localX = x - FromWidth;

        PixelValues[localX, y] = z;
        Lighting[localX, y] = light;
    }
}




