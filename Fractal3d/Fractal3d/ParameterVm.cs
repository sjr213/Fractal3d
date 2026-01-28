using System.Collections.Generic;

namespace Fractal3d;

using BasicWpfLibrary;
using ImageCalculator;
using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows;

public class ParameterVm : ViewModelBase
{

 #region fields
    private readonly FractalParams _fractalParams;
    private readonly Action<FractalParams> _onParamsChanged;
    private ObservableCollection<QuaternionEquationType> _allowedQuatEquations;
    private Visibility _quatEquationVisibility = Visibility.Collapsed;
    private Visibility _constC_Visibility = Visibility.Collapsed;
    private Visibility _shaderSceneTypeVisibility = Visibility.Collapsed;
    private Visibility _rayTraceFieldVisibility = Visibility.Collapsed;
    private Visibility _escapeThresholdVisibility = Visibility.Collapsed;
    private Visibility _lightingOnZeroIndexVisibility = Visibility.Collapsed;
    private Visibility _ifsVisibility = Visibility.Collapsed;
    private ObservableCollection<ShaderType> _allowedShaderTypes;
    private List<ShaderSceneType> _allowedSceneType;
    private ObservableCollection<IfsEquationType> _allowedIfsEquationTypes;
    private bool _isCraneShader = false;
    #endregion

    #region construction

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public ParameterVm(FractalParams fractalParams, Action<FractalParams> paramsChanged)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        _fractalParams = fractalParams;
        _onParamsChanged = paramsChanged;
        IsVisibleChangedCommand = new RelayCommand(param => ExecuteIsVisibleChangedCommand(param is DependencyPropertyChangedEventArgs args ? args : default));

        AllowedShaderTypes = new ObservableCollection<ShaderType>
        {
            ShaderType.FractalShader, ShaderType.CraneShader, ShaderType.CranePixel, ShaderType.CraneRaymarch, ShaderType.ShapeShader, 
            ShaderType.ShadertoyShader, ShaderType.IFSShader
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
        Transform1 = new TransformVm2(_fractalParams, _fractalParams.IfsTransform1, _onParamsChanged);
        Transform2 = new TransformVm2(_fractalParams, _fractalParams.IfsTransform2, _onParamsChanged);
        SelectedIfsEquationType = _fractalParams.IfsEquation;
        AllowedIfsEquationTypes = new ObservableCollection<IfsEquationType>
        {
            IfsEquationType.Standard, IfsEquationType.CenterStretch
        };
    }

#endregion

#region properties

    public RelayCommand IsVisibleChangedCommand { get; }

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
        get => (float)Math.Round(_fractalParams.FromX,5,MidpointRounding.AwayFromZero);
        set
        {
            if(value > ToX)
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
        get => (float)Math.Round(_fractalParams.ToX, 5, MidpointRounding.AwayFromZero);
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
        get => (float)Math.Round(_fractalParams.FromY, 5, MidpointRounding.AwayFromZero);
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
        get => (float)Math.Round(_fractalParams.ToY, 5, MidpointRounding.AwayFromZero);
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

    public ObservableCollection<QuaternionEquationType> AllowedQuatEquations
    {
        get => _allowedQuatEquations;
        set => SetProperty(ref _allowedQuatEquations, value);
    }

    public Visibility QuatEquationVisibility
    {
        get => _quatEquationVisibility;
        set
        {
            _quatEquationVisibility = value;
            OnPropertyChanged();
        }
    }

    public Visibility ConstantC_Visibility
    {
        get => _constC_Visibility;
        set
        {
            _constC_Visibility = value;
            OnPropertyChanged();
        }
    }

    public Visibility ShaderSceneTypeVisibility
    {
        get => _shaderSceneTypeVisibility;
        set
        {
            _shaderSceneTypeVisibility = value;
            OnPropertyChanged();
        }
    }

    public Visibility RayTraceFieldVisibility
    {
        get => _rayTraceFieldVisibility;
        set
        {
            _rayTraceFieldVisibility = value;
            OnPropertyChanged();
        }
    }

    public Visibility EscapeThresholdVisibility
    {
        get => _escapeThresholdVisibility;
        set
        {
            _escapeThresholdVisibility = value;
            OnPropertyChanged();
        }
    }

    public Visibility NonCraneShaderVisibility
    {
        get => _lightingOnZeroIndexVisibility;
        set
        {
            _lightingOnZeroIndexVisibility = value;
            OnPropertyChanged();
        }
    }

    public Visibility IfsVisibility
    {
        get => _ifsVisibility;
        set
        {
            _ifsVisibility = value;
            OnPropertyChanged();
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

    public int BackgroundColorA
    {
        get => _fractalParams.BackgroundColor.A;
        set
        {
            if(value < 0 || value > 255)
                return;

            var bValue = (byte)value;
            if (bValue == _fractalParams.BackgroundColor.A)
                return;
            _fractalParams.BackgroundColor = Color.FromArgb(bValue, _fractalParams.BackgroundColor.R, _fractalParams.BackgroundColor.G, _fractalParams.BackgroundColor.B);
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public int BackgroundColorR
    {
        get => _fractalParams.BackgroundColor.R;
        set
        {
            if(value < 0 || value > 255)
                return;
            var bValue = (byte)value;
            if(bValue == _fractalParams.BackgroundColor.R)
                return;
            _fractalParams.BackgroundColor = Color.FromArgb(_fractalParams.BackgroundColor.A, bValue, _fractalParams.BackgroundColor.G, _fractalParams.BackgroundColor.B);
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public int BackgroundColorG
    {
        get => _fractalParams.BackgroundColor.G;
        set
        {
            if(value < 0 || value > 255)
                return;
            var bValue = (byte)value;
            if(bValue == _fractalParams.BackgroundColor.G)
                return;
            _fractalParams.BackgroundColor = Color.FromArgb(_fractalParams.BackgroundColor.A, _fractalParams.BackgroundColor.R, bValue, _fractalParams.BackgroundColor.B);
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public int BackgroundColorB
    {
        get => _fractalParams.BackgroundColor.B;
        set
        {
            if(value < 0 || value > 255)
                return;
            var bValue = (byte)value;
            if(bValue == _fractalParams.BackgroundColor.B)
                return;
            _fractalParams.BackgroundColor = Color.FromArgb(_fractalParams.BackgroundColor.A, _fractalParams.BackgroundColor.R, _fractalParams.BackgroundColor.G, bValue);
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public bool IsCraneShader
    {
        get => _isCraneShader;
        set
        {
            _isCraneShader = value;
            OnPropertyChanged();
        }

    }

    public bool LightingOnZeroIndex
    {
        get => _fractalParams.LightingOnZeroIndex;
        set
        {
            if (value == _fractalParams.LightingOnZeroIndex)
                return;
            _fractalParams.LightingOnZeroIndex = value;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float MinStretchDistance
    {
        get => _fractalParams.MinStretchDistance;
        set
        {
            if(value > MaxStretchDistance)
            {
                var max = MaxStretchDistance;
                MaxStretchDistance = value;
                _fractalParams.MinStretchDistance = max;
            }
            else
                _fractalParams.MinStretchDistance = value;

            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float MaxStretchDistance
    {
        get => _fractalParams.MaxStretchDistance;
        set
        {
            if (value < MinStretchDistance)
            {
                var min = MinStretchDistance;
                MinStretchDistance = value;
                _fractalParams.MaxStretchDistance = min;
            }
            else
                _fractalParams.MaxStretchDistance = value;

            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    #endregion

    #region handlers

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
            Transform1 = new TransformVm2(_fractalParams, _fractalParams.IfsTransform1, _onParamsChanged);
            Transform2 = new TransformVm2(_fractalParams, _fractalParams.IfsTransform2, _onParamsChanged);
            SelectedIfsEquationType = _fractalParams.IfsEquation;
        }
    }

    #endregion

    #region methods

    private void UpdateQuatEquationAndShaderSceneTypeVisibility()
    {
        QuatEquationVisibility = SelectedShaderType == ShaderType.FractalShader || SelectedShaderType == ShaderType.CraneShader || SelectedShaderType == ShaderType.CranePixel || 
            SelectedShaderType == ShaderType.CraneRaymarch || SelectedShaderType == ShaderType.ShadertoyShader ?
            Visibility.Visible : Visibility.Collapsed;

        ShaderSceneTypeVisibility = SelectedShaderType == ShaderType.ShapeShader ? Visibility.Visible : Visibility.Collapsed;

        RayTraceFieldVisibility = SelectedShaderType == ShaderType.CraneRaymarch || SelectedShaderType == ShaderType.FractalShader || 
            SelectedShaderType == ShaderType.ShapeShader || _fractalParams.ShaderType == ShaderType.IFSShader ?
            Visibility.Visible : Visibility.Collapsed;

        EscapeThresholdVisibility = SelectedShaderType == ShaderType.CraneShader || SelectedShaderType == ShaderType.CranePixel ||
            SelectedShaderType == ShaderType.CraneRaymarch || SelectedShaderType == ShaderType.ShadertoyShader ?
            Visibility.Visible : Visibility.Collapsed;

        NonCraneShaderVisibility = SelectedShaderType == ShaderType.CraneShader || SelectedShaderType == ShaderType.ShadertoyShader 
            || SelectedShaderType == ShaderType.IFSShader ? Visibility.Collapsed : Visibility.Visible;

        IsCraneShader = _fractalParams.ShaderType == ShaderType.CraneShader || _fractalParams.ShaderType == ShaderType.ShadertoyShader;

        ConstantC_Visibility = SelectedShaderType == ShaderType.ShapeShader || SelectedShaderType == ShaderType.IFSShader ?
            Visibility.Collapsed : Visibility.Visible;

        IfsVisibility = SelectedShaderType == ShaderType.IFSShader ? Visibility.Visible : Visibility.Collapsed;

        UpdateAllowedQuatEquations();
    }

    private void UpdateAllowedQuatEquations()
    {
        if (SelectedShaderType == ShaderType.FractalShader)
        {
            AllowedQuatEquations = new ObservableCollection<QuaternionEquationType>
            {
                QuaternionEquationType.Q_Squared, QuaternionEquationType.Q_Cubed, QuaternionEquationType.Q_InglesCubed
            };

            if (SelectedQuatEquationType == QuaternionEquationType.Q_CubedZZ2 || SelectedQuatEquationType == QuaternionEquationType.Q_CubedZ2Z)
                SelectedQuatEquationType = QuaternionEquationType.Q_Squared;
        }
        else
        {
            AllowedQuatEquations = new ObservableCollection<QuaternionEquationType>
            {
                QuaternionEquationType.Q_Squared, QuaternionEquationType.Q_Cubed, QuaternionEquationType.Q_CubedZZ2, QuaternionEquationType.Q_CubedZ2Z
            };

            if (SelectedQuatEquationType == QuaternionEquationType.Q_InglesCubed)
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

    public ObservableCollection<IfsEquationType> AllowedIfsEquationTypes
    {
        get => _allowedIfsEquationTypes;
        set => SetProperty(ref _allowedIfsEquationTypes, value);
    }

    public IfsEquationType SelectedIfsEquationType
    {
        get => _fractalParams.IfsEquation;
        set
        {
            if (value == _fractalParams.IfsEquation)
                return;
            _fractalParams.IfsEquation = value;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float IfsScale
    {
        get => _fractalParams.IfsScale;
        set
        {
            _fractalParams.IfsScale = value;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    private TransformVm2 _transformVm1;
    public TransformVm2 Transform1
    {
        get => _transformVm1;
        set => SetProperty(ref _transformVm1, value);
    }

    private TransformVm2 _transformVm2;
    public TransformVm2 Transform2
    {
        get => _transformVm2;
        set => SetProperty(ref _transformVm2, value);
    }

    public float IfsCx
    {
        get => _fractalParams.IfsC.X;
        set
        {
            if (Math.Abs(value - _fractalParams.IfsC.X) < ParameterConstants.FloatTolerance)
                return;
            var ifsC = _fractalParams.IfsC;
            ifsC.X = value;
            _fractalParams.IfsC = ifsC;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float IfsCy
    {
        get => _fractalParams.IfsC.Y;
        set
        {
            if (Math.Abs(value - _fractalParams.IfsC.Y) < ParameterConstants.FloatTolerance)
                return;
            var ifsC = _fractalParams.IfsC;
            ifsC.Y = value;
            _fractalParams.IfsC = ifsC;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float IfsCz
    {
        get => _fractalParams.IfsC.Z;
        set
        {
            if (Math.Abs(value - _fractalParams.IfsC.Z) < ParameterConstants.FloatTolerance)
                return;
            var ifsC = _fractalParams.IfsC;
            ifsC.Z = value;
            _fractalParams.IfsC = ifsC;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    #endregion
}
