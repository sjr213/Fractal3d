using System.Collections.Generic;

namespace Fractal3d;

using BasicWpfLibrary;
using ImageCalculator;
using System;
using System.Collections.ObjectModel;
using System.Windows;

public class ParameterVm : ViewModelBase
{
    private readonly FractalParams _fractalParams;
    private readonly Action<FractalParams> _onParamsChanged;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public ParameterVm(FractalParams fractalParams, Action<FractalParams> paramsChanged)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        _fractalParams = fractalParams;
        _onParamsChanged = paramsChanged;
        IsVisibleChangedCommand = new RelayCommand(param => ExecuteIsVisibleChangedCommand(param is DependencyPropertyChangedEventArgs args ? args : default));

        AllowedShaderTypes = new ObservableCollection<ShaderType>
        {
            ShaderType.FractalShader, ShaderType.CraneShader, ShaderType.CranePixel, ShaderType.CraneRaymarch, ShaderType.ShapeShader
        };  

        AllowedSceneTypes = new List<ShaderSceneType>
        {
            ShaderSceneType.Sphere, ShaderSceneType.Box, ShaderSceneType.Torus
        };

        SelectedShaderType = _fractalParams.ShaderType;
        SelectedQuatEquationType = _fractalParams.QuatEquation;
        SelectedSceneType = _fractalParams.SceneType;
        AimToOrigin = _fractalParams.AimToOrigin;
        UpdateQuatEquationAndShaderSceneTypeVisibility();
    }

    public RelayCommand IsVisibleChangedCommand { get; }

    public void ExecuteIsVisibleChangedCommand(DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue != null && (bool)e.NewValue)
        {
            ImageWidth = _fractalParams.ImageSize.Width;
            ImageHeight = _fractalParams.ImageSize.Height;
            DisplayWidth = _fractalParams.ImageSize.Width;
            DisplayHeight = _fractalParams.ImageSize.Height;
            FromX = _fractalParams.FromX;
            ToX = _fractalParams.ToX;
            FromY = _fractalParams.FromY;
            ToY = _fractalParams.ToY;
            FromZ = _fractalParams.FromZ;
            ToZ = _fractalParams.ToZ;
            Bailout = _fractalParams.Bailout;
            Iterations = _fractalParams.Iterations;
            MaxRaySteps = _fractalParams.MaxRaySteps;
            MinRayDistance = _fractalParams.MinRayDistance;
            Distance = _fractalParams.Distance;
            MaxDistance = _fractalParams.MaxDistance;
            StepDivisor = _fractalParams.StepDivisor;

            SelectedQuatEquationType = _fractalParams.QuatEquation;
            SelectedSceneType = _fractalParams.SceneType;
        }
        
    }

    public int ImageWidth
    {
        get => _fractalParams.ImageSize.Width;
        set 
        {
            var size = _fractalParams.ImageSize;
            size.Width = value;
            _fractalParams.ImageSize = size;

            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public int ImageHeight
    {
        get => _fractalParams.ImageSize.Height;
        set
        {
            var size = _fractalParams.ImageSize;
            size.Height= value;
            _fractalParams.ImageSize = size;

            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public int DisplayWidth
    {
        get => _fractalParams.DisplaySize.Width;
        set
        {
            var size = _fractalParams.DisplaySize;
            size.Width = value;
            _fractalParams.DisplaySize = size;

            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public int DisplayHeight
    {
        get => _fractalParams.DisplaySize.Height;
        set
        {
            var size = _fractalParams.DisplaySize;
            size.Height = value;
            _fractalParams.DisplaySize = size;

            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float FromX
    {
        get => _fractalParams.FromX;
        set
        {
            if (value > ToX)
            {
                var from = ToX;
                ToX = value;
                _fractalParams.FromX = from;
            }
            else
            {
                _fractalParams.FromX = value;
            }
            
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float ToX
    {
        get => _fractalParams.ToX;
        set
        {
            if (value < FromX)
            {
                var to = FromX;
                FromX = value;
                _fractalParams.ToX = to;
            }
            else
            {
                _fractalParams.ToX = value;
            }
            
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float FromY
    {
        get => _fractalParams.FromY;
        set
        {
            if (value > ToY)
            {
                var from = ToY;
                ToY = value;
                _fractalParams.FromY = from;
            }
            else
            {
                _fractalParams.FromY = value;
            }

            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float ToY
    {
        get => _fractalParams.ToY;
        set
        {
            if (value < FromY)
            {
                var to = FromY;
                FromY = value;
                _fractalParams.ToY = to;
            }
            else
            {
                _fractalParams.ToY = value;
            }

            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float FromZ
    {
        get => _fractalParams.FromZ;
        set
        {
            if (value > ToZ)
            {
                var from = ToZ;
                ToZ = value;
                _fractalParams.FromZ = from;
            }
            else
            {
                _fractalParams.FromZ = value;
            }

            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float ToZ
    {
        get => _fractalParams.ToZ;
        set
        {
            if (value < FromZ)
            {
                var to = FromZ;
                FromZ = value;
                _fractalParams.ToZ = to;
            }
            else
            {
                _fractalParams.ToZ = value;
            }

            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float Bailout
    {
        get => _fractalParams.Bailout;
        set
        {
            _fractalParams.Bailout = value;

            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public int Iterations
    {
        get => _fractalParams.Iterations;
        set
        {
            _fractalParams.Iterations = value;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float Cw
    {
        get => _fractalParams.C4.W;
        set
        {
            if (Math.Abs(value - _fractalParams.C4.W) < ParameterConstants.FloatTolerance)
                return;

            var c4 = _fractalParams.C4;
            c4.W = value;
            _fractalParams.C4 = c4;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float Cx
    {
        get => _fractalParams.C4.X;
        set
        {
            if (Math.Abs(value - _fractalParams.C4.X) < ParameterConstants.FloatTolerance)
                return;

            var c4 = _fractalParams.C4;
            c4.X = value;
            _fractalParams.C4 = c4;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float Cy
    {
        get => _fractalParams.C4.Y;
        set
        {
            if (Math.Abs(value - _fractalParams.C4.Y) < ParameterConstants.FloatTolerance)
                return;

            var c4 = _fractalParams.C4;
            c4.Y = value;
            _fractalParams.C4 = c4;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float Cz
    {
        get => _fractalParams.C4.Z;
        set
        {
            if (Math.Abs(value - _fractalParams.C4.Z) < ParameterConstants.FloatTolerance)
                return;

            var c4 = _fractalParams.C4;
            c4.Z = value;
            _fractalParams.C4 = c4;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public int MaxRaySteps
    {
        get => _fractalParams.MaxRaySteps;
        set
        {
            _fractalParams.MaxRaySteps = value;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float MinRayDistance
    {
        get => _fractalParams.MinRayDistance;
        set
        {
            _fractalParams.MinRayDistance = value;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float Distance
    {
        get => _fractalParams.Distance;
        set
        {
            _fractalParams.Distance = value;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float MaxDistance
    {
        get => _fractalParams.MaxDistance;
        set
        {
            _fractalParams.MaxDistance = value;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float StepDivisor
    {
        get => _fractalParams.StepDivisor;
        set
        {
            _fractalParams.StepDivisor = value;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    private ObservableCollection<QuaternionEquationType> _allowedQuatEquations;
    public ObservableCollection<QuaternionEquationType> AllowedQuatEquations
    {
        get => _allowedQuatEquations;
        set => SetProperty(ref _allowedQuatEquations, value);
    }

    private Visibility _quatEquationVisibility = Visibility.Collapsed;
    public Visibility QuatEquationVisibility
    {
        get => _quatEquationVisibility;
        set
        {
            _quatEquationVisibility = value;
            OnPropertyChanged();
        }
    }

    private Visibility _shaderSceneTypeVisibility = Visibility.Collapsed;
    public Visibility ShaderSceneTypeVisibility
    {
        get => _shaderSceneTypeVisibility;
        set
        {
            _shaderSceneTypeVisibility = value;
            OnPropertyChanged();
        }
    }

    private Visibility _rayTraceFieldVisibility = Visibility.Collapsed;
    public Visibility RayTraceFieldVisibility
    {
        get => _rayTraceFieldVisibility;
        set
        {
            _rayTraceFieldVisibility = value;
            OnPropertyChanged();
        }
    }

    private Visibility _escapeThresholdVisibility = Visibility.Collapsed;
    public Visibility EscapeThresholdVisibility
    {
        get => _escapeThresholdVisibility;
        set
        {
            _escapeThresholdVisibility = value;
            OnPropertyChanged();
        }
    }

    private void UpdateQuatEquationAndShaderSceneTypeVisibility()
    {
        QuatEquationVisibility = SelectedShaderType == ShaderType.FractalShader || SelectedShaderType == ShaderType.CraneShader || SelectedShaderType == ShaderType.CranePixel || SelectedShaderType == ShaderType.CraneRaymarch ? 
            Visibility.Visible : Visibility.Collapsed;

        ShaderSceneTypeVisibility = SelectedShaderType == ShaderType.ShapeShader ? Visibility.Visible : Visibility.Collapsed;

        RayTraceFieldVisibility = SelectedShaderType == ShaderType.CraneRaymarch || SelectedShaderType == ShaderType.FractalShader || SelectedShaderType == ShaderType.ShapeShader ? 
            Visibility.Visible : Visibility.Collapsed;   

        EscapeThresholdVisibility = SelectedShaderType == ShaderType.CraneShader || SelectedShaderType == ShaderType.CranePixel || SelectedShaderType == ShaderType.CraneRaymarch ?
            Visibility.Visible : Visibility.Collapsed;

        UpdateAllowedQuatEquations();
    }

    private void UpdateAllowedQuatEquations()
    {
        if(SelectedShaderType == ShaderType.FractalShader)
        { 
            AllowedQuatEquations = new ObservableCollection<QuaternionEquationType>
            {
                QuaternionEquationType.Q_Squared, QuaternionEquationType.Q_Cubed, QuaternionEquationType.Q_InglesCubed
            };

            if(SelectedQuatEquationType == QuaternionEquationType.Q_CubedZZ2 || SelectedQuatEquationType == QuaternionEquationType.Q_CubedZ2Z)
                SelectedQuatEquationType = QuaternionEquationType.Q_Squared;
        }
        else
        {
            AllowedQuatEquations = new ObservableCollection<QuaternionEquationType>
            {
                QuaternionEquationType.Q_Squared, QuaternionEquationType.Q_Cubed, QuaternionEquationType.Q_CubedZZ2, QuaternionEquationType.Q_CubedZ2Z
            };

            if(SelectedQuatEquationType == QuaternionEquationType.Q_InglesCubed)
                SelectedQuatEquationType = QuaternionEquationType.Q_Squared;
        }
    }

    public QuaternionEquationType SelectedQuatEquationType
    {
        get => _fractalParams.QuatEquation;
        set 
        {
            if (value == _fractalParams.QuatEquation)
                return;
            _fractalParams.QuatEquation = value;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public bool AimToOrigin
    {
        get => _fractalParams.AimToOrigin;
        set
        {
            if (value == _fractalParams.AimToOrigin)
                return;
            _fractalParams.AimToOrigin = value;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    private ObservableCollection<ShaderType> _allowedShaderTypes;
    public ObservableCollection<ShaderType> AllowedShaderTypes  
    {
        get => _allowedShaderTypes;
        set => SetProperty(ref _allowedShaderTypes, value);
    }

    public ShaderType SelectedShaderType
    {
        get => _fractalParams.ShaderType;
        set
        {             
            if (value == _fractalParams.ShaderType)
                return;
            _fractalParams.ShaderType = value;
            OnPropertyChanged();
            UpdateQuatEquationAndShaderSceneTypeVisibility();
            _onParamsChanged(_fractalParams);
        }
    }

    private List<ShaderSceneType> _allowedSceneType;
    public List<ShaderSceneType> AllowedSceneTypes
    {
        get => _allowedSceneType;
        set => SetProperty(ref _allowedSceneType, value);
    }

    public ShaderSceneType SelectedSceneType
    {
        get => _fractalParams.SceneType;
        set
        {
            if (value == _fractalParams.SceneType)
                return;
            _fractalParams.SceneType = value;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float DistanceScale
    {
        get => _fractalParams.DistanceScale;
        set
        {
            _fractalParams.DistanceScale = value;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float ColorBase
    {
        get => _fractalParams.ColorBase;
        set
        {
            _fractalParams.ColorBase = value;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public bool RenderShadows
    {
        get => _fractalParams.RenderShadows;
        set
        {
            if(value == _fractalParams.RenderShadows)
                return;
            _fractalParams.RenderShadows = value;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float EscapeThreshold
    {
        get => _fractalParams.EscapeThreshold;
        set
        {
            _fractalParams.EscapeThreshold = value;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

}
