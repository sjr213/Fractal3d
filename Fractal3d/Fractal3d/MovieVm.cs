﻿using System.Collections.Generic;
using BasicWpfLibrary;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System;

namespace Fractal3d
{
    public class MovieVm : ViewModelBase
    {
        private List<BitmapImage> _movieImages = new List<BitmapImage>();
        private DispatcherTimer? _timer;
        private int _currentImage = 0;

        public MovieVm()
        {
        }

        #region Properties

        private BitmapImage _image = new();
        public BitmapImage Image
        {
            get => _image;
            set => SetProperty(ref _image, value);
        }

        #endregion

        #region methods

        void SetImages(List<BitmapImage> images)
        {
            _movieImages = images;

            if (images.Count > 0)
            {
                Image = images[0];
            }
        }

        public void Start()
        {
            if (_movieImages.Count < 2)
                return;

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += TimerTick;
            _timer.Start();
        }

        public void Stop()
        {
            if(_timer != null)
                _timer.Stop();
        }

        private void TimerTick(object? sender, EventArgs e)
        {
            if (_movieImages.Count < 1)
                return;

            ++_currentImage;

            if (_currentImage >= _movieImages.Count)
                _currentImage = 0;

            Image = _movieImages[_currentImage];
        }

        #endregion
    }
}
