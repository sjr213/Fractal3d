using BasicWpfLibrary;
using System.Windows.Media.Imaging;

namespace Fractal3d
{
    public class ImageVm : ViewModelBase
    {
        #region construction

        public ImageVm(BitmapImage image, int width, int height)
        {
            Image = image;
            Width = width;
            Height = height;
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

        public void UpdateViewSize(int width, int height)
        {
            Width = width;
            Height = height;
        }
#endregion
    }
}
