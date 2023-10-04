namespace Fractal3d;

using BasicWpfLibrary;
using ImageCalculator;
using ImageCalculator.Movie;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

public class MovieVm : ViewModelBase
{
    private FractalParams _fractalParams;
    private readonly MovieParams _movieParams;
    private readonly Action<MovieParams> _onMovieParamsChanged;

    public MovieVm(FractalParams fractalParams, MovieParams movieParams, Action<MovieParams> onMovieParamsChanged)
    {
        _fractalParams = fractalParams;
        _movieParams = movieParams;
        _onMovieParamsChanged = onMovieParamsChanged;

        _playCommand = new RelayCommand(_ => OnPlay());
        _stopCommand = new RelayCommand(_ => OnStop());

        AllowedMovieTypes = new ObservableCollection<MovieTypes>
        {
            MovieTypes.Angles
        };
        SelectedMovieType = MovieTypes.Angles;
    }

    #region Commands

    private readonly RelayCommand _playCommand;
    public ICommand PlayCommand => _playCommand;

    private readonly RelayCommand _stopCommand;
    public ICommand StopCommand => _stopCommand;

    #endregion

    #region public methods

    public void SetFractalParams(FractalParams fractalParams)
    {
        _fractalParams = fractalParams;
        OnPropertyChanged(nameof(SelectedMovieType));
        OnPropertyChanged(nameof(CurrentImage));
        OnPropertyChanged(nameof(NumberOfImages));
    }

    #endregion

    #region handlers

    private void OnPlay()
    {}

    private void OnStop() { }

    #endregion

    #region properties

    public string Name => "Movie View Model";

    private ObservableCollection<MovieTypes> _allowedMovieTypes = null!;
    public ObservableCollection<MovieTypes> AllowedMovieTypes
    {
        get => _allowedMovieTypes;
        set => SetProperty(ref _allowedMovieTypes, value);
    }

    private MovieTypes _selectedMovieType;
    public MovieTypes SelectedMovieType
    {
        get => _selectedMovieType;
        set
        {
            _selectedMovieType = value;
            OnPropertyChanged();
        }
    }

    public int CurrentImage
    {
        get => _movieParams.CurrentImage;
        set
        {
            _movieParams.CurrentImage = value;
            OnPropertyChanged();
        }
    }

    public int NumberOfImages
    {
        get => _movieParams.NumberOfImages;
        set
        {
            _movieParams.NumberOfImages = value;
            OnPropertyChanged();
        }
    }

    public float FromAngleX
    {
        get => _movieParams.FromAngleX;
        set
        {
            _movieParams.FromAngleX = value;
            OnPropertyChanged();
        }
    }

    public float ToAngleX
    {
        get => _movieParams.ToAngleX;
        set
        {
            _movieParams.ToAngleX = value;
            OnPropertyChanged();
        }
    }

    public float FromAngleY
    {
        get => _movieParams.FromAngleY;
        set
        {
            _movieParams.FromAngleY = value;
            OnPropertyChanged();
        }
    }

    public float ToAngleY
    {
        get => _movieParams.ToAngleY;
        set
        {
            _movieParams.ToAngleY = value;
            OnPropertyChanged();
        }
    }

    public float FromAngleZ
    {
        get => _movieParams.FromAngleZ;
        set
        {
            _movieParams.FromAngleZ = value;
            OnPropertyChanged();
        }
    }

    public float ToAngleZ
    {
        get => _movieParams.ToAngleZ;
        set
        {
            _movieParams.ToAngleZ = value;
            OnPropertyChanged();
        }
    }

    #endregion
}

