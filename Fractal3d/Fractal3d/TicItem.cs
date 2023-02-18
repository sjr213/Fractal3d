namespace Fractal3d;

using BasicWpfLibrary;

public class TicItem : ViewModelBase
{
    public const int CanvasLeft = 0;
    public const int CanvasRight = 1000;

    public TicItem(double relativePosition, int numberOfColors)
    {
        var x = CanvasLeft + relativePosition * CanvasRight;
        X1 = X2 = x;
        Y1 = 0;
        Y2 = 10;
        ColorNumber = (int)(relativePosition * numberOfColors + 0.4999);
    }

    private double _x1;
    public double X1
    {
        get => _x1;
        set => SetProperty(ref _x1, value);
    }

    private double _x2;
    public double X2
    {
        get => _x2;
        set => SetProperty(ref _x2, value);
    }

    private double _y1;
    public double Y1
    {
        get => _y1;
        set => SetProperty(ref _y1, value);
    }

    private double _y2;
    public double Y2
    {
        get => _y2;
        set => SetProperty(ref _y2, value);
    }

    private int _colorNumber;

    public int ColorNumber
    {
        get => _colorNumber;
        set => SetProperty(ref _colorNumber, value);    
    }

    private int _thickness = 2;

    public int Thickness
    {
        get => _thickness;
        set => SetProperty(ref _thickness, value);
    }

    private System.Windows.Media.Brush _brush = System.Windows.Media.Brushes.Black;

    public System.Windows.Media.Brush Stroke
    {
        get => _brush; 
        set => SetProperty(ref _brush, value);
    }
}

