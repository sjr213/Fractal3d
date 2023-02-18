using FractureCommonLib;
using System.Drawing;
using System.Drawing.Imaging;

namespace ImageCalculator;

public static class ImageUtilities
{
    public static Bitmap CreateBitmapForPalette(IPalette palette)
    {
        const int Height = 100;
        Bitmap bmp = new Bitmap(palette.NumberOfColors, Height, PixelFormat.Format32bppArgb);

        Rectangle imageRect = new Rectangle(0, 0, palette.NumberOfColors, Height);
        System.Drawing.Imaging.BitmapData bmpData;
        IntPtr intptr;

        try
        {
            bmpData = bmp.LockBits(imageRect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            intptr = bmpData.Scan0;
        }
        catch (System.Exception ex)
        {
            throw new Exception("BulbImage.GetBitmap(): failed during LockBits()", ex);
        }

        // copy colors
        unsafe
        {
            for (int x = 0; x < palette.NumberOfColors; ++x)
            {
                var color = palette.GetColor(x);

                for(int y = 0; y < Height; ++y)
                {
                    int pos = bmpData.Stride * y + x * 4;

                    byte* pixel = (byte*)intptr;

                    pixel[pos] = color.B;
                    pixel[pos + 1] = color.G;
                    pixel[pos + 2] = color.R;
                    pixel[pos + 3] = color.A;
                }

            }
        }

        // unlock
        bmp.UnlockBits(bmpData);

        return bmp;
    }

    public static Bitmap CreateBitmapForPalette(IPalette palette, int width, int height)
    {
        Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);

        Rectangle imageRect = new Rectangle(0, 0, width, height);
        System.Drawing.Imaging.BitmapData bmpData;
        IntPtr intptr;

        try
        {
            bmpData = bmp.LockBits(imageRect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            intptr = bmpData.Scan0;
        }
        catch (System.Exception ex)
        {
            throw new Exception("BulbImage.GetBitmap(): failed during LockBits()", ex);
        }

        // copy colors
        unsafe
        {
            for (int x = 0; x < width; ++x)
            {
                int colorPos = Math.Min((int)((double)x / width * palette.NumberOfColors), palette.NumberOfColors-1);
                var color = palette.GetColor(colorPos);

                for (int y = 0; y < height; ++y)
                {
                    int pos = bmpData.Stride * y + x * 4;

                    byte* pixel = (byte*)intptr;

                    pixel[pos] = color.B;
                    pixel[pos + 1] = color.G;
                    pixel[pos + 2] = color.R;
                    pixel[pos + 3] = color.A;
                }

            }
        }

        // unlock
        bmp.UnlockBits(bmpData);

        return bmp;
    }


}
