namespace Fractal3d;

using BasicWpfLibrary;
using ImageCalculator;
using System;
using System.Collections.ObjectModel;

public class ParameterVm : ViewModelBase
{
    private const float FloatTolerance = 0.00001f;
    private FractalParams _fractalParams;
    private Action<FractalParams> _onParamsChanged;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public ParameterVm(FractalParams fractalParams, Action<FractalParams> paramsChanged)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        _fractalParams = fractalParams;
        _onParamsChanged = paramsChanged;

        AllowedQuatEquations = new ObservableCollection<QuaternionEquationType>
        {
            QuaternionEquationType.Q_Squared, QuaternionEquationType.Q_Cubed, QuaternionEquationType.Q_InglesCubed
        };
        SelectedQuatEquationType = _fractalParams.QuatEquation;
        AimToOrigin = _fractalParams.AimToOrigin;
    }

    public int ImageWidth
    {
        get { return _fractalParams.ImageSize.Width; }
        set 
        {
            if (_fractalParams.ImageSize.Width == value)
                return;

            var size = _fractalParams.ImageSize;
            size.Width = value;
            _fractalParams.ImageSize = size;

            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public int ImageHeight
    {
        get { return _fractalParams.ImageSize.Height; }
        set
        {
            if (_fractalParams.ImageSize.Height == value)
                return;
            var size = _fractalParams.ImageSize;
            size.Height= value;
            _fractalParams.ImageSize = size;

            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public int DisplayWidth
    {
        get { return _fractalParams.DisplaySize.Width; }
        set
        {
            if (_fractalParams.DisplaySize.Width == value)
                return;

            var size = _fractalParams.DisplaySize;
            size.Width = value;
            _fractalParams.DisplaySize = size;

            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public int DisplayHeight
    {
        get { return _fractalParams.DisplaySize.Height; }
        set
        {
            if (_fractalParams.DisplaySize.Height == value)
                return;
            var size = _fractalParams.DisplaySize;
            size.Height = value;
            _fractalParams.DisplaySize = size;

            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float FromX
    {
        get { return _fractalParams.FromX; }
        set
        {
            if (Math.Abs(_fractalParams.FromX - value) < FloatTolerance)
                return;
            _fractalParams.FromX = value;

            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float ToX
    {
        get { return _fractalParams.ToX; }
        set
        {
            if (Math.Abs(_fractalParams.ToX - value) < FloatTolerance)
                return;
            _fractalParams.ToX = value;

            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float FromY
    {
        get { return _fractalParams.FromY; }
        set
        {
            if (Math.Abs(_fractalParams.FromY - value) < FloatTolerance)
                return;
            _fractalParams.FromY = value;

            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float ToY
    {
        get { return _fractalParams.ToY; }
        set
        {
            if (Math.Abs(_fractalParams.ToY - value) < FloatTolerance)
                return;
            _fractalParams.ToY = value;

            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float Bailout
    {
        get { return _fractalParams.Bailout; }
        set
        {
            if (Math.Abs(_fractalParams.Bailout - value) < FloatTolerance)
                return;
            _fractalParams.Bailout = value;

            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public int Iterations
    {
        get { return _fractalParams.Iterations; }
        set
        {
            if (value == _fractalParams.Iterations)
                return;
            _fractalParams.Iterations = value;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float Cw
    {
        get { return _fractalParams.C4.W; }
        set
        {
            if (Math.Abs(value - _fractalParams.C4.W) < FloatTolerance)
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
        get { return _fractalParams.C4.X; }
        set
        {
            if (Math.Abs(value - _fractalParams.C4.X) < FloatTolerance)
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
        get { return _fractalParams.C4.Y; }
        set
        {
            if (Math.Abs(value - _fractalParams.C4.Y) < FloatTolerance)
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
        get { return _fractalParams.C4.Z; }
        set
        {
            if (Math.Abs(value - _fractalParams.C4.Z) < FloatTolerance)
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
        get { return _fractalParams.MaxRaySteps; }
        set
        {
            if (value == _fractalParams.MaxRaySteps)
                return;
            _fractalParams.MaxRaySteps = value;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float MinRayDistance
    {
        get { return _fractalParams.MinRayDistance; }
        set
        {
            if (Math.Abs(value - _fractalParams.MinRayDistance) < FloatTolerance)
                return;
            _fractalParams.MinRayDistance = value;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float Distance
    {
        get { return _fractalParams.Distance; }
        set
        {
            if (Math.Abs(value - _fractalParams.Distance) < FloatTolerance)
                return;
            _fractalParams.Distance = value;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float MaxDistance
    {
        get { return _fractalParams.MaxDistance; }
        set
        {
            if (Math.Abs(value - _fractalParams.MaxDistance) < FloatTolerance)
                return;
            _fractalParams.MaxDistance = value;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float StepDivisor
    {
        get { return _fractalParams.StepDivisor; }
        set
        {
            if (Math.Abs(value - _fractalParams.StepDivisor) < FloatTolerance)
                return;
            _fractalParams.StepDivisor = value;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    private ObservableCollection<QuaternionEquationType> _allowedQuatEquations;
    public ObservableCollection<QuaternionEquationType> AllowedQuatEquations
    {
        get => _allowedQuatEquations;
        set { SetProperty(ref _allowedQuatEquations, value); }
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

}
