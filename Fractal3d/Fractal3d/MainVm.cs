namespace Fractal3d;

using BasicWpfLibrary;
using FractureCommonLib;
using ImageCalculator;
using Microsoft.Win32;
using Newtonsoft.Json;
using System.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Reactive.Linq;
using System.Threading;
using System.IO;
using System.Drawing.Imaging;

//using System.Windows.Forms;

public class MainVm : ViewModelBase, IDisposable
{
    const int NumberOfColors = 1000;

    private readonly FractalFactory _fractalFactory = new();
    private FractalParams _fractalParams = new() { Palette = PaletteFactory.CreateStandardPalette(NumberOfColors) };
    private FractalResult? _fractalResult;
    private bool _isDisposed;
    private readonly IDisposable? _progressSubject;
    private CancellationTokenSource _cancelSource = new();
    private int _fractalNumber = 1;
     
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public MainVm()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        _calculateCommand = new RelayCommand(_ => Calculate(), _ => CanCalculate());
        _saveAllCommand = new RelayCommand(_ => OnSaveAll(), _ => _fractalResult != null);
        _saveOneCommand = new RelayCommand(_ => OnSaveOne(), _ => _fractalResult != null);
        _saveImageCommand = new RelayCommand(_ => OnSaveImage(), _ => _fractalResult != null);
        _deleteCommand = new RelayCommand(_ => OnDelete(), _ => _fractalResult != null);
        _openCommand = new RelayCommand(_ => OnOpen(), _ => true);
        _cancelCommand = new RelayCommand(_ => OnCancel(), _ => true);  // This should only be visible when calculating because it's in the progress Stack panel
        MakePaletteViewModel();
        ParameterViewModel = new ParameterVm(_fractalParams, OnParamsChanged);
        LightingViewModel = new LightingVm(_fractalParams, OnParamsChanged);
        Width = _fractalParams.DisplaySize.Width;
        Height = _fractalParams.DisplaySize.Height;

        if (SynchronizationContext.Current != null)
        {
            _progressSubject = _fractalFactory.Progress.ObserveOn(SynchronizationContext.Current)
                .Subscribe(progress => PercentProgress = progress);
        }
    }

    public void Dispose()
    {
        Dispose(true);

        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed)
            return;

        if (disposing)
        {
            _progressSubject?.Dispose();
            _fractalFactory.Dispose();
        }

        _isDisposed = true;
    }

    private readonly RelayCommand _calculateCommand;   
    public ICommand CalculateCommand => _calculateCommand;

    private readonly RelayCommand _saveAllCommand;
    public ICommand SaveAllCommand => _saveAllCommand;

    private readonly RelayCommand _saveOneCommand;
    public ICommand SaveOneCommand => _saveOneCommand;

    private readonly RelayCommand _saveImageCommand;
    public ICommand SaveImageCommand => _saveImageCommand;

    private readonly RelayCommand _deleteCommand;
    public ICommand DeleteCommand => _deleteCommand;

    private readonly RelayCommand _openCommand;
    public ICommand OpenCommand => _openCommand;

    private readonly RelayCommand _cancelCommand;
    public ICommand CancelCommand => _cancelCommand;

    protected async void Calculate()
    {
        _cancelSource = new();
        var cancelToken = _cancelSource.Token;
        ProgressVisibility = Visibility.Visible;

        _fractalResult = await _fractalFactory.CreateFractalAsync(_fractalParams, cancelToken);

        if (cancelToken.IsCancellationRequested)
        {
            ProgressVisibility = Visibility.Hidden;
            return;
        }

        if (_fractalResult == null)
            return;

        DisplayImage(_fractalResult);

        ProgressVisibility = Visibility.Hidden;

        FractalResults.Add(new FractalResultVm((FractalResult)_fractalResult.Clone(), _fractalNumber++));
    }

    protected void DisplayImage(FractalResult result)
    {
        if (result.Image == null || result.Params == null)
            return;

        var bmp = result.Image.GetBitmap(_fractalParams.Palette, _fractalParams.ColorInfo, _fractalParams.Light.AmbientPower);

        Width = result.Params.DisplaySize.Width;
        Height = result.Params.DisplaySize.Height;

        Image = ImageUtil.BitmapToImageSource(bmp);
    }

    protected bool CanCalculate()
    {
        return AreParametersValid();
    }

    private BitmapImage _image = new();
    public BitmapImage Image
    {
        get => _image;
        set => SetProperty(ref _image, value);
    }

    private PaletteVm _paletteVm;
    public PaletteVm PaletteViewModel
    {
        get => _paletteVm;
        set => SetProperty(ref _paletteVm, value);
    }

    // We will need to update the palette when the number of colors change
    private ParameterVm _parameterVm;
    public ParameterVm ParameterViewModel
    {
        get => _parameterVm;
        set => SetProperty(ref _parameterVm, value);
    }

    private LightingVm _lightingVm;
    public LightingVm LightingViewModel
    {
        get => _lightingVm;
        set => SetProperty(ref _lightingVm, value);
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

    private Visibility _progressVisibility = Visibility.Hidden;
    public Visibility ProgressVisibility
    {
        get => _progressVisibility;
        set => SetProperty(ref _progressVisibility, value);
    }

    private int _percentProgress;
    public int PercentProgress
    {
        get => _percentProgress;
        set => SetProperty(ref _percentProgress, value);
    }

    protected void OnPaletteChanged(Palette palette)
    {
        _fractalParams.Palette = palette;
    }

    protected void OnParamsChanged(FractalParams fractalParams)
    {
        _fractalParams = fractalParams;
        Width = _fractalParams.DisplaySize.Width;
        Height = _fractalParams.DisplaySize.Height;
    }

    protected void OnSaveAll()
    {
        if (_fractalResult == null)
            return;

        SaveFileDialog saveFileDialog = new SaveFileDialog
        {
            Filter = "Fractal3D file (*.f3d)|*.f3d"
        };

        if (saveFileDialog.ShowDialog() != true) return;
        try
        {
            var results = FractalResults.Select(rvm => rvm.Result).ToList();
            string jsonString = JsonConvert.SerializeObject(results);

            //write string to file
            File.WriteAllText(saveFileDialog.FileName, jsonString);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Cannot save result to file");
        }
    }

    protected void OnSaveOne()
    {
        if (_fractalResult == null)
            return;

        var saveFileDialog = new SaveFileDialog
        {
            Filter = "Fractal3D file (*.f3d)|*.f3d"
        };

        if (saveFileDialog.ShowDialog() != true) return;
        try
        {
            List<FractalResult> results = new() { _fractalResult };
            string jsonString = JsonConvert.SerializeObject(results);

            //write string to file
            File.WriteAllText(saveFileDialog.FileName, jsonString);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Cannot save result to file");
        }
    }

    protected void OnSaveImage()
    {
        if (_fractalResult == null || _fractalResult.Image == null)
            return;

        var saveFileDialog = new SaveFileDialog
        {
            Filter = "Jpeg Files (*.jpg)|*.jpg|bitmap Files (*.bmp)|*.bmp|Png Files (*.png)|*.png"
        };

        if (saveFileDialog.ShowDialog() != true)
            return;
        
        string filename = saveFileDialog.FileName;
        string ext = Path.GetExtension(filename);

        var bmp = _fractalResult.Image.GetBitmap(_fractalParams.Palette, _fractalParams.ColorInfo, _fractalParams.Light.AmbientPower);

        if (string.Compare(ext, ".jpg", StringComparison.OrdinalIgnoreCase) == 0)
        {
            SaveAsJpg(bmp, filename);
        }
        else if (string.Compare(ext, ".bmp", StringComparison.OrdinalIgnoreCase) == 0)
        {
            bmp.Save(filename, ImageFormat.Bmp);
        }
        else if (string.Compare(ext, ".png", StringComparison.OrdinalIgnoreCase) == 0)
        { 
            SaveAsPng(bmp, filename);
        }
    }

    private ImageCodecInfo? GetEncoderInfo(string imageType)
    {
        var codec = ImageCodecInfo.GetImageEncoders();

        for (var i = 0; i < codec.Length; i++)
        {

            if (codec[i].MimeType == imageType) 
                return codec[i];
        }

        return null;
    }

    private void SaveAsJpg(System.Drawing.Bitmap bmp, string filename)
    {
        var myImageCodecInfo = GetEncoderInfo("image/jpeg");
        if (myImageCodecInfo == null)
        {
            MessageBox.Show("Cannot get codex for jpg", "Error");
            return;
        }
        ImageCodecInfo info = myImageCodecInfo;
        Encoder myEncoder = Encoder.Quality;
        EncoderParameters myEncoderParameters = new EncoderParameters(1);

        var myEncoderParameter = new EncoderParameter(myEncoder, 90L);
        myEncoderParameters.Param[0] = myEncoderParameter;

        bmp.Save(filename, info, myEncoderParameters);
    }

    private void SaveAsPng(System.Drawing.Bitmap bmp, string filename)
    {
        var myImageCodecInfo = GetEncoderInfo("image/png");
        if (myImageCodecInfo == null)
        {
            MessageBox.Show("Cannot get codex for jpg", "Error");
            return;
        }
        ImageCodecInfo info = myImageCodecInfo;
        var myEncoder = Encoder.Quality;
        var myEncoderParameters = new EncoderParameters(1);

        var myEncoderParameter = new EncoderParameter(myEncoder, 90L);
        myEncoderParameters.Param[0] = myEncoderParameter;

        bmp.Save(filename, info, myEncoderParameters);
    }

    protected void OnOpen()
    {
        if (_fractalResult != null)
        {
            var reply = MessageBox.Show("Opening file will result in loss of current images", "Warning",
                MessageBoxButton.OKCancel);

            if (reply == MessageBoxResult.Cancel)
                return;
        }
        
        var openFileDialog = new OpenFileDialog
        {
            Filter = "Fractal3D file (*.f3d)|*.f3d|All files (*.*)|*.*"
        };

        if (openFileDialog.ShowDialog() != true) return;
        try
        {
            string jsonString = File.ReadAllText(openFileDialog.FileName);

            var results = JsonConvert.DeserializeObject<List<FractalResult>>(jsonString);

            if (results == null) return;

            FractalResults.Clear();
            _fractalNumber = 1;
            foreach (var fractalResult in results)
            {
                FractalResults.Add(new FractalResultVm(fractalResult, _fractalNumber++));
            }
 
            _fractalResult = results.Last();   

            if(_fractalResult.Params == null)
            {
                MessageBox.Show("Can't load", "Params came back null");
                return;
            }
            _fractalParams = _fractalResult.Params;
            MakePaletteViewModel();
            ParameterViewModel = new ParameterVm(_fractalParams, OnParamsChanged);
            Width = _fractalParams.DisplaySize.Width;
            Height = _fractalParams.DisplaySize.Height;

            if (_fractalResult.Image == null)
                return;

            var bmp = _fractalResult.Image.GetBitmap(_fractalParams.Palette, _fractalParams.ColorInfo, _fractalParams.Light.AmbientPower);

            Image = ImageUtil.BitmapToImageSource(bmp);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Cannot load result from file");
        }
    }

    protected void OnDelete()
    {
        if (_selectedFractalResult != null)
        {
            ObservableCollection<FractalResultVm> newResults = new();

            foreach (var r in FractalResults)
            {
                if (r != _selectedFractalResult)
                {
                    newResults.Add(r);
                }
            }
            FractalResults = newResults;
            _selectedFractalResult = null;
        }
    }

    protected void OnCancel()
    {
        _cancelSource.Cancel();
    }

    private ObservableCollection<FractalResultVm> _fractalResults = new();
    public ObservableCollection<FractalResultVm> FractalResults
    {
        get => _fractalResults;
        set => SetProperty(ref _fractalResults, value);
    }

    protected void OnDisplayInfoChanged(DisplayInfo displayInfo)
    {
        _fractalParams.ColorInfo = displayInfo;
    }

    private void MakePaletteViewModel()
    {
        PaletteViewModel = new PaletteVm(_fractalParams.Palette, OnPaletteChanged, OnDisplayInfoChanged, _fractalParams.ColorInfo);
    }

    private FractalResultVm? _selectedFractalResult;
    public FractalResultVm? SelectedFractalResult
    {
        get => _selectedFractalResult;
        set
        {
            SetProperty(ref _selectedFractalResult, value);
            if (_selectedFractalResult != null)
            {
                _fractalResult = _selectedFractalResult.Result;
                _fractalParams = _fractalResult.Params != null ? (FractalParams) _fractalResult.Params.Clone(): new FractalParams();
                MakePaletteViewModel();
                ParameterViewModel = new ParameterVm(_fractalParams, OnParamsChanged);
                LightingViewModel = new LightingVm(_fractalParams, OnParamsChanged);
                DisplayImage(_fractalResult);
            }
                
        }
    }

    private bool AreFractalParametersValid()
    {
        if (_fractalParams.ImageSize.Width is < ParameterConstants.MinImage or > ParameterConstants.MaxImage)
            return false;

        if (_fractalParams.ImageSize.Height is < ParameterConstants.MinImage or > ParameterConstants.MaxImage)
            return false;

        if (_fractalParams.DisplaySize.Width is < ParameterConstants.MinImage or > ParameterConstants.MaxImage)
            return false;

        if (_fractalParams.DisplaySize.Height is < ParameterConstants.MinImage or > ParameterConstants.MaxImage)
            return false;

        if (_fractalParams.FromX is < ParameterConstants.MinFromTo or > ParameterConstants.MaxFromTo)
            return false;

        if (_fractalParams.ToX is < ParameterConstants.MinFromTo or > ParameterConstants.MaxFromTo)
            return false;

        if (_fractalParams.FromY is < ParameterConstants.MinFromTo or > ParameterConstants.MaxFromTo)
            return false;

        if (_fractalParams.ToY is < ParameterConstants.MinFromTo or > ParameterConstants.MaxFromTo)
            return false;

        if (_fractalParams.FromX > _fractalParams.ToX)
            return false;

        if (_fractalParams.FromY > _fractalParams.ToY)
            return false;

        if (_fractalParams.Bailout is < ParameterConstants.MinBailout or > ParameterConstants.MaxBailout)
            return false;

        if (_fractalParams.Iterations is < ParameterConstants.MinIterations or > ParameterConstants.MaxIterations)
            return false;

        if (_fractalParams.MaxRaySteps is < ParameterConstants.MinMaxRaySteps or > ParameterConstants.MaxMaxRaySteps)
            return false;

        if (_fractalParams.MinRayDistance is < ParameterConstants.MinMinRayDistance
            or > ParameterConstants.MaxMinRayDistance)
            return false;

        if (_fractalParams.Distance is < ParameterConstants.MinDistance or > ParameterConstants.MaxDistance)
            return false;

        if (_fractalParams.MaxDistance is < ParameterConstants.MinDistance or > ParameterConstants.MaxDistance)
            return false;

        if (_fractalParams.StepDivisor is < ParameterConstants.MinStepDivisor or > ParameterConstants.MaxStepDivisor)
            return false;

        return true;
    }

    private bool AreLightingParametersValid()
    {
        if (_fractalParams.NormalDistance is < ParameterConstants.MinNormalDistance or > ParameterConstants.MaxNormalDistance)
            return false;

        if (_fractalParams.Light.DiffuseColor.X is < ParameterConstants.MinFloatColor or > ParameterConstants.MaxFloatColor)
            return false;

        if (_fractalParams.Light.DiffuseColor.Y is < ParameterConstants.MinFloatColor or > ParameterConstants.MaxFloatColor)
            return false;

        if (_fractalParams.Light.DiffuseColor.Z is < ParameterConstants.MinFloatColor or > ParameterConstants.MaxFloatColor)
            return false;

        if (_fractalParams.Light.SpecularColor.X is < ParameterConstants.MinFloatColor or > ParameterConstants.MaxFloatColor)
            return false;

        if (_fractalParams.Light.SpecularColor.Y is < ParameterConstants.MinFloatColor or > ParameterConstants.MaxFloatColor)
            return false;

        if (_fractalParams.Light.SpecularColor.Z is < ParameterConstants.MinFloatColor or > ParameterConstants.MaxFloatColor)
            return false;

        if (_fractalParams.Light.LightColor.X is < ParameterConstants.MinFloatColor or > ParameterConstants.MaxFloatColor)
            return false;

        if (_fractalParams.Light.LightColor.Y is < ParameterConstants.MinFloatColor or > ParameterConstants.MaxFloatColor)
            return false;

        if (_fractalParams.Light.LightColor.Z is < ParameterConstants.MinFloatColor or > ParameterConstants.MaxFloatColor)
            return false;

        if (_fractalParams.Light.DiffusePower is < ParameterConstants.MinPower or > ParameterConstants.MaxPower)
            return false;

        if (_fractalParams.Light.SpecularPower is < ParameterConstants.MinPower or > ParameterConstants.MaxPower)
            return false;

        if (_fractalParams.Light.AmbientPower is < ParameterConstants.MinPower or > ParameterConstants.MaxPower)
            return false;

        if (_fractalParams.Light.Shininess is < ParameterConstants.MinShininess or > ParameterConstants.MaxShininess)
            return false;

        if (_fractalParams.Light.ScreenGamma is < ParameterConstants.MinScreenGamma or > ParameterConstants.MaxScreenGamma)
            return false;

        return true;
    }

    private bool AreParametersValid()
    {
        if (AreFractalParametersValid() == false)
            return false;

        if (AreLightingParametersValid() == false)
            return false;

        if (_fractalParams.Palette.NumberOfColors is < ParameterConstants.MinPaletteColors
            or > ParameterConstants.MaxPaletteColors)
            return false;

        return true;
    }
}

