using FractureCommonLib;

namespace ImageCalculator
{
    [Serializable]
    public class FractalResult : ICloneable
    {
        public FractalParams? Params { get; set; }
        public RawLightedImage? Image { get; set; }
        public long Time { get; set; } = 0;

        public object Clone()
        {
            var copy = new FractalResult
            {
                Params = (FractalParams)Params?.Clone()!,
                Image = (RawLightedImage)Image?.Clone()!,
                Time = Time
            };
            return copy;
        }
    }
}
