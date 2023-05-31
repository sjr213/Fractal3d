using System.Diagnostics;
using System.Numerics;

namespace ImageCalculator;

public class PixelContainer
{
    // fromHeight and toHeight are the valid index values on the ends
    public PixelContainer(int width, int fromHeight, int toHeight, int depth)
    {
        if (width < 1)
        {
            Debug.Assert(false);
            width = 1;
        }

        if (toHeight - fromHeight < 1)
        {
            Debug.Assert(false);
            toHeight = fromHeight + 1;
        }

        if (depth < 1)
        {
            Debug.Assert(false);
            depth = 1;
        }

        FromHeight = fromHeight;
        ToHeight = toHeight;
        Width = width;
        Height = toHeight - fromHeight + 1;
        Depth = depth;

        PixelValues = new int[Width, Height];

        SetAllPixels(0);

        Lighting = new Vector3[Width, Height];

        SetAllLighting(new Vector3());
    }

    public int FromHeight
    {
        get;
        private set;
    }

    public int ToHeight
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
        if (x < 0 || x >= Width || y < FromHeight || y > ToHeight || z < 0 || z >= Depth)
        {
            Debug.Assert(false);
            return;
        }

        var localY = y - FromHeight;

        PixelValues[x, localY] = z;
        Lighting[x, localY] = light;
    }
}




