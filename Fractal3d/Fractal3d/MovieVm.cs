using System.Collections.Generic;
using BasicWpfLibrary;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System;
using System.Reactive.Subjects;
using ImageCalculator;
using ImageCalculator.Movie;

namespace Fractal3d
{
    public class MovieVm : ViewModelBase, IObservable<int>
    {
        private List<BitmapImage> _movieImages = new List<BitmapImage>();
        private FractalParams _fractalParams = new FractalParams();
        private DispatcherTimer? _timer;
        private int _currentImage;
        private bool _isRunning;

        #region Properties

        private BitmapImage _image = new();
        public BitmapImage Image
        {
            get => _image;
            set => SetProperty(ref _image, value);
        }

        private int _width;
        public int Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }

        private int _height;
        public int Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }

        #endregion

        #region methods

        public void SetImages(List<BitmapImage> images, FractalParams fractalParams)
        {
            _movieImages = images;
            _fractalParams = fractalParams;

            if (images.Count > 0)
            {
                Image = images[0];
            }
        }

        public void Start(int framesPerSecond)
        {
            if (_movieImages.Count < 2 || _isRunning)
                return;

            _isRunning = true;

            var timeMs = 1000 / 5;
            if (framesPerSecond is >= MovieConstants.MinFramesPerSecond and <= MovieConstants.MaxFramesPerSecond)
                timeMs = 1000 / framesPerSecond;

            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(timeMs) };
            _timer.Tick += TimerTick;
            _timer.Start();
        }

        public void Stop()
        {
            if(_timer != null)
                _timer.Stop();

            _isRunning = false;
        }

        public void UpdateCurrentImage(int currentImageIndex)
        {
            if (_isRunning)
                return;

            if (currentImageIndex > _movieImages.Count || currentImageIndex < 0)
                return;

            if (currentImageIndex == _currentImage)
                return;

            _currentImage = currentImageIndex;

            ChangeImage();
        }

        private void TimerTick(object? sender, EventArgs e)
        {
            if (_movieImages.Count < 1)
                return;

            ++_currentImage;

            ChangeImage();
        }

        private void ChangeImage()
        {
            if (_currentImage >= _movieImages.Count)
                _currentImage = 0;

            Image = _movieImages[_currentImage];
            Width = _fractalParams.DisplaySize.Width;
            Height = _fractalParams.DisplaySize.Height;

            _currentImageObserver.OnNext(_currentImage);
        }

        #endregion

        #region IObserver<int>

        private readonly Subject<int> _currentImageObserver = new Subject<int>();

        public virtual IDisposable Subscribe(IObserver<int> observer)
        {
            return _currentImageObserver.Subscribe(observer);
        }

        #endregion
    }
}
