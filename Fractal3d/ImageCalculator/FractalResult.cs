using FractureCommonLib;

namespace ImageCalculator
{
    [Serializable]
    public class FractalResult : ICloneable
    {
        public FractalParams? Params { get; set; }
        public RawLightedImage? Image { get; set; }

        public object Clone()
        {
            var copy = new FractalResult
            {
                Params = (FractalParams)Params?.Clone()!,
                Image = (RawLightedImage)Image?.Clone()!
            };
            return copy;
        }
    }
}
