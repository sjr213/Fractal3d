namespace ImageCalculator;

using FractureCommonLib;
using System.Drawing.Imaging;
using System.Drawing;

public static class PaletteImage
{
    public const int Range = 255;

    public static Bitmap CreateBitmapForPalette(Palette palette, DisplayInfo displayInfo)
    {
        return CreateBitmapForPalette(palette, displayInfo, palette.NumberOfColors, 100);
    }

    public static Bitmap CreateBitmapForPalette(Palette palette, DisplayInfo displayInfo,int width, int height)
    {
        Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);

        switch (displayInfo.Mode)
        {
            case DisplayMode.Contrast:
                return GetContrastBitmap(palette, bmp, displayInfo, width, height);

            case DisplayMode.Hsl when displayInfo.Hue:
            {
                if (displayInfo.Saturation)
                {
                    if (displayInfo.Lightness)
                        return GetHslBitmap(palette, bmp, displayInfo, width, height);
                    else
                        return GetHueSatBitmap(palette, bmp, displayInfo, width, height);
                }
                else if (displayInfo.Lightness)
                    return GetHueLightnessBitmap(palette, bmp, displayInfo, width, height);
                else
                    return GetHueBitmap(palette, bmp, displayInfo, width, height);
            }
   
            case DisplayMode.Hsl when displayInfo.Saturation:
            {
                if (displayInfo.Lightness)
                    return GetSaturationLightnessBitmap(palette, bmp, displayInfo, width, height);
                else
                    return GetSaturationBitmap(palette, bmp, displayInfo, width, height);
            }
            case DisplayMode.Hsl when displayInfo.Lightness:
                return GetLightnessBitmap(palette, bmp, displayInfo, width, height);
            default:
                return GetBitmap(palette, bmp, width, height);
        }
    }

    public static Bitmap GetBitmap(Palette palette, Bitmap bmp, int width, int height)
    {
        Rectangle imageRect = new Rectangle(0, 0, width, height);
        BitmapData bmpData;
        IntPtr intptr;

        try
        {
            bmpData = bmp.LockBits(imageRect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            intptr = bmpData.Scan0;
        }
        catch (Exception ex)
        {
            throw new Exception("BulbImage.GetBitmap(): failed during LockBits()", ex);
        }

        // copy colors
        unsafe
        {
            for (int x = 0; x < width; ++x)
            {
                var colorIndex = (double)x * palette.NumberOfColors/ width + 0.499;
                var color = palette.GetColor((int)colorIndex);

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


    private static Bitmap GetContrastBitmap(IPalette palette, Bitmap bmp, DisplayInfo displayInfo, int width, int height)
    {
        double[] stretch = { 0.0, 0.0, 0.0 };
        for (int x = 0; x < 3; x++)
            stretch[x] = ((double)Range / (double)(displayInfo.MaxRgb[x] - displayInfo.MinRgb[x]));


        Rectangle imageRect = new Rectangle(0, 0, width, height);
        BitmapData bmpData;
        IntPtr intptr;

        try
        {
            bmpData = bmp.LockBits(imageRect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            intptr = bmpData.Scan0;
        }
        catch (Exception ex)
        {
            throw new ImageException("GetContrastBitmap(): failed during LockBits()", ex);
        }

        // copy colors
        unsafe
        {
            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    var colorIndex = (double)x * palette.NumberOfColors / width + 0.49999;
                    var color = palette.GetColor((int)colorIndex);
                    int pos = bmpData.Stride * y + x * 4;
                    byte* pixel = (byte*)intptr;

                    var b = ((double)color.B - (double)displayInfo.MinRgb[0]) * stretch[0] + 0.49999;
                    var g = ((double)color.G - (double)displayInfo.MinRgb[1]) * stretch[1] + 0.49999;
                    var r = ((double)color.R - (double)displayInfo.MinRgb[2]) * stretch[2] + 0.49999;

                    Color palColor = Color.FromArgb(color.A, (byte)Math.Min(Math.Max(0, (int)(r)), Range),
                        (byte)Math.Min(Math.Max(0, (int)(g)), Range), (byte)Math.Min(Math.Max(0, (int)(b)), Range));

                    pixel[pos] = palColor.B;
                    pixel[pos + 1] = palColor.G;
                    pixel[pos + 2] = palColor.R;
                    pixel[pos + 3] = palColor.A;
                }
            }
        }

        // unlock
        bmp.UnlockBits(bmpData);

        return bmp;
    }

    public struct DbPair
    {
        public double MinDb;
        public double MaxDb;
    }

    private static DbPair GetMinMaxLightness(IPalette palette, int width, int height)
    {
        var sp = new DbPair
        {
            MaxDb = 0.0,
            MinDb = 1.0
        };

        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                var colorIndex = (double)x * palette.NumberOfColors / width + 0.499;
                var color = palette.GetColor((int)colorIndex);

                HSL hsl = HslConvertor.ToHsl(color);
                sp.MinDb = Math.Min(sp.MinDb, hsl.L);
                sp.MaxDb = Math.Max(sp.MaxDb, hsl.L);
            }
        }

        return sp;
    }

    private static Bitmap GetLightnessBitmap(IPalette palette, Bitmap bmp, DisplayInfo displayInfo, int width, int height)
    {
        // find min and max sat
        DbPair sp = GetMinMaxLightness(palette, width, height);

        // figure scale factor
        double scaleFactor = 1.0;
        if (sp.MaxDb > sp.MinDb)
            scaleFactor = (displayInfo.MaxLightness - displayInfo.MinLightness) / (sp.MaxDb - sp.MinDb);

        Rectangle imageRect = new Rectangle(0, 0, width, height);
        BitmapData bmpData;
        IntPtr intptr;

        try
        {
            bmpData = bmp.LockBits(imageRect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            intptr = bmpData.Scan0;
        }
        catch (Exception ex)
        {
            throw new ImageException("GetLightnessBitmap(): failed during LockBits()", ex);
        }

        // copy colors
        unsafe
        {
            // For each pixel
            //      Calculate sat
            //      Calculate new sat
            //      Convert to new HSL
            //      Convert new HSL to RGB and set
            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    int pos = bmpData.Stride * y + x * 4;
                    var colorIndex = (double)x * palette.NumberOfColors / width + 0.499;
                    var pixelColor = palette.GetColor((int)colorIndex);
                    byte* pixel = (byte*)intptr;

                    HSL hsl = HslConvertor.ToHsl(pixelColor);

                    double newLt = displayInfo.MinLightness + (hsl.L - sp.MinDb) * scaleFactor;
                    hsl.L = newLt;

                    Color newColor = HslConvertor.ToRgb(hsl, pixelColor.A);

                    pixel[pos] = newColor.B;
                    pixel[pos + 1] = newColor.G;
                    pixel[pos + 2] = newColor.R;
                    pixel[pos + 3] = newColor.A;
                }
            }
        }

        // unlock
        bmp.UnlockBits(bmpData);

        return bmp;
    }

    private static DbPair GetMinMaxSaturation(IPalette palette, int width, int height)
    {
        DbPair sp = new DbPair
        {
            MaxDb = 0.0,
            MinDb = 1.0
        };

        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                var colorIndex = (double)x * palette.NumberOfColors / width + 0.499;
                var color = palette.GetColor((int)colorIndex);

                HSL hsl = HslConvertor.ToHsl(color);
                sp.MinDb = Math.Min(sp.MinDb, hsl.S);
                sp.MaxDb = Math.Max(sp.MaxDb, hsl.S);
            }
        }

        return sp;
    }

    private static Bitmap GetSaturationBitmap(IPalette palette, Bitmap bmp, DisplayInfo displayInfo, int width, int height)
    {
        // find min and max sat
        DbPair sp = GetMinMaxSaturation(palette, width, height);

        // figure scale factor
        double scaleFactor = 1.0;
        if (sp.MaxDb > sp.MinDb)
            scaleFactor = (displayInfo.MaxSaturation - displayInfo.MinSaturation) / (sp.MaxDb - sp.MinDb);

        Rectangle imageRect = new Rectangle(0, 0, width, height);
        BitmapData bmpData;
        IntPtr intptr;

        try
        {
            bmpData = bmp.LockBits(imageRect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            intptr = bmpData.Scan0;
        }
        catch (Exception ex)
        {
            throw new ImageException("GetSaturationBitmap(): failed during LockBits()", ex);
        }

        // copy colors
        unsafe
        {
            // For each pixel
            //      Calculate sat
            //      Calculate new sat
            //      Convert to new HSL
            //      Convert new HSL to RGB and set
            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    int pos = bmpData.Stride * y + x * 4;
                    var colorIndex = (double)x * palette.NumberOfColors / width + 0.499;
                    var pixelColor = palette.GetColor((int)colorIndex);
                    byte* pixel = (byte*)intptr;

                    HSL hsl = HslConvertor.ToHsl(pixelColor);

                    double newSat = displayInfo.MinSaturation + (hsl.S - sp.MinDb) * scaleFactor;
                    hsl.S = newSat;

                    Color newColor = HslConvertor.ToRgb(hsl, pixelColor.A);

                    pixel[pos] = newColor.B;
                    pixel[pos + 1] = newColor.G;
                    pixel[pos + 2] = newColor.R;
                    pixel[pos + 3] = newColor.A;
                }
            }
        }

        // unlock
        bmp.UnlockBits(bmpData);

        return bmp;
    }

    private static Bitmap GetSaturationLightnessBitmap(IPalette palette, Bitmap bmp, DisplayInfo displayInfo, int width, int height)
    {
        // find min and max sat
        DbPair satPr = GetMinMaxSaturation(palette, width, height);
        DbPair lightPr = GetMinMaxLightness(palette, width, height);

        // figure scale factor
        double satScaleFactor = 1.0;
        if (satPr.MaxDb > satPr.MinDb)
            satScaleFactor = (displayInfo.MaxSaturation - displayInfo.MinSaturation) / (satPr.MaxDb - satPr.MinDb);

        double lightScaleFactor = 1.0;
        if (lightPr.MaxDb > lightPr.MinDb)
            lightScaleFactor = (displayInfo.MaxLightness - displayInfo.MinLightness) / (lightPr.MaxDb - lightPr.MinDb);

        Rectangle imageRect = new Rectangle(0, 0, width, height);
        BitmapData bmpData;
        IntPtr intptr;

        try
        {
            bmpData = bmp.LockBits(imageRect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            intptr = bmpData.Scan0;
        }
        catch (Exception ex)
        {
            throw new ImageException("GetSaturationLightnessBitmap(): failed during LockBits()", ex);
        }

        // copy colors
        unsafe
        {
            // For each pixel
            //      Calculate hsl
            //      Calculate new hsl
            //      Convert to new HSL
            //      Convert new HSL to RGB and set
            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    int pos = bmpData.Stride * y + x * 4;
                    var colorIndex = (double)x * palette.NumberOfColors / width + 0.499;
                    var pixelColor = palette.GetColor((int)colorIndex);
                    byte* pixel = (byte*)intptr;

                    HSL hsl = HslConvertor.ToHsl(pixelColor);

                    double newSat = displayInfo.MinSaturation + (hsl.S - satPr.MinDb) * satScaleFactor;
                    hsl.S = newSat;

                    double newLight = displayInfo.MinLightness + (hsl.L - lightPr.MinDb) * lightScaleFactor;
                    hsl.L = newLight;

                    Color newColor = HslConvertor.ToRgb(hsl, pixelColor.A);

                    pixel[pos] = newColor.B;
                    pixel[pos + 1] = newColor.G;
                    pixel[pos + 2] = newColor.R;
                    pixel[pos + 3] = newColor.A;
                }
            }
        }

        // unlock
        bmp.UnlockBits(bmpData);

        return bmp;
    }

    private static DbPair GetMinMaxHue(IPalette palette, int width, int height)
    {
        DbPair sp = new DbPair
        {
            MaxDb = 0.0,
            MinDb = 360.0
        };

        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                var colorIndex = (double)x * palette.NumberOfColors / width + 0.499;
                var color = palette.GetColor((int)colorIndex);

                HSL hsl = HslConvertor.ToHsl(color);
                sp.MinDb = Math.Min(sp.MinDb, hsl.H);
                sp.MaxDb = Math.Max(sp.MaxDb, hsl.H);
            }
        }

        return sp;
    }

    private static Bitmap GetHueBitmap(IPalette palette, Bitmap bmp, DisplayInfo displayInfo, int width, int height)
    {
        // find min and max sat
        DbPair sp = GetMinMaxHue(palette, width, height);

        // figure scale factor
        double scaleFactor = 1.0;
        if (sp.MaxDb > sp.MinDb)
            scaleFactor = (displayInfo.MaxHue - displayInfo.MinHue) / (sp.MaxDb - sp.MinDb);

        Rectangle imageRect = new Rectangle(0, 0, width, height);
        BitmapData bmpData;
        IntPtr intptr;

        try
        {
            bmpData = bmp.LockBits(imageRect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            intptr = bmpData.Scan0;
        }
        catch (Exception ex)
        {
            throw new ImageException("GetHueBitmap(): failed during LockBits()", ex);
        }

        // copy colors
        unsafe
        {
            // For each pixel
            //      Calculate hue
            //      Calculate new hue
            //      Convert to new HSL
            //      Convert new HSL to RGB and set
            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    int pos = bmpData.Stride * y + x * 4;
                    var colorIndex = (double)x * palette.NumberOfColors / width + 0.499;
                    var pixelColor = palette.GetColor((int)colorIndex);
                    byte* pixel = (byte*)intptr;

                    HSL hsl = HslConvertor.ToHsl(pixelColor);

                    double newHue = displayInfo.MinSaturation + (hsl.H - sp.MinDb) * scaleFactor;
                    hsl.H = newHue;

                    Color newColor = HslConvertor.ToRgb(hsl, pixelColor.A);

                    pixel[pos] = newColor.B;
                    pixel[pos + 1] = newColor.G;
                    pixel[pos + 2] = newColor.R;
                    pixel[pos + 3] = newColor.A;
                }
            }
        }

        // unlock
        bmp.UnlockBits(bmpData);

        return bmp;
    }

    private static Bitmap GetHueLightnessBitmap(IPalette palette, Bitmap bmp, DisplayInfo displayInfo, int width, int height)
    {
        // find min and max sat
        DbPair huePr = GetMinMaxHue(palette, width, height);
        DbPair lightPr = GetMinMaxLightness(palette, width, height);

        // figure scale factor
        double lightScaleFactor = 1.0;
        if (lightPr.MaxDb > lightPr.MinDb)
            lightScaleFactor = (displayInfo.MaxLightness - displayInfo.MinLightness) / (lightPr.MaxDb - lightPr.MinDb);

        double hueScaleFactor = 1.0;
        if (huePr.MaxDb > huePr.MinDb)
            hueScaleFactor = (displayInfo.MaxHue - displayInfo.MinHue) / (huePr.MaxDb - huePr.MinDb);

        Rectangle imageRect = new Rectangle(0, 0, width, height);
        BitmapData bmpData;
        IntPtr intptr;

        try
        {
            bmpData = bmp.LockBits(imageRect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            intptr = bmpData.Scan0;
        }
        catch (Exception ex)
        {
            throw new ImageException("GetHueLightnessBitmap(): failed during LockBits()", ex);
        }

        // copy colors
        unsafe
        {
            // For each pixel
            //      Calculate hsl
            //      Calculate new hsl
            //      Convert to new HSL
            //      Convert new HSL to RGB and set
            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    int pos = bmpData.Stride * y + x * 4;
                    var colorIndex = (double)x * palette.NumberOfColors / width + 0.499;
                    var pixelColor = palette.GetColor((int)colorIndex);
                    byte* pixel = (byte*)intptr;

                    HSL hsl = HslConvertor.ToHsl(pixelColor);

                    double newLight = displayInfo.MinLightness + (hsl.L - lightPr.MinDb) * lightScaleFactor;
                    hsl.L = newLight;

                    double newHue = displayInfo.MinHue + (hsl.H - huePr.MinDb) * hueScaleFactor;
                    hsl.H = newHue;

                    Color newColor = HslConvertor.ToRgb(hsl, pixelColor.A);

                    pixel[pos] = newColor.B;
                    pixel[pos + 1] = newColor.G;
                    pixel[pos + 2] = newColor.R;
                    pixel[pos + 3] =newColor.A;
                }
            }
        }

        // unlock
        bmp.UnlockBits(bmpData);

        return bmp;
    }

    private static Bitmap GetHueSatBitmap(IPalette palette, Bitmap bmp, DisplayInfo displayInfo, int width, int height)
    {
        // find min and max sat
        DbPair huePr = GetMinMaxHue(palette, width, height);
        DbPair satPr = GetMinMaxSaturation(palette, width, height);

        // figure scale factor
        double satScaleFactor = 1.0;
        if (satPr.MaxDb > satPr.MinDb)
            satScaleFactor = (displayInfo.MaxSaturation - displayInfo.MinSaturation) / (satPr.MaxDb - satPr.MinDb);

        double hueScaleFactor = 1.0;
        if (huePr.MaxDb > huePr.MinDb)
            hueScaleFactor = (displayInfo.MaxHue - displayInfo.MinHue) / (huePr.MaxDb - huePr.MinDb);

        Rectangle imageRect = new Rectangle(0, 0, width, height);
        BitmapData bmpData;
        IntPtr intptr;

        try
        {
            bmpData = bmp.LockBits(imageRect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            intptr = bmpData.Scan0;
        }
        catch (Exception ex)
        {
            throw new ImageException("GetHueSatBitmap(): failed during LockBits()", ex);
        }

        // copy colors
        unsafe
        {
            // For each pixel
            //      Calculate hsl
            //      Calculate new hsl
            //      Convert to new HSL
            //      Convert new HSL to RGB and set
            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    int pos = bmpData.Stride * y + x * 4;
                    var colorIndex = (double)x * palette.NumberOfColors / width + 0.499;
                    var pixelColor = palette.GetColor((int)colorIndex);
                    byte* pixel = (byte*)intptr;

                    HSL hsl = HslConvertor.ToHsl(pixelColor);

                    double newSat = displayInfo.MinSaturation + (hsl.S - satPr.MinDb) * satScaleFactor;
                    hsl.S = newSat;

                    double newHue = displayInfo.MinHue + (hsl.H - huePr.MinDb) * hueScaleFactor;
                    hsl.H = newHue;

                    Color newColor = HslConvertor.ToRgb(hsl, pixelColor.A);

                    pixel[pos] = newColor.B;
                    pixel[pos + 1] = newColor.G;
                    pixel[pos + 2] = newColor.R;
                    pixel[pos + 3] = newColor.A;
                }
            }
        }

        // unlock
        bmp.UnlockBits(bmpData);

        return bmp;
    }

    private static Bitmap GetHslBitmap(IPalette palette, Bitmap bmp, DisplayInfo displayInfo, int width, int height)
    {
        // find min and max sat
        DbPair huePr = GetMinMaxHue(palette, width, height);
        DbPair satPr = GetMinMaxSaturation(palette, width, height);
        DbPair lightPr = GetMinMaxLightness(palette, width, height);

        // figure scale factor
        double satScaleFactor = 1.0;
        if (satPr.MaxDb > satPr.MinDb)
            satScaleFactor = (displayInfo.MaxSaturation - displayInfo.MinSaturation) / (satPr.MaxDb - satPr.MinDb);

        double lightScaleFactor = 1.0;
        if (lightPr.MaxDb > lightPr.MinDb)
            lightScaleFactor = (displayInfo.MaxLightness - displayInfo.MinLightness) / (lightPr.MaxDb - lightPr.MinDb);

        double hueScaleFactor = 1.0;
        if (huePr.MaxDb > huePr.MinDb)
            hueScaleFactor = (displayInfo.MaxHue - displayInfo.MinHue) / (huePr.MaxDb - huePr.MinDb);

        Rectangle imageRect = new Rectangle(0, 0, width, height);
        BitmapData bmpData;
        IntPtr intptr;

        try
        {
            bmpData = bmp.LockBits(imageRect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            intptr = bmpData.Scan0;
        }
        catch (Exception ex)
        {
            throw new ImageException("GetHslBitmap(): failed during LockBits()", ex);
        }

        // copy colors
        unsafe
        {
            // For each pixel
            //      Calculate hsl
            //      Calculate new hsl
            //      Convert to new HSL
            //      Convert new HSL to RGB and set
            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    int pos = bmpData.Stride * y + x * 4;
                    var colorIndex = (double)x * palette.NumberOfColors / width + 0.499;
                    var pixelColor = palette.GetColor((int)colorIndex);
                    byte* pixel = (byte*)intptr;

                    HSL hsl = HslConvertor.ToHsl(pixelColor);

                    double newSat = displayInfo.MinSaturation + (hsl.S - satPr.MinDb) * satScaleFactor;
                    hsl.S = newSat;

                    double newLight = displayInfo.MinLightness + (hsl.L - lightPr.MinDb) * lightScaleFactor;
                    hsl.L = newLight;

                    double newHue = displayInfo.MinHue + (hsl.H - huePr.MinDb) * hueScaleFactor;
                    hsl.H = newHue;

                    Color newColor = HslConvertor.ToRgb(hsl, pixelColor.A);

                    pixel[pos] = newColor.B;
                    pixel[pos + 1] = newColor.G;
                    pixel[pos + 2] = newColor.R;
                    pixel[pos + 3] = newColor.A;
                }
            }
        }

        // unlock
        bmp.UnlockBits(bmpData);

        return bmp;
    }

}

