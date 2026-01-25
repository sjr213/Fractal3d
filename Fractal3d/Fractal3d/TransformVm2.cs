namespace Fractal3d;

using BasicWpfLibrary;
using ImageCalculator;
using System;

public class TransformVm2 : ViewModelBase
{
    private readonly FractalParams _fractalParams;
    private readonly TransformationParams _transParams;
    private readonly Action<FractalParams> _onParamsChanged;
    
    public TransformVm2(FractalParams fractalParams, TransformationParams transParams, Action<FractalParams> onParamsChanged)
    {
        _fractalParams = fractalParams;
        _transParams = transParams;
        _onParamsChanged = onParamsChanged;
    }

    public float TranslateX
    {
        get => _transParams.TranslateX;
        set
        {
            _transParams.TranslateX = value;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float TranslateY
    {
        get => _transParams.TranslateY;
        set
        {
            _transParams.TranslateY = value;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float TranslateZ
    {
        get => _transParams.TranslateZ;
        set
        {
            _transParams.TranslateZ = value;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float RotateX
    {
        get => _transParams.RotateX;
        set
        {
            _transParams.RotateX = value;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float RotateY
    {
        get => _transParams.RotateY;
        set
        {
            _transParams.RotateY = value;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float RotateZ
    {
        get => _transParams.RotateZ;
        set
        {
            _transParams.RotateZ = value;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float ScaleX
    {
        get => _transParams.ScaleX;
        set
        {
            _transParams.ScaleX = value;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float ScaleY
    {
        get => _transParams.ScaleY;
        set
        {
            _transParams.ScaleY = value;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }

    public float ScaleZ
    {
        get => _transParams.ScaleZ;
        set
        {
            _transParams.ScaleZ = value;
            OnPropertyChanged();
            _onParamsChanged(_fractalParams);
        }
    }
}