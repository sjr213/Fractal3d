using System.Numerics;
namespace Fractal3d;

using BasicWpfLibrary;
using ImageCalculator;
using System.Text;
using System.Windows;

public class FractalResultVm : ViewModelBase
{
    private readonly FractalResult _fractalResult;
    private Visibility _paletteVisibility = Visibility.Collapsed;
    private Visibility _equationVisibility = Visibility.Collapsed;
    private Visibility _constantC_Visibility = Visibility.Collapsed;
    private Visibility _ifsC_Visibility = Visibility.Collapsed;
    private Visibility _ifsGeneral_Visibility = Visibility.Collapsed;
    private Visibility _StepDivisorVisibility = Visibility.Collapsed;
    private Visibility _bailoutVisibility = Visibility.Collapsed;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public FractalResultVm(FractalResult fractalResult, int number)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        _fractalResult = fractalResult;
        _number = number;
        if (_fractalResult != null && _fractalResult.Params != null)
        {
            PaletteVisibility = ShaderTypeUtils.UsesPalette(_fractalResult.Params.ShaderType) ?
                Visibility.Visible : Visibility.Collapsed;
            EquationVisibility = _fractalResult.Params.ShaderType == ShaderType.ShapeShader ? Visibility.Collapsed : Visibility.Visible;
            ConstantC_Visibility = _fractalResult.Params.ShaderType == ShaderType.ShapeShader || _fractalResult.Params.ShaderType == ShaderType.IFSShader ? 
                Visibility.Collapsed : Visibility.Visible;
            IfsC_Visibility = _fractalResult.Params.ShaderType == ShaderType.IFSShader && _fractalResult.Params.IfsEquation != IfsEquationType.Test ? Visibility.Visible : Visibility.Collapsed;
            IfsGeneral_Visibility = _fractalResult.Params.ShaderType == ShaderType.IFSShader ? Visibility.Visible : Visibility.Collapsed;
            StepDivisorVisibility = _fractalResult.Params.ShaderType == ShaderType.CraneRaymarch || _fractalResult.Params.ShaderType == ShaderType.FractalShader ||
                _fractalResult.Params.ShaderType == ShaderType.IFSShader ? Visibility.Visible : Visibility.Collapsed;

            BailoutVisibility = _fractalResult.Params.ShaderType == ShaderType.ShapeShader ||
                (_fractalResult.Params.ShaderType == ShaderType.IFSShader && (_fractalResult.Params.IfsEquation == IfsEquationType.StandardNoBailout || _fractalResult.Params.IfsEquation == IfsEquationType.KnightyNoBailout)) ?
                Visibility.Collapsed : Visibility.Visible;
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

            if (_fractalResult.Params.ShaderType == ShaderType.IFSShader)
            {
                var val = _fractalResult.Params.IfsEquation;
                return ((IfsEquationType)val).GetDescription();
            }
            else
            {
                var val = _fractalResult.Params.QuatEquation;
                return ((QuaternionEquationType)val).GetDescription();
            }
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

    public static string TruncateFloatStringOnRight(float val)
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

    public static string GetConstantC_String(Vector4 c)
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

    private string GetIfsC_String(Vector3 c)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(TruncateFloatStringOnRight(c.X));
        sb.Append(", ");
        sb.Append(TruncateFloatStringOnRight(c.Y));
        sb.Append(", ");
        sb.Append(TruncateFloatStringOnRight(c.Z));

        return sb.ToString();
    }

    public string IfsC
    {
        get
        {
            if (_fractalResult.Params == null)
                return string.Empty;

            var c = _fractalResult.Params.IfsC;

            //return $"{c.W,6:N3}, {c.X,6:N3}, {c.Y,6:N3}, {c.Z,6:N3}";
            return GetIfsC_String(c);
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

            return "Min Ray: " + TruncateFloatStringOnRight(_fractalResult.Params.MinRayDistance);
        }
    }

    public string MaxRaySteps
    {
        get
        {
            if (_fractalResult.Params == null)
                return string.Empty;


            return $"Max Steps: {_fractalResult.Params.MaxRaySteps}";
        }
    }

    public string Distance
    {
        get
        {
            if (_fractalResult.Params == null)
                return string.Empty;

            return "Dist: " + TruncateFloatStringOnRight(_fractalResult.Params.Distance);
        }
    }

    public string MaxDistance
    {
        get
        {
            if (_fractalResult.Params == null)
                return string.Empty;

            return "Max Dist: " + TruncateFloatStringOnRight(_fractalResult.Params.MaxDistance);
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

    public string IfsScale
    {
        get
        {
            if (_fractalResult.Params == null)
                return string.Empty;

            return "Ifs Scale: " + TruncateFloatStringOnRight(_fractalResult.Params.IfsScale);
        }
    }

    public string IfsAbs
    {
        get
        {
            if (_fractalResult.Params == null)
                return string.Empty;
            return "Ifs Abs: " + (_fractalResult.Params.IfsAbs ? "Yes" : "No");
        }
    }

    private static string GetRot_String(TransformationParams tp)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(TruncateFloatStringOnRight(tp.RotateX));
        sb.Append(", ");
        sb.Append(TruncateFloatStringOnRight(tp.RotateY));
        sb.Append(", ");
        sb.Append(TruncateFloatStringOnRight(tp.RotateZ));

        return sb.ToString();
    }

    public string IfsRot1
    {
        get
        {
            if (_fractalResult.Params == null)
                return string.Empty;
            return "Ifs Rot1: " + GetRot_String(_fractalResult.Params.IfsTransform1);
        }
    }

    public string IfsRot2
    {
        get
        {
            if (_fractalResult.Params == null)
                return string.Empty;
            return "Ifs Rot2: " + GetRot_String(_fractalResult.Params.IfsTransform2);
        }
    }

    public Visibility PaletteVisibility
    {
        get => _paletteVisibility;
        set => SetProperty(ref _paletteVisibility, value);
    }

    public Visibility EquationVisibility
    {
        get => _equationVisibility;
        set => SetProperty(ref _equationVisibility, value);
    }

    public Visibility ConstantC_Visibility
    {
        get => _constantC_Visibility;
        set => SetProperty(ref _constantC_Visibility, value);
    }

    public Visibility IfsC_Visibility
    {
        get => _ifsC_Visibility;
        set => SetProperty(ref _ifsC_Visibility, value);
    }

    public Visibility IfsGeneral_Visibility
    {
        get => _ifsGeneral_Visibility;
        set => SetProperty(ref _ifsGeneral_Visibility, value);
    }

    public Visibility StepDivisorVisibility
    {
        get => _StepDivisorVisibility;
        set => SetProperty(ref _StepDivisorVisibility, value);
    }

    public Visibility BailoutVisibility
    {
        get => _bailoutVisibility;
        set => SetProperty(ref _bailoutVisibility, value);
    }

    public FractalResult Result => _fractalResult;

};


