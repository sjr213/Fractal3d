namespace Fractal3d;

using BasicWpfLibrary;
using ImageCalculator;
using System;
using System.Collections.ObjectModel;
using System.Windows;

public class LightingVm : ViewModelBase
{
    private readonly FractalParams _fractalParams;
    private readonly Action<FractalParams> _onParamsChanged;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public LightingVm(FractalParams fractalParams, Action<FractalParams> onParamsChanged)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        _fractalParams = fractalParams;
        _onParamsChanged = onParamsChanged;
        IsVisibleChangedCommand = new RelayCommand(param => ExecuteIsVisibleChangedCommand(param is DependencyPropertyChangedEventArgs args ? args : default));

        AllowedLightingTypes = new ObservableCollection<LightingType>
        {
            LightingType.BlinnPhong, LightingType.Phong
        };
        SelectedLightingType = _fractalParams.Light.LightingType;
    }

    public RelayCommand IsVisibleChangedCommand { get; }

    public void ExecuteIsVisibleChangedCommand(DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue != null && (bool)e.NewValue)
        {
            NormalDistance = _fractalParams.NormalDistance;
            DiffuseColorRed = _fractalParams.Light.DiffuseColor.X;
            DiffuseColorGreen = _fractalParams.Light.DiffuseColor.Y;
            DiffuseColorBlue = _fractalParams.Light.DiffuseColor.Z;
            DiffusePower = _fractalParams.Light.DiffusePower;
            SpecularColorRed = _fractalParams.Light.SpecularColor.X;
            SpecularColorGreen = _fractalParams.Light.SpecularColor.Y;
            SpecularColorBlue = _fractalParams.Light.SpecularColor.Z;
            SpecularPower = _fractalParams.Light.SpecularPower;
            Shininess = _fractalParams.Light.Shininess;
            LightColorRed = _fractalParams.Light.LightColor.X;
            LightColorGreen = _fractalParams.Light.LightColor.Y;
            LightColorBlue = _fractalParams.Light.LightColor.Z;
            AmbientPower = _fractalParams.Light.AmbientPower;
        }

    }

    public float NormalDistance
    {
        get => _fractalParams.NormalDistance;
        set
        {
            _fractalParams.NormalDistance = value;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float LightPositionX
    {
        get => _fractalParams.Light.Position.X;
        set
        {
            if (Math.Abs(value - _fractalParams.Light.Position.X) < ParameterConstants.FloatTolerance)
                return;

            var pos = _fractalParams.Light.Position;
            pos.X = value;
            _fractalParams.Light.Position = pos;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float LightPositionY
    {
        get => _fractalParams.Light.Position.Y;
        set
        {
            if (Math.Abs(value - _fractalParams.Light.Position.Y) < ParameterConstants.FloatTolerance)
                return;

            var pos = _fractalParams.Light.Position;
            pos.Y = value;
            _fractalParams.Light.Position = pos;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float LightPositionZ
    {
        get => _fractalParams.Light.Position.Z;
        set
        {
            if (Math.Abs(value - _fractalParams.Light.Position.Z) < ParameterConstants.FloatTolerance)
                return;

            var pos = _fractalParams.Light.Position;
            pos.Z = value;
            _fractalParams.Light.Position = pos;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float DiffuseColorRed
    {
        get => _fractalParams.Light.DiffuseColor.X;
        set
        {
            var color = _fractalParams.Light.DiffuseColor;
            color.X = value;
            _fractalParams.Light.DiffuseColor = color;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float DiffuseColorGreen
    {
        get => _fractalParams.Light.DiffuseColor.Y;
        set
        {
            var color = _fractalParams.Light.DiffuseColor;
            color.Y = value;
            _fractalParams.Light.DiffuseColor = color;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float DiffuseColorBlue
    {
        get => _fractalParams.Light.DiffuseColor.Z;
        set
        {
            var color = _fractalParams.Light.DiffuseColor;
            color.Z = value;
            _fractalParams.Light.DiffuseColor = color;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float DiffusePower
    {
        get => _fractalParams.Light.DiffusePower;
        set
        {
            _fractalParams.Light.DiffusePower = value;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float SpecularColorRed
    {
        get => _fractalParams.Light.SpecularColor.X;
        set
        {
            var color = _fractalParams.Light.SpecularColor;
            color.X = value;
            _fractalParams.Light.SpecularColor = color;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float SpecularColorGreen
    {
        get => _fractalParams.Light.SpecularColor.Y;
        set
        {
            var color = _fractalParams.Light.SpecularColor;
            color.Y = value;
            _fractalParams.Light.SpecularColor = color;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float SpecularColorBlue
    {
        get => _fractalParams.Light.SpecularColor.Z;
        set
        {
            var color = _fractalParams.Light.SpecularColor;
            color.Z = value;
            _fractalParams.Light.SpecularColor = color;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float SpecularPower
    {
        get => _fractalParams.Light.SpecularPower;
        set
        {
            _fractalParams.Light.SpecularPower = value;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float Shininess
    {
        get => _fractalParams.Light.Shininess;
        set
        {
            _fractalParams.Light.Shininess = value;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float LightColorRed
    {
        get => _fractalParams.Light.LightColor.X;
        set
        {
            var color = _fractalParams.Light.LightColor;
            color.X = value;
            _fractalParams.Light.LightColor = color;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float LightColorGreen
    {
        get => _fractalParams.Light.LightColor.Y;
        set
        {
            var color = _fractalParams.Light.LightColor;
            color.Y = value;
            _fractalParams.Light.LightColor = color;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float LightColorBlue
    {
        get => _fractalParams.Light.LightColor.Z;
        set
        {
            var color = _fractalParams.Light.LightColor;
            color.Z = value;
            _fractalParams.Light.LightColor = color;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float AmbientPower
    {
        get => _fractalParams.Light.AmbientPower;
        set
        {
            _fractalParams.Light.AmbientPower = value;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    private ObservableCollection<LightingType> _allowedLightingTypes;
    public ObservableCollection<LightingType> AllowedLightingTypes
    {
        get => _allowedLightingTypes;
        set => SetProperty(ref _allowedLightingTypes, value);
    }

    public LightingType SelectedLightingType
    {
        get => _fractalParams.Light.LightingType;
        set
        {
            if (value == _fractalParams.Light.LightingType)
                return;
            _fractalParams.Light.LightingType = value;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }
}
