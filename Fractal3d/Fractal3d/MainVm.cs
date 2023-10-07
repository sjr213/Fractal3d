﻿using ImageCalculator.Movie;

namespace Fractal3d;

using BasicWpfLibrary;
using FractureCommonLib;
using ImageCalculator;
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
using System.Windows.Forms;
using System.ComponentModel;
using System.Threading.Tasks;

internal class FractalRange
{
    public double FromX { get; set; }
    public double ToX { get; set; }
    public double FromY { get; set; }
    public double ToY { get; set; }
}

public class MainVm : ViewModelBase, IDisposable
{
    #region Members

    private const int NumberOfColors = 1000;

    private readonly ShaderFactory _shaderFactory = new();
    private readonly ParallelFractalFactory _fractalParallelFactory = new();
    private FractalParams _fractalParams = new(FractalParams.MakeLights()) { Palette = PaletteFactory.CreateStandardPalette(NumberOfColors) };
    private FractalResult? _fractalResult;
    private bool _isDisposed;
    private readonly IDisposable? _progressSubject;
    private readonly IDisposable? _progressShaderSubject;
    private CancellationTokenSource _cancelSource = new();
    private int _fractalNumber = 1;
    private bool _isDirty;
    private Rect _selectionRect;
    private FractalRange? _fractalRange;
    private MovieResult? _movieResult;

    private MovieParams _movieParams = new MovieParams();

    #endregion


#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public MainVm(string fileName)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        _calculateCommand = new RelayCommand(_ => Calculate(), _ => CanCalculate());
        _saveAllCommand = new RelayCommand(_ => OnSaveAll(), _ => _fractalResult != null);
        _saveOneCommand = new RelayCommand(_ => OnSaveOne(), _ => _fractalResult != null);
        _saveImageCommand = new RelayCommand(_ => OnSaveImage(), _ => _fractalResult != null);
        _deleteCommand = new RelayCommand(_ => OnDelete(), _ => _fractalResult != null);
        _deleteMostCommand = new RelayCommand(_ => OnDeleteMost(), _ => CanDeleteMost());
        _openCommand = new RelayCommand(_ => OnOpen(), _ => true);
        _cancelCommand = new RelayCommand(_ => OnCancel(), _ => true);  // This should only be visible when calculating because it's in the progress Stack panel
        _applyRectCommand = new RelayCommand(_ => OnApplyRect());
        _defaultParametersCommand = new RelayCommand(_ => OnDefaultParameters());
        _addToQueueCommand = new RelayCommand(_ => OnAddToQueue());
        MakePaletteViewModel();
        ImageViewModel = new ImageVm(_fractalParams, new BitmapImage(), SetSelectionRect);
        ParameterViewModel = new ParameterVm(_fractalParams, OnParamsChanged);
        UpdateMovieParams();
        MovieViewModel = new MovieVm(_fractalParams, _movieParams, OnMovieParamsChanged);
        LightingViewModel = new LightingVm(_fractalParams, OnParamsChanged);
        TransformViewModel = new TransformVm(_fractalParams, OnParamsChanged);
        DisplayInfoViewModel = new DisplayInfoVm(_fractalParams.ColorInfo, OnDisplayInfoChanged);
        
        AllowedViewModes = new ObservableCollection<ViewModes>
        {
            ViewModes.Queue, ViewModes.Temp, ViewModes.Movie
        };
        SelectedViewMode = ViewModes.Queue;

        if (SynchronizationContext.Current != null)
        {
            _progressSubject = _fractalParallelFactory.Progress.ObserveOn(SynchronizationContext.Current)
                .Subscribe(progress => PercentProgress = progress);

            _progressShaderSubject = _shaderFactory.Progress.ObserveOn(SynchronizationContext.Current)
                .Subscribe(progress => PercentProgress = progress);
        }

        OpenFileFromStartUp(fileName);
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
            _progressShaderSubject?.Dispose();
            _shaderFactory.Dispose();
            _fractalParallelFactory.Dispose();
        }

        _isDisposed = true;
    }

    #region Commands

    
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

    private readonly RelayCommand _deleteMostCommand;
    public ICommand DeleteMostCommand => _deleteMostCommand;

    private readonly RelayCommand _openCommand;
    public ICommand OpenCommand => _openCommand;

    private readonly RelayCommand _cancelCommand;
    public ICommand CancelCommand => _cancelCommand;

    private readonly RelayCommand _applyRectCommand;
    public ICommand ApplyRectCommand => _applyRectCommand;

    private readonly RelayCommand _defaultParametersCommand;
    public ICommand DefaultParametersCommand => _defaultParametersCommand;

    private readonly RelayCommand _addToQueueCommand;
    public RelayCommand AddToQueueCommand => _addToQueueCommand;

    #endregion

    #region NonCommandProperties

    private PaletteVm _paletteVm;
    public PaletteVm PaletteViewModel
    {
        get => _paletteVm;
        set => SetProperty(ref _paletteVm, value);
    }

    private ImageVm _imageVm;
    public ImageVm ImageViewModel
    {
        get => _imageVm;
        set => SetProperty(ref _imageVm, value);
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

    private TransformVm _transformVm;
    public TransformVm TransformViewModel
    {
        get => _transformVm;
        set => SetProperty(ref _transformVm, value);
    }

    private DisplayInfoVm _displayInfoVm;
    public DisplayInfoVm DisplayInfoViewModel
    {
        get => _displayInfoVm;
        set => SetProperty(ref _displayInfoVm, value);
    }

    private MovieVm _movieVm;
    public MovieVm MovieViewModel
    {
        get => _movieVm;
        set => SetProperty(ref _movieVm, value);
    }

    private Visibility _progressVisibility = Visibility.Collapsed;
    public Visibility ProgressVisibility
    {
        get => _progressVisibility;
        set => SetProperty(ref _progressVisibility, value);
    }

    private double _percentProgress;
    public double PercentProgress
    {
        get => _percentProgress;
        set => SetProperty(ref _percentProgress, value);
    }

    private long _time;
    public long Time
    {
        get => _time;
        set => SetProperty(ref _time, value);
    }

    private string _fractalRangeText = "";
    public string FractalRange
    {
        get => _fractalRangeText;
        set => SetProperty(ref _fractalRangeText, value);
    }

    private ObservableCollection<FractalResultVm> _fractalResults = new();
    public ObservableCollection<FractalResultVm> FractalResults
    {
        get => _fractalResults;
        set => SetProperty(ref _fractalResults, value);
    }

    private Visibility _applyRectVisibility = Visibility.Collapsed;
    public Visibility ApplyRectVisibility
    {
        get => _applyRectVisibility;
        set => SetProperty(ref _applyRectVisibility, value);
    }

    public double ResultListHeight => 50 + _fractalParams.DisplaySize.Height;

    private ObservableCollection<ViewModes> _allowedViewModes;
    public ObservableCollection<ViewModes> AllowedViewModes
    {
        get => _allowedViewModes;
        set => SetProperty(ref _allowedViewModes, value);
    }

    private ViewModes _selectedViewMode;
    public ViewModes SelectedViewMode
    {
        get => _selectedViewMode;
        set
        {
            _selectedViewMode = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Handlers

    public void OnWindowClosing(object sender, CancelEventArgs e)
    {
        if (_isDirty)
        {
            DoYouWantToSaveResults();
        }
    }

    protected bool CanCalculate()
    {
        return AreParametersValid();
    }

    protected void OnPaletteChanged(Palette palette)
    {
        _fractalParams.Palette = palette;

        if (SelectedViewMode == ViewModes.Temp)
            Calculate();
    }

    protected void OnParamsChanged(FractalParams fractalParams)
    {
        _fractalParams = fractalParams;
        ClearFractalRange();
        OnPropertyChanged(nameof(ResultListHeight));
        ImageViewModel.SetFractalParams(_fractalParams);
        UpdateMovieParams();
        MovieViewModel = new MovieVm(_fractalParams, _movieParams, OnMovieParamsChanged);

        if (SelectedViewMode == ViewModes.Temp)
            Calculate();
    }

    protected void OnMovieParamsChanged(MovieParams movieParams)
    {
        _movieParams = movieParams;
    }

    protected void OnSaveAll()
    {
        if (_fractalResult == null)
            return;

        var saveFileDialog = new SaveFileDialog
        {
            Filter = string.Format("Fractal3D file (*{0})|*{0}", Fractal3dConstants.FileExtension)
        };

        if (saveFileDialog.ShowDialog() != DialogResult.OK) return;
        try
        {
            var results = FractalResults.Select(rvm => rvm.Result).ToList();
            string jsonString = JsonConvert.SerializeObject(results);

            //write string to file
            File.WriteAllText(saveFileDialog.FileName, jsonString);
            _isDirty = false;
        }
        catch (Exception ex)
        {
            System.Windows.Forms.MessageBox.Show(ex.Message, "Cannot save result to file");
        }
    }

    protected void OnSaveOne()
    {
        if (_fractalResult == null)
            return;

        var saveFileDialog = new SaveFileDialog
        {
            Filter = string.Format("Fractal3D file (*{0})|*{0}", Fractal3dConstants.FileExtension)
        };

        if (saveFileDialog.ShowDialog() != DialogResult.OK) return;
        try
        {
            List<FractalResult> results = new() { _fractalResult };
            string jsonString = JsonConvert.SerializeObject(results);

            //write string to file
            File.WriteAllText(saveFileDialog.FileName, jsonString);

            if (FractalResults.Count <= 1)
            {
                _isDirty = false;
            }
        }
        catch (Exception ex)
        {
            System.Windows.Forms.MessageBox.Show(ex.Message, "Cannot save result to file");
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

        if (saveFileDialog.ShowDialog() != DialogResult.OK)
            return;

        string filename = saveFileDialog.FileName;
        string ext = Path.GetExtension(filename);

        var bmp = _fractalResult.Image.GetBitmap(_fractalParams.Palette, _fractalParams.ColorInfo, _fractalParams.AmbientPower);

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

    protected void OnOpen()
    {
        if (_fractalResult != null)
        {
            DoYouWantToSaveResults();
        }

        var openFileDialog = new OpenFileDialog
        {
            Filter = string.Format("Fractal3D file (*{0})|*{0}|All files (*.*)|*.*", Fractal3dConstants.FileExtension)
        };

        if (openFileDialog.ShowDialog() != DialogResult.OK) return;

        OpenResultFile(openFileDialog.FileName);
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

        if (FractalResults.Count == 0)
        {
            _isDirty = false;
        }
    }

    protected void OnDeleteMost()
    {
        if (_selectedFractalResult != null)
        {
            ObservableCollection<FractalResultVm> newResults = new();

            foreach (var r in FractalResults)
            {
                if (r == _selectedFractalResult)
                {
                    newResults.Add(r);
                }
            }
            FractalResults = newResults;
            _selectedFractalResult = null;
        }

        if (FractalResults.Count == 0)
        {
            _isDirty = false;
        }
    }

    protected bool CanDeleteMost()
    {
        return _selectedFractalResult != null && FractalResults.Count > 1;
    }

    protected void OnCancel()
    {
        _cancelSource.Cancel();
    }

    protected void OnDisplayInfoChanged(DisplayInfo displayInfo)
    {
        _fractalParams.ColorInfo = displayInfo;

        if (SelectedViewMode == ViewModes.Temp)
            Calculate();
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
                ClearFractalRange();
                _fractalResult = _selectedFractalResult.Result;
                _fractalParams = _fractalResult.Params != null ? (FractalParams)_fractalResult.Params.Clone() : new FractalParams();
                PaletteViewModel.SetNewPalette(_fractalParams.Palette, _fractalParams.ColorInfo);
                ParameterViewModel = new ParameterVm(_fractalParams, OnParamsChanged);
                DisplayInfoViewModel = new DisplayInfoVm(_fractalParams.ColorInfo, OnDisplayInfoChanged);
                LightingViewModel = new LightingVm(_fractalParams, OnParamsChanged);
                TransformViewModel = new TransformVm(_fractalParams, OnParamsChanged);
                UpdateMovieParams();
                MovieViewModel = new MovieVm(_fractalParams, _movieParams, OnMovieParamsChanged);
                DisplayImage(_fractalResult);
            }

        }
    }

    private void OnApplyRect()
    {
        if (_selectionRect.Width == 0 || _selectionRect.Height == 0)
            return;

        if (_fractalRange == null)
            return;

        var width = _fractalRange.ToX - _fractalRange.FromX;
        var height = _fractalRange.ToY - _fractalRange.FromY;

        _fractalParams.FromX = (float)(_fractalRange.FromX + _selectionRect.X * width);
        _fractalParams.ToX = (float)(_fractalRange.FromX + (_selectionRect.X + _selectionRect.Width) * width);
        _fractalParams.FromY = (float)(_fractalRange.FromY + _selectionRect.Y * height);
        _fractalParams.ToY = (float)(_fractalRange.FromY + (_selectionRect.Y + _selectionRect.Height) * height);

        ImageViewModel.ClearRectangle();
        ApplyRectVisibility = Visibility.Collapsed;
    }

    private void OnDefaultParameters()
    {
        var fractalParams = new FractalParams(FractalParams.MakeLights()) { Palette = PaletteFactory.CreateStandardPalette(NumberOfColors) };

        ClearFractalRange();
        _fractalParams = fractalParams;
        _movieParams = new MovieParams();
        PaletteViewModel.SetNewPalette(_fractalParams.Palette, _fractalParams.ColorInfo);
        ParameterViewModel = new ParameterVm(_fractalParams, OnParamsChanged);
        DisplayInfoViewModel = new DisplayInfoVm(_fractalParams.ColorInfo, OnDisplayInfoChanged);
        LightingViewModel = new LightingVm(_fractalParams, OnParamsChanged);
        TransformViewModel = new TransformVm(_fractalParams, OnParamsChanged);
        UpdateMovieParams();
        MovieViewModel = new MovieVm(_fractalParams, _movieParams, OnMovieParamsChanged);
    }

    private void OnAddToQueue()
    {
        if (SelectedViewMode == ViewModes.Temp && _fractalResult != null)
            FractalResults.Add(new FractalResultVm((FractalResult)_fractalResult.Clone(), _fractalNumber++));
    }

    #endregion

    #region Methods

    protected async void Calculate()
    {
        if (SelectedViewMode == ViewModes.Movie)
        {
            _cancelSource = new();
            var cancelToken = _cancelSource.Token;
            ProgressVisibility = Visibility.Visible;
            ClearFractalRange();

            try
            {
                int nImages = _movieParams.NumberOfImages;
                double sumProgress = 100.0 / nImages;
                double startProgress = 0;

                _movieResult = new MovieResult
                {
                    Params = _movieParams
                };

                for (int i = 1; i < nImages; ++i)
                {
                    var fractalParams = FractalParamCalculator.CalculateFractalParams(_fractalParams, _movieParams, i);

                    FractalResult? fractalResult;
                    if (_fractalParams.PlainShader)
                        fractalResult = await _shaderFactory.CreateShaderAsync(fractalParams, startProgress, sumProgress, cancelToken);
                    else
                        fractalResult = await _fractalParallelFactory.CreateFractalAsync(fractalParams, startProgress, sumProgress, cancelToken);

                    startProgress += sumProgress;

                    _movieResult.Results.Add(fractalResult);
                    _fractalResult = fractalResult;

                    DisplayImage(fractalResult);
                    Time = fractalResult.Time;
                }

                _isDirty = true;

            }
            catch (Exception)
            {
                _fractalResult = null;
            }

            if (cancelToken.IsCancellationRequested)
            {
                ProgressVisibility = Visibility.Collapsed;
                return;
            }

            ProgressVisibility = Visibility.Collapsed;
        }
        else
        {
            _cancelSource = new();
            var cancelToken = _cancelSource.Token;
            ProgressVisibility = Visibility.Visible;
            ClearFractalRange();

            try
            {
                if (_fractalParams.PlainShader)
                    _fractalResult = await _shaderFactory.CreateShaderAsync(_fractalParams, 0, 100, cancelToken);
                else
                    _fractalResult = await _fractalParallelFactory.CreateFractalAsync(_fractalParams, 0, 100, cancelToken);
            }
            catch (Exception)
            {
                _fractalResult = null;
            }

            if (cancelToken.IsCancellationRequested)
            {
                ProgressVisibility = Visibility.Collapsed;
                return;
            }

            if (_fractalResult == null)
                return;

            DisplayImage(_fractalResult);
            Time = _fractalResult.Time;

            ProgressVisibility = Visibility.Collapsed;

            if (SelectedViewMode == ViewModes.Queue)
                FractalResults.Add(new FractalResultVm((FractalResult)_fractalResult.Clone(), _fractalNumber++));

            _isDirty = true;
        }
    }

    protected void DisplayImage(FractalResult result)
    {
        if (result.Image == null || result.Params == null)
            return;

        var bmp = result.Image.GetBitmap(_fractalParams.Palette, _fractalParams.ColorInfo, _fractalParams.AmbientPower);

        var image = ImageUtil.BitmapToImageSource(bmp);
        ImageViewModel = new ImageVm(_fractalParams, image, SetSelectionRect);
    }

    private ImageCodecInfo? GetEncoderInfo(string imageType)
    {
        var codec = ImageCodecInfo.GetImageEncoders();

        foreach (var t in codec)
        {
            if (t.MimeType == imageType) 
                return t;
        }

        return null;
    }

    private void SaveAsJpg(System.Drawing.Bitmap bmp, string filename)
    {
        var myImageCodecInfo = GetEncoderInfo("image/jpeg");
        if (myImageCodecInfo == null)
        {
            System.Windows.Forms.MessageBox.Show("Cannot get codex for jpg", "Error");
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
            System.Windows.Forms.MessageBox.Show("Cannot get codex for jpg", "Error");
            return;
        }
        ImageCodecInfo info = myImageCodecInfo;
        var myEncoder = Encoder.Quality;
        var myEncoderParameters = new EncoderParameters(1);

        var myEncoderParameter = new EncoderParameter(myEncoder, 90L);
        myEncoderParameters.Param[0] = myEncoderParameter;

        bmp.Save(filename, info, myEncoderParameters);
    }

    private void OpenFileFromStartUp(string filename)
    {
        if (string.IsNullOrEmpty(filename))
            return;

        var ext = Path.GetExtension(filename);
        if (string.IsNullOrEmpty(ext))
            return;

        if (ext == Fractal3dConstants.FileExtension)
            OpenResultFile(filename);
    }

    protected void OpenResultFile(string filename)
    {
        try
        {
            ClearFractalRange();
            string jsonString = File.ReadAllText(filename);

            var results = JsonConvert.DeserializeObject<List<FractalResult>>(jsonString);

            if (results == null) return;

            FractalResults.Clear();
            _fractalNumber = 1;
            foreach (var fractalResult in results)
            {
                FractalResults.Add(new FractalResultVm(fractalResult, _fractalNumber++));
            }

            _fractalResult = results.Last();
            _isDirty = false;

            if (_fractalResult.Params == null)
            {
                System.Windows.Forms.MessageBox.Show("Can't load", "Params came back null");
                return;
            }
            _fractalParams = _fractalResult.Params;
            PaletteViewModel.SetNewPalette(_fractalParams.Palette, _fractalParams.ColorInfo);
            ParameterViewModel = new ParameterVm(_fractalParams, OnParamsChanged);
            DisplayInfoViewModel = new DisplayInfoVm(_fractalParams.ColorInfo, OnDisplayInfoChanged);

            if (_fractalResult.Image == null)
                return;

            var bmp = _fractalResult.Image.GetBitmap(_fractalParams.Palette, _fractalParams.ColorInfo, _fractalParams.AmbientPower);

            var image = ImageUtil.BitmapToImageSource(bmp);
            ImageViewModel = new ImageVm(_fractalParams, image, SetSelectionRect);

            SelectedFractalResult = FractalResults.FirstOrDefault();
        }
        catch (Exception ex)
        {
            System.Windows.Forms.MessageBox.Show(ex.Message, "Cannot load result from file");
        }
    }

    private void MakePaletteViewModel()
    {
        PaletteViewModel = new PaletteVm(_fractalParams.Palette, OnPaletteChanged, _fractalParams.ColorInfo);
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

        if (_fractalParams.Lights.Count == 0)
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

    private void DoYouWantToSaveResults()
    {
        DialogResult result = System.Windows.Forms.MessageBox.Show("Do you want to save your results?", "Unsaved Changes", MessageBoxButtons.YesNo);
        if (result == DialogResult.Yes)
        {
            OnSaveAll();
        }
    }

    private void SetSelectionRect(Rect rect)
    {
        _selectionRect = rect;

        if (_fractalRange == null)
        {
            _fractalRange = new FractalRange()
            {
                FromX = _fractalParams.FromX,
                ToX = _fractalParams.ToX,
                FromY = _fractalParams.FromY,
                ToY = _fractalParams.ToY
            };
        }

        SetFractalRangeText();
        ApplyRectVisibility = Visibility.Visible;
    }

    private void SetFractalRangeText()
    {
        if (_fractalRange == null)
        {
            FractalRange = "";
            return;
        }

        var width = _fractalRange.ToX - _fractalRange.FromX;
        var height = _fractalRange.ToY - _fractalRange.FromY;

        var fromX = (float)(_fractalRange.FromX + _selectionRect.X * width);
        var toX = (float)(_fractalRange.FromX + (_selectionRect.X + _selectionRect.Width) * width);
        var fromY = (float)(_fractalRange.FromY + _selectionRect.Y * height);
        var toY = (float)(_fractalRange.FromY + (_selectionRect.Y + _selectionRect.Height) * height);

        FractalRange = $"From X: {fromX:0.####}, To X: {toX:0.####}, From Y: {fromY:0.####}, To Y: {toY:0.####}";
    }

    private void ClearFractalRange()
    {
        _fractalRange = null;
        FractalRange = "";
        ApplyRectVisibility = Visibility.Collapsed;
    }

    private void UpdateMovieParams()
    {
        _movieParams.FromAngleX = _movieParams.ToAngleX = _fractalParams.TransformParams.RotateX;
        _movieParams.FromAngleY = _movieParams.ToAngleY = _fractalParams.TransformParams.RotateY;
        _movieParams.FromAngleZ = _movieParams.ToAngleZ = _fractalParams.TransformParams.RotateZ;
    }

    #endregion

}

