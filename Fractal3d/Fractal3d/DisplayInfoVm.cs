using System.Collections.ObjectModel;

namespace Fractal3d;

using BasicWpfLibrary;
using FractureCommonLib;
using System;
using System.Windows;

public class DisplayInfoVm : ViewModelBase
{
    private const double DoubleTolerance = 1e-4;

    private readonly DisplayInfo _displayInfo;
    private readonly Action<DisplayInfo> _onDisplayInfoChanged;

    public DisplayInfoVm(DisplayInfo displayInfo, Action<DisplayInfo> displayInfoChanged)
    {
        _displayInfo = displayInfo;
        _onDisplayInfoChanged = displayInfoChanged;

        AllowedColorModes = new ObservableCollection<DisplayMode>
        {
            DisplayMode.Off, DisplayMode.Contrast, DisplayMode.Hsl
        };

        ContrastVisibility = _displayInfo.Mode == DisplayMode.Contrast ? Visibility.Visible : Visibility.Collapsed;
        HslVisibility = _displayInfo.Mode == DisplayMode.Hsl ? Visibility.Visible : Visibility.Collapsed;
    }

    private ObservableCollection<DisplayMode> _allowedColorModes = null!;
    public ObservableCollection<DisplayMode> AllowedColorModes
    {
        get => _allowedColorModes;
        set => SetProperty(ref _allowedColorModes, value);
    }

    public DisplayMode SelectedColorMode
    {
        get => _displayInfo.Mode;
        set
        {
            if (value == _displayInfo.Mode)
                return;
            _displayInfo.Mode = value;
            OnPropertyChanged();
            _onDisplayInfoChanged(_displayInfo);

            ContrastVisibility = _displayInfo.Mode == DisplayMode.Contrast ? Visibility.Visible : Visibility.Collapsed;
            HslVisibility = _displayInfo.Mode == DisplayMode.Hsl ? Visibility.Visible : Visibility.Collapsed;
        }
    }
     
    private Visibility _contrastVisibility = Visibility.Collapsed;
    public Visibility ContrastVisibility
    {
        get => _contrastVisibility;
        set => SetProperty(ref _contrastVisibility, value);
    }

    private Visibility _hslVisibility = Visibility.Collapsed;
    public Visibility HslVisibility
    {
        get => _hslVisibility;
        set => SetProperty(ref _hslVisibility, value);
    }

    public byte MinRed
    {
        get => _displayInfo.MinRgb[0];

        set
        {
            if (value > MaxRed)
                MaxRed = value;

            _displayInfo.MinRgb[0] = value;
            OnPropertyChanged();
            _onDisplayInfoChanged(_displayInfo);
        }
    }

    public byte MaxRed
    {
        get => _displayInfo.MaxRgb[0];

        set
        {
            if (value < MinRed)
                MinRed = value;

            _displayInfo.MaxRgb[0] = value;
            OnPropertyChanged();
            _onDisplayInfoChanged(_displayInfo);
        }
    }

    public byte MinGreen
    {
        get => _displayInfo.MinRgb[1];

        set
        {
            if (value > MaxGreen)
                MaxGreen = value;

            _displayInfo.MinRgb[1] = value;
            OnPropertyChanged();
            _onDisplayInfoChanged(_displayInfo);
        }
    }

    public byte MaxGreen
    {
        get => _displayInfo.MaxRgb[1];

        set
        {
            if (value < MinGreen)
                MinGreen = value;

            _displayInfo.MaxRgb[1] = value;
            OnPropertyChanged();
            _onDisplayInfoChanged(_displayInfo);
        }
    }

    public byte MinBlue
    {
        get => _displayInfo.MinRgb[2];

        set
        {
            if (value > MaxBlue)
                MaxBlue = value;

            _displayInfo.MinRgb[2] = value;
            OnPropertyChanged();
            _onDisplayInfoChanged(_displayInfo);
        }
    }

    public byte MaxBlue
    {
        get => _displayInfo.MaxRgb[2];

        set
        {
            if (value < MinBlue)
                MinBlue = value;

            _displayInfo.MaxRgb[2] = value;
            OnPropertyChanged();
            _onDisplayInfoChanged(_displayInfo);
        }
    }

    public bool IsHueEnabled
    {
        get => _displayInfo.Hue;

        set
        {
            if (value == _displayInfo.Hue)
                return;

            _displayInfo.Hue = value;
            OnPropertyChanged();
            _onDisplayInfoChanged(_displayInfo);
        }
    }

    public bool IsSaturationEnabled
    {
        get => _displayInfo.Saturation;

        set
        {
            if (value == _displayInfo.Saturation)
                return;

            _displayInfo.Saturation = value;
            OnPropertyChanged();
            _onDisplayInfoChanged(_displayInfo);
        }
    }

    public bool IsLightnessEnabled
    {
        get => _displayInfo.Lightness;

        set
        {
            if (value == _displayInfo.Lightness)
                return;

            _displayInfo.Lightness = value;
            OnPropertyChanged();
            _onDisplayInfoChanged(_displayInfo);
        }
    }

    public double MinHue
    {
        get => Math.Round(_displayInfo.MinHue,1,MidpointRounding.AwayFromZero);

        set
        {
            if (Math.Abs(value - _displayInfo.MinHue) < DoubleTolerance)
                return;

            if (value < 0)
                value = 0;
            else if (value > DisplayInfo.MAX_HUE)
                value = DisplayInfo.MAX_HUE;

            if (MaxHue < value)
                MaxHue = value;

            _displayInfo.MinHue = value;
            OnPropertyChanged();
            _onDisplayInfoChanged(_displayInfo);
        }
    }

    public double MaxHue
    {
        get => Math.Round(_displayInfo.MaxHue, 1, MidpointRounding.AwayFromZero);

        set
        {
            if(Math.Abs(value - _displayInfo.MaxHue) < DoubleTolerance)
                return;

            if (value < 0)
                value = 0;
            else if (value > DisplayInfo.MAX_HUE)
                value = DisplayInfo.MAX_HUE;

            if(MinHue > value) 
                MinHue = value;

            _displayInfo.MaxHue = value;
            OnPropertyChanged();
            _onDisplayInfoChanged(_displayInfo);
        }
    }

    public double MinSaturation
    {
        get => Math.Round(_displayInfo.MinSaturation, 3, MidpointRounding.AwayFromZero);

        set
        {
            if (Math.Abs(value - _displayInfo.MinSaturation) < DoubleTolerance)
                return;

            if (value < 0)
                value = 0;
            else if (value > 1)
                value = 1;

            if (value > MaxSaturation)
                MaxSaturation = value;

            _displayInfo.MinSaturation = value;
            OnPropertyChanged();
            _onDisplayInfoChanged(_displayInfo);
        }
    }

    public double MaxSaturation
    {
        get => Math.Round(_displayInfo.MaxSaturation, 3, MidpointRounding.AwayFromZero);

        set
        {
            if (Math.Abs(value - _displayInfo.MaxSaturation) < DoubleTolerance)
                return;

            if (value < 0)
                value = 0;
            else if (value > 1)
                value = 1;

            if (value < MinSaturation)
                MinSaturation = value;

            _displayInfo.MaxSaturation = value;
            OnPropertyChanged();
            _onDisplayInfoChanged(_displayInfo);
        }
    }

    public double MinLightness
    {
        get => Math.Round(_displayInfo.MinLightness, 3, MidpointRounding.AwayFromZero);

        set
        {
            if (Math.Abs(value - _displayInfo.MinLightness) < DoubleTolerance)
                return;

            if (value < 0)
                value = 0;
            else if (value > 1)
                value = 1;

            if (value > MaxLightness)
                MaxLightness = value;

            _displayInfo.MinLightness = value;
            OnPropertyChanged();
            _onDisplayInfoChanged(_displayInfo);
        }
    }

    public double MaxLightness
    {
        get => Math.Round(_displayInfo.MaxLightness, 3, MidpointRounding.AwayFromZero);

        set
        {
            if (Math.Abs(value - _displayInfo.MaxLightness) < DoubleTolerance)
                return;

            if (value < 0)
                value = 0;
            else if (value > 1)
                value = 1;

            if (value < MinLightness)
                MinLightness = value;

            _displayInfo.MaxLightness = value;
            OnPropertyChanged();
            _onDisplayInfoChanged(_displayInfo);
        }
    }
}


