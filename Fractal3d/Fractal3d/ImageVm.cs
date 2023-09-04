using System;
using System.Drawing;
using System.Windows;
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
        private readonly Action<Rect> _setSelectionRect;
        #endregion

        #region construction

        public ImageVm(FractalParams fractalParams, BitmapImage image, Action<Rect> setSelectionRect)
        {
            _fractalParams = fractalParams;
            Image = image;
            Width = fractalParams.DisplaySize.Width;
            Height = fractalParams.DisplaySize.Height;

#pragma warning disable CS8604
            ImageVmLeftMouseUpCommand = new RelayCommand(param => ExecuteLeftMouseUp((param as MouseEventArgs)));
            ImageVmLeftMouseDownCommand = new RelayCommand(param => ExecuteLeftMouseDown((param as MouseEventArgs)));
            _setSelectionRect = setSelectionRect;
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

        private double _rectWidth;
        public double RectWidth
        {
            get => _rectWidth;
            set => SetProperty(ref _rectWidth, value);
        }

        private double _rectHeight;
        public double RectHeight
        {
            get => _rectHeight;
            set => SetProperty(ref _rectHeight, value);
        }

        private double _rectLeft;
        public double RectLeft
        {
            get => _rectLeft; 
            set => SetProperty(ref _rectLeft, value);
        }

        private double _rectTop;

        public double RectTop
        {
            get => _rectTop; 
            set => SetProperty(ref _rectTop, value);
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

            _endPt = e.GetPosition(canvas);

            if (CalculateImageRange(canvas.Width, canvas.Height) == false)
                return;

            DrawRectangle();
        }

        private void ExecuteLeftMouseDown(MouseEventArgs e)
        {
            var canvas = GetCanvas(e.Source);
            if (canvas == null)
                return;

            if (canvas.Width == 0)
                return;

            ClearRectangle();

            _startPt = e.GetPosition(canvas);
        }

        private bool CalculateImageRange(double width, double height)
        {
            if(width == 0 || height == 0) return false;

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

            return true;
        }

        private void DrawRectangle()
        {
            var width = _endPt.X - _startPt.X;
            var height = _endPt.Y - _startPt.Y;

            RectWidth = width;
            RectHeight = height;
            RectLeft = _startPt.X;
            RectTop = _startPt.Y;

            var relativeX = _startPt.X / Width;
            var relativeY = _startPt.Y / Height;
            var relativeWidth = width / Width;
            var relativeHeight = height / Height;

            var rect = new Rect(relativeX, relativeY, relativeWidth, relativeHeight);
            _setSelectionRect(rect);
        }

        public void ClearRectangle()
        {
            RectWidth = 0;
            RectHeight = 0;
            RectLeft = 0;
            RectTop = 0;
        }

        public void SetFractalParams(FractalParams fractalParams)
        {
            _fractalParams = fractalParams;
            Width = fractalParams.DisplaySize.Width;
            Height = fractalParams.DisplaySize.Height;

            // Reset rectangle
            ClearRectangle();

        }

        #endregion
    }
}
