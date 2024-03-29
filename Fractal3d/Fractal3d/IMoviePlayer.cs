﻿using ImageCalculator.Movie;
using System;

namespace Fractal3d;

public interface IMoviePlayer
{
    void PlayMovie(int framesPerSecond);
    bool CanPlayMovie();

    void StopMovie();
    bool CanStopMovie();

    void UpdateCurrentImage(int currentImageIndex);
    bool CanUpdateCurrentImage();

    void MoveImageToQueue(int imageIndex);
    bool CanMoveImageToQueue();

    bool IsMovie();

    void OnMovieParamsChanged(MovieParams movieParams);

    event EventHandler<MovieChangedEventArgs> MovieChanged;
}

