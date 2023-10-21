using ImageCalculator.Movie;

namespace Fractal3d
{
    public interface IMoviePlayer
    {
        void PlayMovie();
        bool CanPlayMovie();

        void StopMovie();
        bool CanStopMovie();

        void OnMovieParamsChanged(MovieParams movieParams);
    }
}
