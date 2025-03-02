using System;
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
        private Point _startPt;
        private Point _endPt;
        private readonly Action<Rect> _setSelectionRect;
        private bool _mouseCaptured;
        #endregion

        #region construction

        public ImageVm(FractalParams fractalParams, BitmapImage image, Action<Rect> setSelectionRect)
        {
            Image = image;
            Width = fractalParams.DisplaySize.Width;
            Height = fractalParams.DisplaySize.Height;

#pragma warning disable CS8604
            ImageVmLeftMouseUpCommand = new RelayCommand(param => ExecuteLeftMouseUp((param as MouseEventArgs)));
            ImageVmLeftMouseDownCommand = new RelayCommand(param => ExecuteLeftMouseDown((param as MouseEventArgs)));
            ImageVmMouseMoveCommand = new RelayCommand(param => ExecuteMouseMove((param as MouseEventArgs)));
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

        public RelayCommand ImageVmMouseMoveCommand { get; }

        #endregion

        #region methods

        private Canvas? GetCanvas(object obj)
        {
            var image = obj as Image;
            return image?.FindParentOfType<Canvas>();
        }

        private void ExecuteLeftMouseUp(MouseEventArgs e)
        {
            _mouseCaptured = false;

            var canvas = GetCanvas(e.Source);
            if (canvas == null)
                return;
            _endPt = e.GetPosition(canvas);

            if (canvas.Width == 0 || canvas.Height == 0) return;

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
            _mouseCaptured = true;
        }

        private void ExecuteMouseMove(MouseEventArgs e)
        {
            if (_mouseCaptured == false)
                return;

            var canvas = GetCanvas(e.Source);
            if (canvas == null)
                return;

            _endPt = e.GetPosition(canvas);

            if (canvas.Width == 0 || canvas.Height == 0) return;

            DrawRectangle();
        }

        private void DrawRectangle()
        {
            var minX = Math.Min(_startPt.X, _endPt.X);
            var maxX = Math.Max(_startPt.X, _endPt.X);
            var minY = Math.Min(_startPt.Y, _endPt.Y);
            var maxY = Math.Max(_startPt.Y, _endPt.Y);

            var width = maxX - minX;
            var height = maxY - minY;

            RectWidth = width;
            RectHeight = height;
            RectLeft = minX;
            RectTop = minY;

            var relativeX = minX / Width;
            var relativeY = minY / Height;
            var relativeWidth = width / Width;
            var relativeHeight = height / Height;

            if (relativeWidth <= 0 || relativeHeight <= 0)
                return;

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
            Width = fractalParams.DisplaySize.Width;
            Height = fractalParams.DisplaySize.Height;

            // Reset rectangle
            ClearRectangle();
        }

        #endregion
    }
}
