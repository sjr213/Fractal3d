namespace BasicWpfLibrary;

using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

public static class ImageUtil
{
    // from https://stackoverflow.com/questions/94456/load-a-wpf-bitmapimage-from-a-system-drawing-bitmap
    public static BitmapImage BitmapToImageSource(Bitmap bitmap)
    {
        using (MemoryStream memory = new MemoryStream())
        {
            bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
            memory.Position = 0;
            BitmapImage bitmapimage = new BitmapImage();
            bitmapimage.BeginInit();
            bitmapimage.StreamSource = memory;
            bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapimage.EndInit();

            return bitmapimage;
        }
    }

    public static System.Windows.Media.Color FromDrawingToMediaColor(System.Drawing.Color dColor)
    {
        return System.Windows.Media.Color.FromArgb(dColor.A, dColor.R, dColor.G, dColor.B);
    }

    public static System.Drawing.Color FromMediaToDrawingColor(System.Windows.Media.Color mColor)
    {
        return System.Drawing.Color.FromArgb(mColor.A, mColor.R, mColor.G, mColor.B);
    }
}
