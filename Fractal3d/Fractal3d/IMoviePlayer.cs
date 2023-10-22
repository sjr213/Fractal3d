using ImageCalculator.Movie;

namespace Fractal3d
{
    public interface IMoviePlayer
    {
        void PlayMovie(int framesPerSecond);
        bool CanPlayMovie();

        void StopMovie();
        bool CanStopMovie();

        void OnMovieParamsChanged(MovieParams movieParams);
    }
}
