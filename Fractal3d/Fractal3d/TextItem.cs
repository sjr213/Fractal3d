using System;

namespace Fractal3d;

using BasicWpfLibrary;

public class TextItem : ViewModelBase
{
    public const int CanvasLeft = 0;
    public const int FixedWidth = 30;
    public const int FixedHeight = 15;

    public TextItem(double relativePosition, int numberOfColors, double canvasRight)
    {
        var x = CanvasLeft + relativePosition * canvasRight;
        X = x - (double)FixedWidth / 2;
        Y = 2;
        var colorNumber = (int)(relativePosition * numberOfColors + 0.4999);
        Text = $"{colorNumber}";
    }

    private double _x;
    public double X
    {
        get => _x;
        set => SetProperty(ref _x, value);
    }

    public double Width => FixedWidth;

    private double _y;
    public double Y
    {
        get => _y;
        set => SetProperty(ref _y, value);
    }

    public double Height => FixedHeight;

    private string _text = String.Empty;

    public string Text
    {
        get => _text;
        set => SetProperty(ref _text, value);
    }

}

