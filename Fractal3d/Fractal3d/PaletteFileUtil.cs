namespace Fractal3d;

using BasicWpfLibrary;
using FractureCommonLib;
using ImageCalculator;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

public class PaletteItem : ViewModelBase
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public PaletteItem(Palette pal, Bitmap bmp, BitmapImage bmpImage)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        Pal = pal;
        Bmp = bmp;
        BmpImage = bmpImage;
    }

    private Palette _palette;
    public Palette Pal 
    { 
        get { return _palette;  }
        set
        {
            _palette = value;
            Name = _palette.PaletteName;
            OnPropertyChanged(nameof(Pal));
        }
    }

    private string _paletteName = "";
    public string Name
    {
        get { return _paletteName; }
        set
        {
            _paletteName = value;
            OnPropertyChanged(nameof(Name));
        }
    }

    private Bitmap _bitmap;
    public Bitmap Bmp
    {
        get { return _bitmap; }
        set 
        { 
            _bitmap = value;
            OnPropertyChanged(nameof(Bmp));
        }
    }

    private BitmapImage _bmpImage;
    public BitmapImage BmpImage
    {
        get { return _bmpImage; }
        set 
        { 
            _bmpImage = value;
            OnPropertyChanged(nameof(BmpImage));
        }
    }
}

public static class PaletteFileUtil
{
    public static Palette? LoadPalette(string filename)
    {
        try
        {
            string jsonString = System.IO.File.ReadAllText(filename);

            return JsonConvert.DeserializeObject<Palette>(jsonString);
        }
        catch (System.Exception)
        {
        }

        return null;
    }

    public static ObservableCollection<Palette> LoadPalettesFromDirectory(string path)
    {
        ObservableCollection<Palette> paletteList = new();

        DirectoryInfo dir = new DirectoryInfo(path);

        if (dir.Exists)
        {
            string fileFilter = @"*.jpal";
            IEnumerable<FileInfo> files = dir.EnumerateFiles(fileFilter);

            foreach (var file in files)
            {
                Palette? palette = LoadPalette(file.FullName);
                if (palette != null)
                {
                    paletteList.Add(palette);
                }
            }
        }

        return paletteList;
    }

    public static ObservableCollection<PaletteItem> LoadPaletteItems(string path)
    {
        const int width = 500;
        const int height = 10;
        ObservableCollection<PaletteItem> paletteItems = new();

        if (string.IsNullOrEmpty(path))
            return paletteItems;

        try
        {
            DirectoryInfo dir = new DirectoryInfo(path);

            if (dir.Exists)
            {
                string fileFilter = @"*.jpal";
                IEnumerable<FileInfo> files = dir.EnumerateFiles(fileFilter);

                foreach (var file in files)
                {
                    Palette? palette = LoadPalette(file.FullName);
                    if (palette != null)
                    {
                        try
                        {
                            palette.NumberOfColors = width;
                            var bmp = ImageUtilities.CreateBitmapForPalette(palette, width, height);
                            var image = ImageUtil.BitmapToImageSource(bmp);
                            paletteItems.Add(new PaletteItem(palette, bmp, image));
                        }
                        catch (System.Exception)
                        { }
                    }
                }
            }
        }
        catch(System.Exception)
        { }

        return paletteItems;
    }
}
