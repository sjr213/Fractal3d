using System.Numerics;
namespace Fractal3d;

using BasicWpfLibrary;
using ImageCalculator;
using System.Text;

public class FractalResultVm : ViewModelBase
{
    private readonly FractalResult _fractalResult;

    public FractalResultVm(FractalResult fractalResult, int number)
    {
        _fractalResult = fractalResult;
        _number = number;
    }

    private int _number;

    public int Number
    {
        get => _number;
        set => SetProperty(ref _number, value);
    }

    public string Title => "Image " + Number;

    public string EquationType
    {
        get
        {
            if (_fractalResult.Params == null)
                return string.Empty;

            if (_fractalResult.Params.ShaderType == ShaderType.ShapeShader)
                return "Shape Shader";

            if(_fractalResult.Params.ShaderType == ShaderType.CraneShader)
                return "Crane Shader";

            if(_fractalResult.Params.ShaderType == ShaderType.CranePixel)
                return "Crane Pixel";

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

public FractalResult Result => _fractalResult;

};


