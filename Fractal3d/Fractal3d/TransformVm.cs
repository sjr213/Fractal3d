namespace Fractal3d;

using BasicWpfLibrary;
using ImageCalculator;
using System;

public class TransformVm : ViewModelBase
{
    private readonly FractalParams _fractalParams;
    private readonly Action<FractalParams> _onParamsChanged;

    public TransformVm(FractalParams fractalParams, Action<FractalParams> onParamsChanged)
    {
        _fractalParams = fractalParams;
        _onParamsChanged = onParamsChanged;
    }

    public float TranslateX
    {
        get => _fractalParams.TransformParams.TranslateX;
        set
        {
            _fractalParams.TransformParams.TranslateX = value;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float TranslateY
    {
        get => _fractalParams.TransformParams.TranslateY;
        set
        {
            _fractalParams.TransformParams.TranslateY = value;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float TranslateZ
    {
        get => _fractalParams.TransformParams.TranslateZ;
        set
        {
            _fractalParams.TransformParams.TranslateZ = value;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float RotateX
    {
        get => _fractalParams.TransformParams.RotateX;
        set
        {
            _fractalParams.TransformParams.RotateX = value;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float RotateY
    {
        get => _fractalParams.TransformParams.RotateY;
        set
        {
            _fractalParams.TransformParams.RotateY = value;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float RotateZ
    {
        get => _fractalParams.TransformParams.RotateZ;
        set
        {
            _fractalParams.TransformParams.RotateX = value;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float ScaleX
    {
        get => _fractalParams.TransformParams.ScaleX;
        set
        {
            _fractalParams.TransformParams.ScaleX = value;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float ScaleY
    {
        get => _fractalParams.TransformParams.ScaleY;
        set
        {
            _fractalParams.TransformParams.ScaleY = value;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float ScaleZ
    {
        get => _fractalParams.TransformParams.ScaleZ;
        set
        {
            _fractalParams.TransformParams.ScaleX = value;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }
}

