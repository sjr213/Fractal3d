using System;
using System.Drawing;
using System.Windows.Controls;
using System.Windows.Input;
using BasicWpfLibrary;
using FractureCommonLib;

namespace Fractal3d;

public class ColorPointVm : ViewModelBase
{
    private const double FloatTolerance = 1e-6;
    private ColorPoint _colorPt = new();
    private int _numberOfColors = 1000;
    private readonly IColorPointVmParent _parent;

    public ColorPointVm(IColorPointVmParent parent)
    {
        UpdateChildProperties();
        _parent = parent;
        _updateCommand = new RelayCommand(_ => OnUpdate(), _ => CanUpdate());
        _addCommand = new RelayCommand(_ => OnAdd(), _ => CanAdd());
        _deleteCommand = new RelayCommand(_ => OnDelete(), _ => CanDelete());
    }

    public ColorPoint ColorPt
    {
        get => _colorPt;
        set 
        { 
            _colorPt = value;
            OnPropertyChanged(nameof(ColorPoint));

            UpdateChildProperties();
        }
    }

    private void UpdateChildProperties()
    {
        Alpha = _colorPt.PointColor.A;
        Red = _colorPt.PointColor.R;
        Green = _colorPt.PointColor.G;
        Blue = _colorPt.PointColor.B;
        ColorPosition = _colorPt.Position;
        ColorIndex = ColorPt.GetColorIndex(_numberOfColors);
    }

    private byte _alpha;
    public byte Alpha
    {
        get => _alpha;

        set
        {
            if (value == _alpha)
                return;

            _alpha = value;
            _colorPt.PointColor = Color.FromArgb(value, _colorPt.PointColor);
            OnPropertyChanged();
        }
    }

    private byte _red;
    public byte Red
    {
        get => _red;

        set
        {
            if (value == _red)
                return;

            _red = value;
            _colorPt.PointColor = Color.FromArgb(_colorPt.PointColor.A, value, _colorPt.PointColor.G, _colorPt.PointColor.B);
            OnPropertyChanged();
        }
    }

    private byte _green;
    public byte Green
    {
        get => _green;

        set
        {
            if (value == _green)
                return;

            _green = value;
            _colorPt.PointColor = Color.FromArgb(_colorPt.PointColor.A, _colorPt.PointColor.R, value, _colorPt.PointColor.B);
            OnPropertyChanged();
        }
    }

    private byte _blue;
    public byte Blue
    { 
        get => _blue;

        set
        {
            if (value == _blue)
                return;

            _blue = value;
            _colorPt.PointColor = Color.FromArgb(_colorPt.PointColor.A, _colorPt.PointColor.R, _colorPt.PointColor.G, value);
            OnPropertyChanged();
        }
    }

    private double _position;
    public double ColorPosition
    {
        get => Math.Round(_position,6);

        set
        {
            if (Math.Abs(value - _position) < FloatTolerance)
                return;

            value = Math.Min(0, value);
            value = Math.Max(1.0, value);

            _colorPt.Position = _position = value;
            OnPropertyChanged();

            ColorIndex = _colorPt.GetColorIndex(_numberOfColors);
        }
    }

    public int NumberOfColors
    {
        get => _numberOfColors;

        set
        {
            if (value == _numberOfColors)
                return;

            if (value < 2)
                return;

            _numberOfColors = value;
            OnPropertyChanged();

            ColorIndex = ColorPt.GetColorIndex(_numberOfColors);
        }
    }

    private int _colorIndex;
    public int ColorIndex
    {
        get => _colorIndex;

        set
        {
            var num = _colorIndex;
            if (value == num)
                return;

            if (value < 0)
                return;

            if (_numberOfColors < 2)
                return;

            if (_numberOfColors <= value)
                return;

            _colorIndex = value;
            _colorPt.SetPositionByIndex(value, _numberOfColors);
            OnPropertyChanged();

            ColorPosition = _colorPt.Position;
        }

    }

    private int? _pinNumber;
    public int? PinNumber
    {
        get => _pinNumber;
        set
        {
            if (value == _pinNumber)
                return;
            _pinNumber = value;
            OnPropertyChanged();
        }
    }

    private readonly RelayCommand _updateCommand;
    public ICommand UpdateCommand => _updateCommand;

    private readonly RelayCommand _addCommand;
    public ICommand AddCommand => _addCommand;

    private readonly RelayCommand _deleteCommand;
    public ICommand DeleteCommand => _deleteCommand;

    private void OnUpdate()
    {
        if(PinNumber != null)
            _parent.UpdateRectItem(_colorPt, PinNumber.Value - 1);
    }

    private bool CanUpdate()
    {
        return PinNumber != null;
    }

    private void OnAdd()
    {
        _parent.AddRectItem(_colorPt);
    }

    private bool CanAdd()
    {
        return _parent.CanAdd(ColorPosition);
    }

    private void OnDelete()
    {
        if (PinNumber != null)
            _parent.DeleteRectItem(PinNumber.Value - 1);
    }

    private bool CanDelete()
    {
        return PinNumber != null;
    }
}
