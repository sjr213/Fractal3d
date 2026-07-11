using System.ComponentModel;

namespace ImageCalculator
{
    [Serializable]
    public enum ShaderType
    {
        [Description("Fractal Shader")]
        FractalShader,
        [Description("Crane Shader")]
        CraneShader,
        [Description("Shape Shader")]
        ShapeShader,
        [Description("Crane Pixel")]
        CranePixel,
        [Description("Crane Raymarch")]
        CraneRaymarch,
        [Description("Shadertoy Shader")]
        ShadertoyShader,
        [Description("IFS Shader")]
        IFSShader,
        [Description("L-system Shader")]
        LSystemShader
    }

    public static class ShaderTypeUtils
    {
        public static bool UsesPalette(ShaderType shaderType)
        {
            if (shaderType == ShaderType.FractalShader || shaderType == ShaderType.CranePixel || 
                shaderType == ShaderType.CraneRaymarch || shaderType == ShaderType.ShapeShader || shaderType == ShaderType.LSystemShader)
                return true;

            return false;
        }
    }
}
