using System.Numerics;
namespace Fractal3d;

using BasicWpfLibrary;
using ImageCalculator;
using System.Text;
using System.Windows;

public class FractalResultVm : ViewModelBase
{
    private readonly FractalResult _fractalResult;
    private Visibility _nonCraneShaderVisibility = Visibility.Collapsed;
    private Visibility _equationVisibility = Visibility.Collapsed;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public FractalResultVm(FractalResult fractalResult, int number)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        _fractalResult = fractalResult;
        _number = number;
        if (_fractalResult != null && _fractalResult.Params != null)
        {
            NonCraneShaderVisibility = _fractalResult.Params.ShaderType == ShaderType.CraneShader || _fractalResult.Params.ShaderType == ShaderType.ShadertoyShader ||
                _fractalResult.Params.ShaderType == ShaderType.IFSShader ?
                Visibility.Collapsed : Visibility.Visible;
            EquationVisibility = _fractalResult.Params.ShaderType == ShaderType.ShapeShader ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    private int _number;

    public int Number
    {
        get => _number;
        set => SetProperty(ref _number, value);
    }

    public string Title => "Image " + Number;

    public string ShaderName
    {
        get
        {
            if (_fractalResult.Params == null)
                return string.Empty;
            return _fractalResult.Params.ShaderType.GetDescription();
        }
    }

    public string EquationType
    {
        get
        {
            if (_fractalResult.Params == null)
                return string.Empty;

            if (_fractalResult.Params.ShaderType == ShaderType.ShapeShader)
                return string.Empty;

            var val = _fractalResult.Params.QuatEquation;
            return ((QuaternionEquationType)val).GetDescription();
        }
    }

    public string PaletteName
    {
        get
        {
            if (_fractalResult.Params == null)
                return string.Empty;

            return _fractalResult.Params.Palette.PaletteName;
        }
    }

    public string TruncateFloatStringOnRight(float val)
    {
        var str = val.ToString("F4");
        if (str.IndexOf('.', 0) > -1)
        {
            char[] charToTrim = { '0' };
            str = str.TrimEnd(charToTrim);
        }
        char[] charsToTrim = { ' ', '.' };
        return str.TrimEnd(charsToTrim);
    }

    public string GetConstantC_String(Vector4 c)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(TruncateFloatStringOnRight(c.W));
        sb.Append(", ");
        sb.Append(TruncateFloatStringOnRight(c.X));
        sb.Append(", ");
        sb.Append(TruncateFloatStringOnRight(c.Y));
        sb.Append(", ");
        sb.Append(TruncateFloatStringOnRight(c.Z));

        return sb.ToString();
    }

    public string GetRangeString(float left, string meat, float right)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(TruncateFloatStringOnRight(left));
        sb.Append(" < ");
        sb.Append(meat);
        sb.Append(" < ");
        sb.Append(TruncateFloatStringOnRight(right));

        return sb.ToString();
    }

    public string ConstantC
    {
        get
        {
            if (_fractalResult.Params == null)
                return string.Empty;

            var c = _fractalResult.Params.C4;

            //return $"{c.W,6:N3}, {c.X,6:N3}, {c.Y,6:N3}, {c.Z,6:N3}";
            return GetConstantC_String(c);
        }
    }

    public string Bailout
    {
        get
        {
            if (_fractalResult.Params == null)
                return string.Empty;

            return TruncateFloatStringOnRight(_fractalResult.Params.Bailout);
        }
    }

    public string RangeX
    {
        get
        {
            if (_fractalResult.Params == null)
                return string.Empty;

            return GetRangeString(_fractalResult.Params.FromX, "X", _fractalResult.Params.ToX);
        }
    }

    public string RangeY
    {
        get
        {
            if (_fractalResult.Params == null)
                return string.Empty;

            return GetRangeString(_fractalResult.Params.FromY, "Y", _fractalResult.Params.ToY);
        }
    }

    public string RangeZ
    {
        get
        {
            if (_fractalResult.Params == null)
                return string.Empty;

            return GetRangeString(_fractalResult.Params.FromZ, "Z", _fractalResult.Params.ToZ);
        }
    }

    public int Iterations
    {
        get
        {
            if (_fractalResult.Params == null)
                return 0;

            return _fractalResult.Params.Iterations;
        }
    }

    public string MinRayDistance
    {
        get
        {
            if (_fractalResult.Params == null)
                return string.Empty;

            return TruncateFloatStringOnRight(_fractalResult.Params.MinRayDistance);
        }
    }

    public int MaxRaySteps
    {
        get
        {
            if (_fractalResult.Params == null)
                return 0;

            return _fractalResult.Params.MaxRaySteps;
        }
    }

    public string Distance
    {
        get
        {
            if (_fractalResult.Params == null)
                return string.Empty;

            return TruncateFloatStringOnRight(_fractalResult.Params.Distance);
        }
    }

    public string MaxDistance
    {
        get
        {
            if (_fractalResult.Params == null)
                return string.Empty;

            return TruncateFloatStringOnRight(_fractalResult.Params.MaxDistance);
        }
    }

    public string StepDivisor
    {
        get
        {
            if (_fractalResult.Params == null)
                return string.Empty;

            return TruncateFloatStringOnRight(_fractalResult.Params.StepDivisor);
        }
    }

    public string AimToOrigin
    {
        get
        {
            if(_fractalResult.Params == null)
                return string.Empty;

            return _fractalResult.Params.AimToOrigin ? "Yes" : "No";
        }
    }

    public string LightingOnZeroIndex
    {
        get
        {
            if (_fractalResult.Params == null)
                return string.Empty;
            return _fractalResult.Params.LightingOnZeroIndex ? "Yes" : "No";
        }
    }

    public string StretchDistanceRangeText
    {
        get
        {
            if (_fractalResult.Params == null)
                return string.Empty;
            return string.Format("{0:F2} < Stretch Distance < {1:F2}", _fractalResult.Params.MinStretchDistance, _fractalResult.Params.MaxStretchDistance);
        }
    }

    public Visibility NonCraneShaderVisibility
    {
        get => _nonCraneShaderVisibility;
        set => SetProperty(ref _nonCraneShaderVisibility, value);
    }

    public Visibility EquationVisibility
    {
        get => _equationVisibility;
        set => SetProperty(ref _equationVisibility, value);
    }

    public FractalResult Result => _fractalResult;

};


