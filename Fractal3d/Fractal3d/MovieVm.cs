namespace Fractal3d;

using BasicWpfLibrary;
using ImageCalculator;

public class MovieVm : ViewModelBase
{
    private readonly FractalParams _fractalParams;

    public MovieVm(FractalParams fractalParams)
    {
        _fractalParams = fractalParams;
    }

    public string Name => "Movie View Model";
}

