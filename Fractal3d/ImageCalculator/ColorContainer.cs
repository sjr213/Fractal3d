using System.Diagnostics;
using System.Drawing;

namespace ImageCalculator;

public class ColorContainer
{
    public ColorContainer(int fromWidth, int toWidth, int height, Color backgroundColor)
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

        FromWidth = fromWidth;
        ToWidth = toWidth;
        Width = toWidth - fromWidth + 1;
        Height = height;

        ColorValues = new Color[Width, Height];

        SetAllColors(backgroundColor);
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

    public Color[,] ColorValues
    { get; set; }

    private void SetAllColors(Color color)
    {
        for (int x = 0; x < Width; ++x)
        {
            for (int y = 0; y < Height; ++y)
                ColorValues[x, y] = color;
        }
    }

    // We expect y to be from the full range
    public void SetColor(int x, int y, Color color)
    {
        if (x < FromWidth || x > ToWidth || y < 0 || y >= Height)
        {
            Debug.Assert(false);
            return;
        }

        var localX = x - FromWidth;

        ColorValues[localX, y] = color;
    }
}
