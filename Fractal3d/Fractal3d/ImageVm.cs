using System;
using System.Drawing;
using BasicWpfLibrary;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using ImageCalculator;
using Image = System.Windows.Controls.Image;

namespace Fractal3d
{
    public class ImageVm : ViewModelBase
    {
        #region members
        private FractalParams _fractalParams;
        private System.Windows.Point _startPt;
        private System.Windows.Point _endPt;
        private PointF _fromPt;
        private PointF _toPt;
        #endregion

        #region construction

        public ImageVm(FractalParams fractalParams, BitmapImage image)
        {
            _fractalParams = fractalParams;
            Image = image;
            Width = fractalParams.DisplaySize.Width;
            Height = fractalParams.DisplaySize.Height;

#pragma warning disable CS8604
            ImageVmLeftMouseUpCommand = new RelayCommand(param => ExecuteLeftMouseUp((param as MouseEventArgs)));
            ImageVmLeftMouseDownCommand = new RelayCommand(param => ExecuteLeftMouseDown((param as MouseEventArgs)));
#pragma warning restore CS8604
        }
        #endregion

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

        public RelayCommand ImageVmLeftMouseUpCommand { get; }

        public RelayCommand ImageVmLeftMouseDownCommand { get; }

        #endregion

        #region methods

        private Canvas? GetCanvas(object obj)
        {
            var image = (Image)obj;

            return image.FindParentOfType<Canvas>();
        }

        private void ExecuteLeftMouseUp(MouseEventArgs e)
        {
            var canvas = GetCanvas(e.Source);
            if (canvas == null)
                return;

            if (canvas.Width == 0)
                return;

            _endPt = e.GetPosition(canvas);

            CalculateImageRange(canvas.Width, canvas.Height);
        }

        private void ExecuteLeftMouseDown(MouseEventArgs e)
        {
            var canvas = GetCanvas(e.Source);
            if (canvas == null)
                return;

            if (canvas.Width == 0)
                return;

            _startPt = e.GetPosition(canvas);
        }

        private void CalculateImageRange(double width, double height)
        {
            if(width == 0 || height == 0) return;

            double fractalWidth = _fractalParams.ToX - _fractalParams.FromX;

            float x1 = (float)(_startPt.X / width * fractalWidth + _fractalParams.FromX);
            float x2 = (float)(_endPt.X / width * fractalWidth + _fractalParams.FromX);

            double fractalHeight = _fractalParams.ToY - _fractalParams.FromY;

            float y1 = (float)(_startPt.Y / height * fractalHeight + _fractalParams.FromY);
            float y2 = (float)(_endPt.Y / height * fractalHeight + _fractalParams.FromY);

            _fromPt.X  = Math.Min(x1, x2);
            _fromPt.Y = Math.Min(y1, y2);

            _toPt.X = Math.Max(x1, x2);
            _toPt.Y = Math.Max(y1, y2);
        }

        public void SetFractalParams(FractalParams fractalParams)
        {
            _fractalParams = fractalParams;
            Width = fractalParams.DisplaySize.Width;
            Height = fractalParams.DisplaySize.Height;
            // Reset rectangle
        }

        #endregion
    }
}
