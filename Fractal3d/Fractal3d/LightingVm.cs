namespace Fractal3d;

using BasicWpfLibrary;
using ImageCalculator;
using System;
using System.Collections.ObjectModel;

public class LightingVm : ViewModelBase
{
    private FractalParams _fractalParams;
    private Action<FractalParams> _onParamsChanged;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public LightingVm(FractalParams fractalParams, Action<FractalParams> onParamsChanged)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        _fractalParams = fractalParams;
        _onParamsChanged = onParamsChanged;

        AllowedLightingTypes = new ObservableCollection<LightingType>
        {
            LightingType.BlinnPhong, LightingType.Phong
        };
        SelectedLightingType = _fractalParams.Light.LightingType;
    }

    public float NormalDistance
    {
        get { return _fractalParams.NormalDistance; }
        set
        {
            if (value == _fractalParams.NormalDistance)
                return;
            _fractalParams.NormalDistance = value;
            OnPropertyChanged(nameof(NormalDistance));
            _onParamsChanged(_fractalParams);
        }
    }

    public float LightPositionX
    {
        get { return _fractalParams.Light.Position.X; }
        set
        {
            if (value == _fractalParams.Light.Position.X)
                return;

            var pos = _fractalParams.Light.Position;
            pos.X = value;
            _fractalParams.Light.Position = pos;
            OnPropertyChanged(nameof(LightPositionX));
            _onParamsChanged(_fractalParams);
        }
    }

    public float LightPositionY
    {
        get { return _fractalParams.Light.Position.Y; }
        set
        {
            if (value == _fractalParams.Light.Position.Y)
                return;

            var pos = _fractalParams.Light.Position;
            pos.Y = value;
            _fractalParams.Light.Position = pos;
            OnPropertyChanged(nameof(LightPositionY));
            _onParamsChanged(_fractalParams);
        }
    }

    public float LightPositionZ
    {
        get { return _fractalParams.Light.Position.Z; }
        set
        {
            if (value == _fractalParams.Light.Position.Z)
                return;

            var pos = _fractalParams.Light.Position;
            pos.Z = value;
            _fractalParams.Light.Position = pos;
            OnPropertyChanged(nameof(LightPositionZ));
            _onParamsChanged(_fractalParams);
        }
    }

    public float DiffuseColorRed
    {
        get { return _fractalParams.Light.DiffuseColor.X; }
        set
        {
            if (value == _fractalParams.Light.DiffuseColor.X)
                return;

            var color = _fractalParams.Light.DiffuseColor;
            color.X = value;
            _fractalParams.Light.DiffuseColor = color;
            OnPropertyChanged(nameof(DiffuseColorRed));
            _onParamsChanged(_fractalParams);
        }
    }

    public float DiffuseColorGreen
    {
        get { return _fractalParams.Light.DiffuseColor.Y; }
        set
        {
            if (value == _fractalParams.Light.DiffuseColor.Y)
                return;

            var color = _fractalParams.Light.DiffuseColor;
            color.Y = value;
            _fractalParams.Light.DiffuseColor = color;
            OnPropertyChanged(nameof(DiffuseColorGreen));
            _onParamsChanged(_fractalParams);
        }
    }

    public float DiffuseColorBlue
    {
        get { return _fractalParams.Light.DiffuseColor.Z; }
        set
        {
            if (value == _fractalParams.Light.DiffuseColor.Z)
                return;

            var color = _fractalParams.Light.DiffuseColor;
            color.Z = value;
            _fractalParams.Light.DiffuseColor = color;
            OnPropertyChanged(nameof(DiffuseColorBlue));
            _onParamsChanged(_fractalParams);
        }
    }

    public float DiffusePower
    {
        get { return _fractalParams.Light.DiffusePower; }
        set
        {
            if (value == _fractalParams.Light.DiffusePower)
                return;

            _fractalParams.Light.DiffusePower = value;
            OnPropertyChanged(nameof(DiffusePower));
            _onParamsChanged(_fractalParams);
        }
    }

    public float SpecularColorRed
    {
        get { return _fractalParams.Light.SpecularColor.X; }
        set
        {
            if (value == _fractalParams.Light.SpecularColor.X)
                return;

            var color = _fractalParams.Light.SpecularColor;
            color.X = value;
            _fractalParams.Light.SpecularColor = color;
            OnPropertyChanged(nameof(SpecularColorRed));
            _onParamsChanged(_fractalParams);
        }
    }

    public float SpecularColorGreen
    {
        get { return _fractalParams.Light.SpecularColor.Y; }
        set
        {
            if (value == _fractalParams.Light.SpecularColor.Y)
                return;

            var color = _fractalParams.Light.SpecularColor;
            color.Y = value;
            _fractalParams.Light.SpecularColor = color;
            OnPropertyChanged(nameof(SpecularColorGreen));
            _onParamsChanged(_fractalParams);
        }
    }

    public float SpecularColorBlue
    {
        get { return _fractalParams.Light.SpecularColor.Z; }
        set
        {
            if (value == _fractalParams.Light.SpecularColor.Z)
                return;

            var color = _fractalParams.Light.SpecularColor;
            color.Z = value;
            _fractalParams.Light.SpecularColor = color;
            OnPropertyChanged(nameof(SpecularColorBlue));
            _onParamsChanged(_fractalParams);
        }
    }

    public float SpecularPower
    {
        get { return _fractalParams.Light.SpecularPower; }
        set
        {
            if (value == _fractalParams.Light.SpecularPower)
                return;

            _fractalParams.Light.SpecularPower = value;
            OnPropertyChanged(nameof(SpecularPower));
            _onParamsChanged(_fractalParams);
        }
    }

    public float Shininess
    {
        get { return _fractalParams.Light.Shininess; }
        set
        {
            if (value == _fractalParams.Light.Shininess)
                return;

            _fractalParams.Light.Shininess = value;
            OnPropertyChanged(nameof(Shininess));
            _onParamsChanged(_fractalParams);
        }
    }

    public float LightColorRed
    {
        get { return _fractalParams.Light.LightColor.X; }
        set
        {
            if (value == _fractalParams.Light.LightColor.X)
                return;

            var color = _fractalParams.Light.LightColor;
            color.X = value;
            _fractalParams.Light.LightColor = color;
            OnPropertyChanged(nameof(LightColorRed));
            _onParamsChanged(_fractalParams);
        }
    }

    public float LightColorGreen
    {
        get { return _fractalParams.Light.LightColor.Y; }
        set
        {
            if (value == _fractalParams.Light.LightColor.Y)
                return;

            var color = _fractalParams.Light.LightColor;
            color.Y = value;
            _fractalParams.Light.LightColor = color;
            OnPropertyChanged(nameof(LightColorGreen));
            _onParamsChanged(_fractalParams);
        }
    }

    public float LightColorBlue
    {
        get { return _fractalParams.Light.LightColor.Z; }
        set
        {
            if (value == _fractalParams.Light.LightColor.Z)
                return;

            var color = _fractalParams.Light.LightColor;
            color.Z = value;
            _fractalParams.Light.LightColor = color;
            OnPropertyChanged(nameof(LightColorBlue));
            _onParamsChanged(_fractalParams);
        }
    }

    public float ScreenGamma
    {
        get { return _fractalParams.Light.ScreenGamma; }
        set
        {
            if (value == _fractalParams.Light.ScreenGamma)
                return;

            _fractalParams.Light.ScreenGamma = value;
            OnPropertyChanged(nameof(ScreenGamma));
            _onParamsChanged(_fractalParams);
        }
    }

    public float AmbientPower
    {
        get { return _fractalParams.Light.AmbientPower; }
        set
        {
            if (value == _fractalParams.Light.AmbientPower)
                return;

            _fractalParams.Light.AmbientPower = value;
            OnPropertyChanged(nameof(AmbientPower));
            _onParamsChanged(_fractalParams);
        }
    }

    private ObservableCollection<LightingType> _allowedLightingTypes;
    public ObservableCollection<LightingType> AllowedLightingTypes
    {
        get => _allowedLightingTypes;
        set { SetProperty(ref _allowedLightingTypes, value); }
    }

    public LightingType SelectedLightingType
    {
        get => _fractalParams.Light.LightingType;
        set
        {
            if (value == _fractalParams.Light.LightingType)
                return;
            _fractalParams.Light.LightingType = value;
            OnPropertyChanged(nameof(SelectedLightingType));
            _onParamsChanged(_fractalParams);
        }
    }
}
