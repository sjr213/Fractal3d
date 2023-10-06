namespace ImageCalculator.Movie;

[Serializable]
public record MovieResult
{
    public MovieParams? Params { get; set; }
    public List<FractalResult> Results { get; set; } = new List<FractalResult>();
}

