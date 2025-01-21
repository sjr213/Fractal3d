namespace Fractal3d;

using BasicWpfLibrary;
using FractureCommonLib;
using ImageCalculator;
using ImageCalculator.Movie;
using Newtonsoft.Json;
using System.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;

internal class FractalRange
{
    public double FromX { get; init; }
    public double ToX { get; init; }
    public double FromY { get; init; }
    public double ToY { get; init; }
}

public sealed class MainVm : ViewModelBase, IDisposable, IMoviePlayer, IObserver<int>
{
    #region Members

    private const int NumberOfColors = 1000;

    private readonly ShaderFactory _shaderFactory = new();
    private readonly ParallelFractalFactory _fractalParallelFactory = new();
    private readonly ParallelCraneShaderFactory _craneShaderFactory = new();
    private readonly ParallelCranePixelShaderFactory _cranePixelFactory = new();
    private readonly ParallelCraneRayMarchFactory _craneRayMarchFactory = new();
    private FractalParams _fractalParams = new(FractalParams.MakeLights()) { Palette = PaletteFactory.CreateStandardPalette(NumberOfColors) };
    private FractalResult? _fractalResult;
    private bool _isDisposed;
    private readonly IDisposable? _progressSubject;
    private readonly IDisposable? _progressCraneShaderSubject;
    private readonly IDisposable? _progressShaderSubject;
    private readonly IDisposable? _progressCranePixelSubject;
    private readonly IDisposable? _progressCraneRaymarchSubject;
    private CancellationTokenSource _cancelSource = new();
    private int _fractalNumber = 1;
    private bool _isDirty;
    private Rect _selectionRect;
    private FractalRange? _fractalRange;
    private MovieResult? _movieResult;
    private List<BitmapImage> _movieImages = new();
    private List<Bitmap> _movieBitmaps = new();

    private MovieParams _movieParams = new();

    #endregion


#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public MainVm(string fileName)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        _calculateCommand = new RelayCommand(_ => Calculate(), _ => CanCalculate());
        _saveAllCommand = new RelayCommand(_ => OnSaveAll(), _ => CanSaveAll());
        _saveOneCommand = new RelayCommand(_ => OnSaveOne(), _ => _fractalResult != null);
        _saveImageCommand = new RelayCommand(_ => OnSaveImage(), _ => _fractalResult != null);
        _deleteCommand = new RelayCommand(_ => OnDelete(), _ => _fractalResult != null);
        _deleteMostCommand = new RelayCommand(_ => OnDeleteMost(), _ => CanDeleteMost());
        _openCommand = new AsyncCommand(OnOpen, () => true, OnOpenFileError);
        _cancelCommand = new RelayCommand(_ => OnCancel(), _ => true);  // This should only be visible when calculating because it's in the progress Stack panel
        _applyRectCommand = new RelayCommand(_ => OnApplyRect());
        _defaultParametersCommand = new RelayCommand(_ => OnDefaultParameters());
        AddToQueueCommand = new RelayCommand(_ => OnAddToQueue());
        MakePaletteViewModel();
        ImageViewModel = new ImageVm(_fractalParams, new BitmapImage(), SetSelectionRect);
        ParameterViewModel = new ParameterVm(_fractalParams, OnParamsChanged);
        UpdateMovieParams();
        MovieParamViewModel = new MovieParamVm(_movieParams, _fractalParams, this);
        AddMovieVm();
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

            _progressCraneShaderSubject = _craneShaderFactory.Progress.ObserveOn(SynchronizationContext.Current)
                .Subscribe(progress => PercentProgress = progress);

            _progressCranePixelSubject = _cranePixelFactory.Progress.ObserveOn(SynchronizationContext.Current)
                .Subscribe(progress => PercentProgress = progress);

            _progressShaderSubject = _shaderFactory.Progress.ObserveOn(SynchronizationContext.Current)
                .Subscribe(progress => PercentProgress = progress);

            _progressCraneRaymarchSubject = _craneRayMarchFactory.Progress.ObserveOn(SynchronizationContext.Current)
                .Subscribe(progress => PercentProgress = progress);
        }

        var fileCmd = new AsyncCommand(() => OpenFileFromStartUp(fileName), () => true, OnOpenFileError);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        fileCmd.ExecuteAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
    }

    public void Dispose()
    {
        Dispose(true);
    }

    private void AddMovieVm()
    {
        MovieViewModel = new MovieVm();
        _movieVmObserver = MovieViewModel.Subscribe(this);
    }

    private void Dispose(bool disposing)
    {
        if (_isDisposed)
            return;

        if (disposing)
        {
            _progressSubject?.Dispose();
            _progressCraneShaderSubject?.Dispose();
            _progressCranePixelSubject?.Dispose();
            _progressShaderSubject?.Dispose();
            _progressCraneRaymarchSubject?.Dispose();
            _shaderFactory.Dispose();
            _fractalParallelFactory.Dispose();
            _craneShaderFactory.Dispose();
            _cranePixelFactory.Dispose();
            _craneRayMarchFactory.Dispose();
            _movieVmObserver.Dispose();
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

    private readonly AsyncCommand _openCommand;
    public ICommand OpenCommand => _openCommand;

    private readonly RelayCommand _cancelCommand;
    public ICommand CancelCommand => _cancelCommand;

    private readonly RelayCommand _applyRectCommand;
    public ICommand ApplyRectCommand => _applyRectCommand;

    private readonly RelayCommand _defaultParametersCommand;
    public ICommand DefaultParametersCommand => _defaultParametersCommand;

    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public RelayCommand AddToQueueCommand { get; }

    #endregion

    #region NonCommandProperties

    private PaletteVm _paletteVm;
    public PaletteVm PaletteViewModel
    {
        get => _paletteVm;
        private set => SetProperty(ref _paletteVm, value);
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

    private MovieParamVm _movieParamVm;
    public MovieParamVm MovieParamViewModel
    {
        get => _movieParamVm;
        set => SetProperty(ref _movieParamVm, value);
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
        set
        {
            SetProperty(ref _percentProgress, value);
            ProgressString = Math.Round(value, 1).ToString(CultureInfo.InvariantCulture);
        }
    }

    private string _progressString = string.Empty;
    public string ProgressString
    {
        get => _progressString;
        set => SetProperty(ref _progressString, value);
    }

    private string _infoString = string.Empty;

    public string InfoString
    {
        get => _infoString;
        set => SetProperty(ref _infoString, value);
    }

    private long _time;
    public long Time
    {
        get => _time;
        set => SetProperty(ref _time, value);
    }

    private string _fileName = string.Empty;
    public string FileName
    {
        get => _fileName;
        set
        {
            var fileName = Path.GetFileName(value);
            SetProperty(ref _fileName, fileName);
        }
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
            if (_selectedViewMode == ViewModes.Movie)
            {
                ImageViewVisibility = Visibility.Collapsed;
                MovieViewVisibility = Visibility.Visible;
            }
            else
            {
                ShowMovie = false;
                StopMovie();
                ImageViewVisibility = Visibility.Visible;
                MovieViewVisibility = Visibility.Collapsed;
            }
            OnPropertyChanged();
            OnMovieChanged(new MovieChangedEventArgs() { ChangeType = MovieChangeType.ImageCountChange });
        }
    }

    private Visibility _imageViewVisibility = Visibility.Visible;
    public Visibility ImageViewVisibility
    {
        get => _imageViewVisibility;
        set
        {
            _imageViewVisibility = value;
            OnPropertyChanged();
        }
    }

    private Visibility _movieViewVisibility = Visibility.Collapsed;
    public Visibility MovieViewVisibility
    {
        get => _movieViewVisibility;
        set
        {
            _movieViewVisibility = value;
            OnPropertyChanged();
        }
    }

    private bool _showMovie;

    public bool ShowMovie
    {
        get => _showMovie;
        set
        {
            _showMovie = value;
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

    private bool CanCalculate()
    {
        if (SelectedViewMode == ViewModes.Movie)
            return AreParametersValid() && AreMovieParamsValid();

        return AreParametersValid();
    }

    private void OnPaletteChanged(Palette palette)
    {
        _fractalParams.Palette = palette;

        if (SelectedViewMode == ViewModes.Temp)
            Calculate();
    }

    private void OnParamsChanged(FractalParams fractalParams)
    {
        _fractalParams = fractalParams;
        ClearFractalRange();
        ImageViewModel.SetFractalParams(_fractalParams);
        if(LightingViewModel != null)
            LightingViewModel.SetFractalParams(_fractalParams);

        if (SelectedViewMode == ViewModes.Temp)
            Calculate();
    }

    private bool CanSaveAll()
    {
        if (SelectedViewMode == ViewModes.Movie)
        {
            if (_movieParams.MovieFileType == MovieFileTypes.Full)
            {
                return _movieImages.Count == _movieParams.NumberOfImages;
            }
            else if (_movieParams.MovieFileType == MovieFileTypes.MP4)
            {
                return _movieBitmaps.Count == _movieParams.NumberOfImages;
            }

            return false;
        }

        return _fractalResult != null;
    }

    private void OnSaveAll()
    {
        InfoString = "Saving...";
        if (SelectedViewMode == ViewModes.Movie)
        {
            SaveMovie();
        }
        else
        {
            SaveAll();
        }
        InfoString = string.Empty;
    }

    private void SaveAll()
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
            SetDirty(false);
            FileName = saveFileDialog.FileName;
        }
        catch (Exception ex)
        {
            System.Windows.Forms.MessageBox.Show(ex.Message, "Cannot save result to file");
        }
    }

    private void SaveMovie()
    {
        if (SelectedViewMode != ViewModes.Movie || _movieResult == null)
            return;

        if (_movieParams.MovieFileType == MovieFileTypes.Full)
        {
            SaveFullMovie();
            return;
        }

        if (_movieParams.MovieFileType == MovieFileTypes.MP4 && _movieBitmaps.Count > 0)
        {
            SaveMp4Movie();
            return;
        }
        
        System.Windows.Forms.MessageBox.Show("Cannot save movie: Unknown File type or missing images");
    }

    private void SaveFullMovie()
    {
        var saveFileDialog = new SaveFileDialog
        {
            Filter = string.Format("Fractal3D movie file (*{0})|*{0}", Fractal3dConstants.MovieFileExtension)
        };

        if (saveFileDialog.ShowDialog() != DialogResult.OK) return;
        
        try
        {
            var results = _movieResult; 
            string jsonString = JsonConvert.SerializeObject(results);

            //write string to file
            File.WriteAllText(saveFileDialog.FileName, jsonString);
            SetDirty(false);
            FileName = saveFileDialog.FileName;
        }
        catch (Exception ex)
        {
            System.Windows.Forms.MessageBox.Show(ex.Message, "Cannot save movie to file");
        }
    }

    private void SaveMp4Movie()
    {
        var saveFileDialog = new SaveFileDialog
        {
            Filter = string.Format("mp4 movie file (*{0})|*{0}", ".mp4")
        };

        if (saveFileDialog.ShowDialog() != DialogResult.OK) return;
        
        try
        {
            MovieUtil.CreateMovieMp4(saveFileDialog.FileName, _movieParams.MovieWidth, 
                _movieParams.MovieHeight, _movieParams.FramesPerSecond, _movieBitmaps);
            
            SetDirty(false);
            FileName = saveFileDialog.FileName;
        }
        catch (Exception ex)
        {
            System.Windows.Forms.MessageBox.Show(ex.Message, "Cannot save movie to file");
        }
    }

    private void OnSaveOne()
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
            InfoString = "Saving...";
            List<FractalResult> results = new() { _fractalResult };
            string jsonString = JsonConvert.SerializeObject(results);

            //write string to file
            File.WriteAllText(saveFileDialog.FileName, jsonString);

            if (FractalResults.Count <= 1)
            {
                SetDirty(false);
                FileName = saveFileDialog.FileName;
            }
   
        }
        catch (Exception ex)
        {
            System.Windows.Forms.MessageBox.Show(ex.Message, "Cannot save result to file");
        }
        finally
        {
            InfoString = string.Empty;
        }
    }

    private void OnSaveImage()
    {
        if (_fractalResult == null || _fractalResult.Image == null)
            return;

        var saveFileDialog = new SaveFileDialog
        {
            Filter = "Jpeg Files (*.jpg)|*.jpg|bitmap Files (*.bmp)|*.bmp|Png Files (*.png)|*.png"
        };

        if (saveFileDialog.ShowDialog() != DialogResult.OK)
            return;

        InfoString = "Saving...";

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

        InfoString = string.Empty;
    }

    private async Task OnOpen()
    {
        if (_fractalResult != null)
        {
            DoYouWantToSaveResults();
        }

        var openFileDialog = new OpenFileDialog
        {
            Filter = string.Format("Fractal3D file (*{0})|*{0}|Fractal3D movie file (*{1})|*{1}|All files (*.*)|*.*", 
                Fractal3dConstants.FileExtension, Fractal3dConstants.MovieFileExtension)
        };

        if (openFileDialog.ShowDialog() != DialogResult.OK) return;

        InfoString = "Loading...";

        string filename = openFileDialog.FileName;
        string ext = Path.GetExtension(filename);

        if(ext == Fractal3dConstants.MovieFileExtension)
            await OpenMovieFile(filename);
        else
            await OpenResultFile(openFileDialog.FileName);

        InfoString = string.Empty;
    }

    private void OnOpenFileError(Exception e)
    {
        System.Windows.Forms.MessageBox.Show("Cannot load file: {0}", e.Message);
    }

    private void OnDelete()
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
            SetDirty(false);
        }
        FileName = string.Empty;
    }

    private void OnDeleteMost()
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
            SetDirty(false);
        }
        FileName = string.Empty;
    }

    private bool CanDeleteMost()
    {
        return _selectedFractalResult != null && FractalResults.Count > 1;
    }

    private void OnCancel()
    {
        _cancelSource.Cancel();
    }

    private void OnDisplayInfoChanged(DisplayInfo displayInfo)
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
        MovieParamViewModel = new MovieParamVm(_movieParams, _fractalParams,this);
    }

    private void OnAddToQueue()
    {
        if (SelectedViewMode == ViewModes.Temp && _fractalResult != null)
            FractalResults.Add(new FractalResultVm((FractalResult)_fractalResult.Clone(), _fractalNumber++));
    }

    #endregion

    #region Methods

    private async void Calculate()
    {
        _movieBitmaps = new List<Bitmap>();
        
        if (SelectedViewMode == ViewModes.Movie)
        {
            _cancelSource = new();
            var cancelToken = _cancelSource.Token;
            ProgressVisibility = Visibility.Visible;
            ClearFractalRange();
            _movieImages = new List<BitmapImage>();
            MovieViewModel.SetImages(_movieImages, _fractalParams);

            try
            {
                int nImages = _movieParams.NumberOfImages;
                double sumProgress = 100.0 / nImages;
                double startProgress = 0;

                _movieResult = new MovieResult
                {
                    Params = _movieParams
                };

                for (int i = 1; i <= nImages; ++i)
                {
                    var fractalParams = MovieParamCalculator.CalculateMovieParams(_fractalParams, _movieParams, i);

                    FractalResult? fractalResult;
                    if (_fractalParams.ShaderType == ShaderType.ShapeShader)
                        fractalResult = await _shaderFactory.CreateShaderAsync(fractalParams, startProgress, sumProgress, cancelToken);
                    else if (_fractalParams.ShaderType == ShaderType.CraneShader)
                        fractalResult = await _craneShaderFactory.CreateFractalAsync(fractalParams, startProgress, sumProgress, cancelToken);
                    else if (_fractalParams.ShaderType == ShaderType.CranePixel)
                        fractalResult = await _cranePixelFactory.CreateFractalAsync(fractalParams, startProgress, sumProgress, cancelToken);
                    else if (_fractalParams.ShaderType == ShaderType.CraneRaymarch)
                        fractalResult = await _craneRayMarchFactory.CreateFractalAsync(fractalParams, startProgress, sumProgress, cancelToken);
                    else
                        fractalResult = await _fractalParallelFactory.CreateFractalAsync(fractalParams, startProgress, sumProgress, cancelToken);

                    startProgress += sumProgress;

                    _movieResult.Results.Add(fractalResult);
                    _fractalResult = fractalResult;

                    DisplayImage(fractalResult);
                    Time = fractalResult.Time;
                }

                OnMovieChanged(new MovieChangedEventArgs(){ ChangeType = MovieChangeType.ImageCountChange });
                MovieViewModel.UpdateCurrentImage(nImages-1);
                SetDirty(true);

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
                if (_fractalParams.ShaderType == ShaderType.ShapeShader)
                    _fractalResult = await _shaderFactory.CreateShaderAsync(_fractalParams, 0, 100, cancelToken);
                else if (_fractalParams.ShaderType == ShaderType.CraneShader)
                    _fractalResult = await _craneShaderFactory.CreateFractalAsync(_fractalParams, 0, 100, cancelToken);
                else if (_fractalParams.ShaderType == ShaderType.CranePixel)
                    _fractalResult = await _cranePixelFactory.CreateFractalAsync(_fractalParams, 0, 100, cancelToken);
                else if (_fractalParams.ShaderType == ShaderType.CraneRaymarch)
                    _fractalResult = await _craneRayMarchFactory.CreateFractalAsync(_fractalParams, 0, 100, cancelToken);
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

            SetDirty(true);
        }
    }

    private void DisplayImage(FractalResult result)
    {
        if (result.Image == null || result.Params == null)
            return;

        var bmp = result.Image.GetBitmap(_fractalParams.Palette, _fractalParams.ColorInfo, _fractalParams.AmbientPower);

        var image = ImageUtil.BitmapToImageSource(bmp);
        if (SelectedViewMode == ViewModes.Movie)
        {
            if (_movieParams.MovieFileType == MovieFileTypes.Full)
            {
                _movieImages.Add(image);
            }
            else if(_movieParams.MovieFileType == MovieFileTypes.MP4)
            {
                _movieBitmaps.Add(bmp);
            }
        }
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

    private void SaveAsJpg(Bitmap bmp, string filename)
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

    private void SaveAsPng(Bitmap bmp, string filename)
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

    private async Task OpenFileFromStartUp(string filename)
    {
        if (string.IsNullOrEmpty(filename))
            return;

        var ext = Path.GetExtension(filename);
        if (string.IsNullOrEmpty(ext))
            return;

        InfoString = "Loading...";

        if (ext == Fractal3dConstants.FileExtension)
            await OpenResultFile(filename);
        else if (ext == Fractal3dConstants.MovieFileExtension)
            await OpenMovieFile(filename);

        InfoString = string.Empty;
    }

    private static List<FractalResult>? ReadFractalResultsFromFile(string filename)
    {
        string jsonString = File.ReadAllText(filename);

        var results = JsonConvert.DeserializeObject<List<FractalResult>>(jsonString);

        return results;
    }

    private void UpdateUiWithFractalResults(List<FractalResult>? results)
    {
        if (results == null)
            throw new InvalidOperationException("Cannot load fractal result from file");

        ClearFractalRange();

        FractalResults.Clear();
        _fractalNumber = 1;
        foreach (var fractalResult in results)
        {
            FractalResults.Add(new FractalResultVm(fractalResult, _fractalNumber++));
        }

        _fractalResult = results.Last();

        _fractalParams = _fractalResult.Params ?? throw new InvalidOperationException("Fractal parameters was null");
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

    private async Task OpenResultFile(string filename)
    {
        var results = await Task.Run(() => ReadFractalResultsFromFile(filename));
        SetDirty(false);
        FileName = filename;
        UpdateUiWithFractalResults(results);
    }

    private static MovieResult ReadMovieResultFromFile(string filename)
    {
        string jsonString = File.ReadAllText(filename);

        var movieResult = JsonConvert.DeserializeObject<MovieResult>(jsonString);

        if (movieResult == null || movieResult.Results.Count == 0)
            throw new InvalidOperationException("Movie result was empty");

        return movieResult;
    }

    private static List<BitmapImage> LoadMovieImages(MovieResult movieResult)
    {
        var movieImages = new List<BitmapImage>();

        var nImages = movieResult.Results.Count;

        for (var i = 1; i <= nImages; ++i)
        {
            var fractalResult = movieResult.Results[i - 1];

            if (fractalResult.Image == null || fractalResult.Params == null)
                throw new InvalidOperationException("Movie fractal result was empty");

            var bmp = fractalResult.Image.GetBitmap(fractalResult.Params.Palette, fractalResult.Params.ColorInfo, fractalResult.Params.AmbientPower);

            var image = ImageUtil.BitmapToImageSource(bmp);
            image.Freeze();     // to avoid exception later: Must create DependencySource on same Thread as the DependencyObject.
            movieImages.Add(image);
        }

        return movieImages;

    }

    private void UpdateUiWithMovieResults(MovieResult? movieResult, List<BitmapImage>? images)
    {
        if (movieResult == null || images == null || movieResult.Params == null)
            throw new InvalidOperationException("Movie result was empty");

        ClearFractalRange();

        _movieResult = movieResult;
        _movieParams = movieResult.Params;
        _movieImages = images;

        FractalResults.Clear();
        _fractalNumber = 1;

        // just add the first result
        var result = movieResult.Results[0];
        FractalResults.Add(new FractalResultVm(result, _fractalNumber++));
        _fractalResult = result;
        SetDirty(false);

        _fractalParams = _fractalResult.Params ?? throw new InvalidOperationException("Movie fractal parameters were null");
        PaletteViewModel.SetNewPalette(_fractalParams.Palette, _fractalParams.ColorInfo);
        ParameterViewModel = new ParameterVm(_fractalParams, OnParamsChanged);
        DisplayInfoViewModel = new DisplayInfoVm(_fractalParams.ColorInfo, OnDisplayInfoChanged);

        SelectedViewMode = ViewModes.Queue;
        SelectedFractalResult = FractalResults.FirstOrDefault();
        SelectedViewMode = ViewModes.Movie;

        ImageViewModel = new ImageVm(_fractalParams, images.First(), SetSelectionRect);
        MovieParamViewModel = new MovieParamVm(_movieParams, _fractalParams,this);
        MovieViewModel.SetImages(_movieImages, _fractalParams);

        OnMovieChanged(new MovieChangedEventArgs() { ChangeType = MovieChangeType.ImageCountChange });
    }

    private async Task OpenMovieFile(string filename)
    {
        var movieResult = await Task.Run(() => ReadMovieResultFromFile(filename));

        var movieImages = await Task.Run(() => LoadMovieImages(movieResult));

        SetDirty(false);
        FileName = filename;
        UpdateUiWithMovieResults(movieResult, movieImages);
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

        if (_fractalParams.DistanceScale is < ParameterConstants.MinDistanceScale or > ParameterConstants.MaxDistanceScale)
            return false;

        if (_fractalParams.EscapeThreshold is < ParameterConstants.MinEscapeThreshold or > ParameterConstants.MaxEscapeThreshold)
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

    private bool AreMovieParamsValid()
    {
        if (_movieParams.MovieParameterType == MovieParameterTypes.Angles)
        {
            if (Math.Abs(_movieParams.FromAngleX - _movieParams.ToAngleX) > MovieConstants.MinAngleDifference)
                return true;

            if (Math.Abs(_movieParams.FromAngleY - _movieParams.ToAngleY) > MovieConstants.MinAngleDifference)
                return true;

            if (Math.Abs(_movieParams.FromAngleZ - _movieParams.ToAngleZ) > MovieConstants.MinAngleDifference)
                return true;

            if (_movieParams.LoopAngleX || _movieParams.LoopAngleY || _movieParams.LoopAngleZ)
                return true;
        }
        else if (_movieParams.MovieParameterType == MovieParameterTypes.Bailout)
        {
            if (Math.Abs(_movieParams.EndBailout - _movieParams.StartBailout) > MovieConstants.MinBailoutDifference)
                return true;
        }
        else if (_movieParams.MovieParameterType == MovieParameterTypes.ConstantC)
        {
            return AreMovieParamsConstCValid();
        }

        return false;
    }

    (bool pass, bool differenceDetected) CheckAlternateConstantC(float start, float end, int steps)
    {
        if (Math.Abs(start - end) > MovieConstants.MinConstantCDifference)
        {
            return steps is > ParameterConstants.MinConstantCStep and <= ParameterConstants.MaxConstantCStep ? (true, true) : (false, false);
        }

        return steps == ParameterConstants.MinConstantCStep ? (true, false) : (false, false);
    }

    private bool AreMovieParamsConstCValid()
    {
        var minDif = _movieParams.DistributionType == DistributionTypes.Exponential
            ? MovieConstants.MinConstantCExpoDifference
            : MovieConstants.MinConstantCDifference;

        if (_movieParams is { Alternate: true, DistributionType: DistributionTypes.Exponential })
            return false;

        if (_movieParams.Alternate)
        {
            var checkW = CheckAlternateConstantC(_movieParams.ConstantCStartW, _movieParams.ConstantCEndW, _movieParams.StepsW);
            if (checkW.pass == false) return false;
            var differenceInStartEnd = checkW.pass;

            var checkX = CheckAlternateConstantC(_movieParams.ConstantCStartX, _movieParams.ConstantCEndX, _movieParams.StepsX);
            if (checkX.pass == false) return false;
            differenceInStartEnd |= checkX.pass;

            var checkY = CheckAlternateConstantC(_movieParams.ConstantCStartY, _movieParams.ConstantCEndY, _movieParams.StepsY);
            if (checkY.pass == false) return false;
            differenceInStartEnd |= checkY.pass;

            var checkZ = CheckAlternateConstantC(_movieParams.ConstantCStartZ, _movieParams.ConstantCEndZ, _movieParams.StepsZ);
            if (checkZ.pass == false) return false;
            differenceInStartEnd |= checkZ.pass;

            return differenceInStartEnd;
        }

        if (Math.Abs(_movieParams.ConstantCStartW - _movieParams.ConstantCEndW) > minDif)
            return true;

        if (Math.Abs(_movieParams.ConstantCStartX - _movieParams.ConstantCEndX) > minDif)
            return true;

        if (Math.Abs(_movieParams.ConstantCStartY - _movieParams.ConstantCEndY) > minDif)
            return true;

        if (Math.Abs(_movieParams.ConstantCStartZ - _movieParams.ConstantCEndZ) > minDif)
            return true;

        return false;
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

    private void SetDirty(bool dirty)
    {
        _isDirty = dirty;
        if(dirty)
            FileName = string.Empty;
    }

    #endregion

    #region IMoviePlayer

    private bool _isPlaying;

    public void PlayMovie(int framesPerSecond)
    {
        if(!CanPlayMovie()) 
            return;

        ShowMovie = true;
        MovieViewModel.Start(framesPerSecond);
        _isPlaying = true;
    }

    public bool CanPlayMovie()
    {
        return SelectedViewMode == ViewModes.Movie && !_isPlaying && _movieImages.Count == _movieParams.NumberOfImages;
    }

    public void StopMovie()
    {
        MovieViewModel.Stop();
        _isPlaying = false;
    }

    public bool CanStopMovie()
    {
        return _isPlaying;
    }

    public void OnMovieParamsChanged(MovieParams movieParams)
    {
        _movieParams = movieParams;
    }

    public event EventHandler<MovieChangedEventArgs>? MovieChanged;

    public bool IsMovie()
    {
        return _movieImages.Count == _movieParams.NumberOfImages && SelectedViewMode == ViewModes.Movie;
    }

    private void OnMovieChanged(MovieChangedEventArgs e)
    {
        MovieChanged?.Invoke(this, e);
    }

    public void UpdateCurrentImage(int currentImageIndex)
    {
        MovieViewModel.UpdateCurrentImage(currentImageIndex);
    }

    public bool CanUpdateCurrentImage()
    {
        return SelectedViewMode == ViewModes.Movie && !_isPlaying && _movieImages.Count == _movieParams.NumberOfImages;
    }

    public void MoveImageToQueue(int imageIndex)
    {
        if (CanMoveImageToQueue() == false)
            return;

        if (_movieResult== null ||  imageIndex < 0 || imageIndex >= _movieParams.NumberOfImages)
            return;

        SelectedViewMode = ViewModes.Queue;

        _fractalResult = (FractalResult) _movieResult.Results[imageIndex].Clone();
        DisplayImage(_fractalResult);

        FractalResults.Add(new FractalResultVm((FractalResult)_fractalResult.Clone(), _fractalNumber++));

        SetDirty(true);
    }

    public bool CanMoveImageToQueue()
    {
        return SelectedViewMode == ViewModes.Movie && !_isPlaying && _movieImages.Count == _movieParams.NumberOfImages;
    }

    #endregion

    #region IObserver<int> current image index

    private IDisposable _movieVmObserver;

    public void OnCompleted()
    {
        // do nothing
    }

    public void OnError(Exception error)
    {
        // do nothing
    }

    public void OnNext(int value)
    {
        OnMovieChanged(new MovieChangedEventArgs() { ChangeType = MovieChangeType.CurrentImageChanged, CurrentImageIndex = value});
    }

    #endregion

}

