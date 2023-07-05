using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using BasicWpfLibrary;
using FractureCommonLib;

namespace Fractal3d;

// see https://stackoverflow.com/questions/22324359/add-n-rectangles-to-canvas-with-mvvm-in-wpf
public class RectItem : ViewModelBase, IComparable<RectItem>
{
    public const int ItemWidth = 10;
    public const int ItemHeight = 50;
    public const int ItemTop = 50;
    public const double ItemGap = 0.1;
    public const int CanvasLeft = 0;
    public const int CanvasRight = 1000;

    public RectItem(ObservableCollection<RectItem> parent, Action updatePins, Action<RectItem?> selectRectItem, double canvasWidth)
    {
        _parent = parent;
        _updatePins = updatePins;
        _selectRectItem = selectRectItem;
        _canvasWidth = canvasWidth;

#pragma warning disable CS8604 // Possible null reference argument.
        MoveCommand = new RelayCommand(param => ExecuteMouseMove(param as MouseEventArgs));
        LeftMouseDownCommand = new RelayCommand(param => ExecuteLeftMouseDown(param as MouseEventArgs));
        LeftMouseUpCommand = new RelayCommand(_ => ExecuteLeftMouseUp());
        LeaveCommand = new RelayCommand(_ => ExecuteMouseLeaveCommand());
#pragma warning restore CS8604 // Possible null reference argument.
    }

    readonly ObservableCollection<RectItem> _parent;
    private readonly Action _updatePins;
    private readonly Action<RectItem?> _selectRectItem;

    private double _canvasX;
    private bool _rectCaptured;
    private readonly double _canvasWidth;

    public static RectItem MakeColorRect(ColorPoint pt, ObservableCollection<RectItem> parent, Action updatePins, Action<RectItem?> selectRectItem, double canvasWidth)
    {
        int pos = (int)(pt.Position * canvasWidth);

          RectItem item = new(parent, updatePins, selectRectItem, canvasWidth)
        {
            X = pos - ItemWidth/2,
            Y = ItemTop,
            Width = ItemWidth,
            Height = ItemHeight,
            PinColor = ImageUtil.FromDrawingToMediaColor(pt.PointColor)
        };

        return item;
    }

    public static ColorPoint GetColorPoint(RectItem item)
    {
        double x = item.X + (double)ItemWidth/2;
        double pos = x / item._canvasWidth;

        System.Drawing.Color pinColor = ImageUtil.FromMediaToDrawingColor(item.PinColor);
        return new ColorPoint(pinColor, pos);
    }

    public void SetColorPoint(ColorPoint pt)
    {
        int pos = (int)(pt.Position * _canvasWidth);
        X = pos - ItemWidth / 2;
        PinColor = ImageUtil.FromDrawingToMediaColor(pt.PointColor);
    }

    public int CompareTo(RectItem? other)
    {
        try
        {
            if (other == null)
                return 1;

            double dx = X - other.X;
            if (Math.Abs(dx) < 0.01)
                return 0;

            return dx > 0 ? -1 : 1;
        }
        catch (InvalidCastException)
        {
            return -1;
        }
    }

    private double _x;
    public double X
    {
        get => _x;
        set => SetProperty(ref _x, value);
    }

    private double _y;
    public double Y
    {
        get => _y;
        set => SetProperty(ref _y, value);
    }

    private double _width;
    public double Width
    {
        get => _width;
        set => SetProperty(ref _width, value);
    }

    private double _height;
    public double Height
    {
        get => _height;
        set => SetProperty(ref _height, value);
    }

    private Color _color = Colors.Black;
    public Color PinColor
    {
        get => _color;
        set => SetProperty(ref _color, value);
    }

    // Mouse commands
    #region

    private Canvas? GetCanvas(object obj)
    {
        var shape = (Shape)obj;

        return shape.FindParentOfType<Canvas>();
    }

    private double GetLeftBorder()
    {
        RectItem? left = null;
        foreach(var item in _parent)
        {
            if (item.X >= X)
                break;

            left = item;
        }

        if (left == null)
            return CanvasLeft - ItemGap;

        return left.X;
    }

    private double GetRightBorder()
    {
        RectItem? right = null;
        foreach (var item in _parent)
        {
            if (item.X <= X)
                continue;

            right = item;
            break;
        }

        if (right == null)
            return _canvasWidth + ItemGap;

        return right.X;
    }

    public RelayCommand MoveCommand { get; }

    private void ExecuteMouseMove(MouseEventArgs e)
    {
        if (_rectCaptured == false)
            return;

        var canvas = GetCanvas(e.Source);
        if (canvas == null)
            return;

        var canvasPos = e.GetPosition(canvas);
        if(canvasPos.Y < ItemTop || canvasPos.Y > ItemTop + ItemHeight)
        {
            _rectCaptured = false;
            return;
        }

        var newX = X + canvasPos.X - _canvasX;

        var leftBorder = GetLeftBorder();
        var rightBorder = GetRightBorder(); 
        if(newX <= leftBorder)
        {
            newX = leftBorder + ItemGap;
            _rectCaptured = false;
        }
        else if(newX >= rightBorder)
        {
            newX = rightBorder - ItemGap;
            _rectCaptured = false;
        }

        X = newX;
        _canvasX = canvasPos.X;

        if(_rectCaptured == false)
            _updatePins();
    }

    public RelayCommand LeftMouseDownCommand { get; }

    private void ExecuteLeftMouseDown(MouseEventArgs e)
    {
        var canvas = GetCanvas(e.Source);
        if (canvas == null)
            return;

        _rectCaptured = true;
        _canvasX = e.GetPosition(canvas).X;
    }

    public RelayCommand LeftMouseUpCommand { get; }

    private void ExecuteLeftMouseUp()
    {
        if (_rectCaptured)
        {
            _updatePins();
            _selectRectItem(this);
        }
        
        _rectCaptured = false;
    }

    public RelayCommand LeaveCommand { get; }

    private void ExecuteMouseLeaveCommand()
    {
        if (_rectCaptured)
        {
            _updatePins();
            _selectRectItem(this);
        }

        _rectCaptured = false;
    }

    #endregion
}
