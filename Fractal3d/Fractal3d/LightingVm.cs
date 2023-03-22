namespace Fractal3d;

using BasicWpfLibrary;
using ImageCalculator;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

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

        AllowedLightTypes = new ObservableCollection<LightType>
        {
            LightType.PointLight, LightType.DirectionalLight
        };

        AllowedReflectionTypes = new ObservableCollection<ReflectionType>
        {
            ReflectionType.BlinnPhong, ReflectionType.Phong
        };

        AllowedLightComboModes = new ObservableCollection<LightCombinationMode>
        {
            LightCombinationMode.Average, LightCombinationMode.Sum
        };

        SelectedLightType = _fractalParams.Lights[SelectedLightIndex - 1].LightType;
        SelectedLightComboMode = _fractalParams.LightComboMode;
        UpdateLightIndices();
        SelectedReflectionType = _fractalParams.Lights[SelectedLightIndex-1].ReflectionType;

        _addLightCommand = new RelayCommand(_ => AddLight(), _ => true);
        _deleteLightCommand = new RelayCommand(_ => DeleteLight(), _ => CanDeleteLight());
    }

    public RelayCommand IsVisibleChangedCommand { get; }

    public void ExecuteIsVisibleChangedCommand(DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue != null && (bool)e.NewValue)
        {
            NormalDistance = _fractalParams.NormalDistance;
            SelectedLightComboMode = _fractalParams.LightComboMode;
            AmbientPower = _fractalParams.AmbientPower;
            UpdateFields();
        }
    }

    protected void UpdateFields()
    {
        var light = CurrentLight();
        
        DiffuseColorRed = light.DiffuseColor.X;
        DiffuseColorGreen = light.DiffuseColor.Y;
        DiffuseColorBlue = light.DiffuseColor.Z;
        DiffusePower = light.DiffusePower;
        SpecularColorRed = light.SpecularColor.X;
        SpecularColorGreen = light.SpecularColor.Y;
        SpecularColorBlue = light.SpecularColor.Z;
        SpecularPower = light.SpecularPower;
        Shininess = light.Shininess;

        // Non-validated fields
        LightPositionX = light.Position.X;
        LightPositionY = light.Position.Y;
        LightPositionZ = light.Position.Z;
        SelectedReflectionType = light.ReflectionType;
        SelectedLightType = light.LightType;
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

    private ObservableCollection<LightCombinationMode> _allowedLightComboModes;
    public ObservableCollection<LightCombinationMode> AllowedLightComboModes
    {
        get => _allowedLightComboModes;
        set => SetProperty(ref _allowedLightComboModes, value);
    }

    public LightCombinationMode SelectedLightComboMode
    {
        get => _fractalParams.LightComboMode;
        set
        {
            _fractalParams.LightComboMode = value;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    private ObservableCollection<int> _allowedLightIndices;
    public ObservableCollection<int> AllowedLightIndices
    {
        get => _allowedLightIndices;
        set => SetProperty(ref _allowedLightIndices, value);
    }

    private int _selectedLightIndex = 1;

    public int SelectedLightIndex
    {
        get => _selectedLightIndex;
        set
        {
            SetProperty(ref _selectedLightIndex, value);
            UpdateFields();
        }
    }

    protected Light CurrentLight()
    {
        if( _fractalParams.Lights.Count == 0) return new Light();

        if( SelectedLightIndex < 1 || SelectedLightIndex > _fractalParams.Lights.Count)
            return new Light();

        return _fractalParams.Lights[SelectedLightIndex-1];
    }

    private readonly RelayCommand _addLightCommand;
    public ICommand AddLightCommand => _addLightCommand;

    private readonly RelayCommand _deleteLightCommand;
    public ICommand DeleteLightCommand => _deleteLightCommand;

    protected void AddLight()
    {
        _fractalParams.Lights.Add(new Light());
        UpdateLightIndices();
    }

    protected void DeleteLight()
    {
        _fractalParams.Lights.RemoveAt(SelectedLightIndex-1);
        UpdateLightIndices();
    }

    protected bool CanDeleteLight()
    {
        return _fractalParams.Lights.Count > 1;
    }

    private void UpdateLightIndices()
    {
        ObservableCollection<int> indices = new();
        for (int i = 1; i <= _fractalParams.Lights.Count; i++)
        {
            indices.Add(i);
        }

        AllowedLightIndices = indices;
        SelectedLightIndex = 1;
    }

    public float LightPositionX
    {
        get => CurrentLight().Position.X;
        set
        {
            var light = CurrentLight();

            var pos = light.Position;
            pos.X = value;
            light.Position = pos;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float LightPositionY
    {
        get => CurrentLight().Position.Y;
        set
        {
            var light = CurrentLight();

            var pos = light.Position;
            pos.Y = value;
            light.Position = pos;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float LightPositionZ
    {
        get => CurrentLight().Position.Z;
        set
        {
            var light = CurrentLight();

            var pos = light.Position;
            pos.Z = value;
            light.Position = pos;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float DiffuseColorRed
    {
        get => CurrentLight().DiffuseColor.X;
        set
        {
            var light = CurrentLight();
            var color = light.DiffuseColor;
            color.X = value;
            light.DiffuseColor = color;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float DiffuseColorGreen
    {
        get => CurrentLight().DiffuseColor.Y;
        set
        {
            var light = CurrentLight();
            var color = light.DiffuseColor;
            color.Y = value;
            light.DiffuseColor = color;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float DiffuseColorBlue
    {
        get => CurrentLight().DiffuseColor.Z;
        set
        {
            var light = CurrentLight();
            var color = light.DiffuseColor;
            color.Z = value;
            light.DiffuseColor = color;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float DiffusePower
    {
        get => CurrentLight().DiffusePower;
        set
        {
            CurrentLight().DiffusePower = value;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float SpecularColorRed
    {
        get => CurrentLight().SpecularColor.X;
        set
        {
            var light = CurrentLight();
            var color = light.SpecularColor;
            color.X = value;
            light.SpecularColor = color;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float SpecularColorGreen
    {
        get => CurrentLight().SpecularColor.Y;
        set
        {
            var light = CurrentLight();
            var color = light.SpecularColor;
            color.Y = value;
            light.SpecularColor = color;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float SpecularColorBlue
    {
        get => CurrentLight().SpecularColor.Z;
        set
        {
            var light = CurrentLight();
            var color = light.SpecularColor;
            color.Z = value;
            light.SpecularColor = color;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float SpecularPower
    {
        get => CurrentLight().SpecularPower;
        set
        {
            CurrentLight().SpecularPower = value;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float Shininess
    {
        get => CurrentLight().Shininess;
        set
        {
            CurrentLight().Shininess = value;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float AmbientPower
    {
        get => _fractalParams.AmbientPower;
        set
        {
            _fractalParams.AmbientPower = value;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    private ObservableCollection<LightType> _allowedLightTypes;
    public ObservableCollection<LightType> AllowedLightTypes
    {
        get => _allowedLightTypes;
        set => SetProperty(ref _allowedLightTypes, value);
    }

    public LightType SelectedLightType
    {
        get => CurrentLight().LightType;
        set
        {
            var light = CurrentLight();
            light.LightType = value;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    private ObservableCollection<ReflectionType> _allowedReflectionTypes;
    public ObservableCollection<ReflectionType> AllowedReflectionTypes
    {
        get => _allowedReflectionTypes;
        set => SetProperty(ref _allowedReflectionTypes, value);
    }

    public ReflectionType SelectedReflectionType
    {
        get => CurrentLight().ReflectionType;
        set
        {
            var light = CurrentLight();
            light.ReflectionType = value;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }
}
