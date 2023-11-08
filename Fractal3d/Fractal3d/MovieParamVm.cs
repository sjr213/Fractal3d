namespace Fractal3d;

using BasicWpfLibrary;
using ImageCalculator;
using ImageCalculator.Movie;
using System.Collections.ObjectModel;
using System.Windows.Input;

public class MovieParamVm : ViewModelBase
{
    private FractalParams _fractalParams;       // remove this later
    private readonly MovieParams _movieParams;
    private readonly IMoviePlayer _moviePlayer;

    public MovieParamVm(FractalParams fractalParams, MovieParams movieParams, IMoviePlayer moviePlayer)
    {
        _fractalParams = fractalParams;
        _movieParams = movieParams;
        _moviePlayer = moviePlayer;

        _playCommand = new RelayCommand(_ => OnPlay(), _ => CanPlay());
        _stopCommand = new RelayCommand(_ => OnStop(), _ => CanStop());
        _forwardCommand = new RelayCommand(_ => OnForward(), _ => CanForward());
        _reverseCommand = new RelayCommand(_ => OnReverse(), _ => CanReverse());

        AllowedMovieTypes = new ObservableCollection<MovieTypes>
        {
            MovieTypes.Angles, MovieTypes.Bailout
        };
        SelectedMovieType = MovieTypes.Angles;

        moviePlayer.MovieChanged += OnMovieChanged;
    }

    #region Commands

    private readonly RelayCommand _playCommand;
    public ICommand PlayCommand => _playCommand;

    private readonly RelayCommand _stopCommand;
    public ICommand StopCommand => _stopCommand;

    private readonly RelayCommand _forwardCommand;
    public ICommand ForwardCommand => _forwardCommand;

    private readonly RelayCommand _reverseCommand;
    public ICommand ReverseCommand => _reverseCommand;

    #endregion

    #region public methods

    public void SetFractalParams(FractalParams fractalParams)
    {
        _fractalParams = fractalParams;
        OnPropertyChanged(nameof(SelectedMovieType));
        OnPropertyChanged(nameof(CurrentImage));
        OnPropertyChanged(nameof(NumberOfImages));
    }

    private void SetMovieIndex(int movieIndex)
    {
        if (_moviePlayer.CanUpdateCurrentImage() == false)
            return;

        _moviePlayer.UpdateCurrentImage(movieIndex);
    }

    #endregion

    #region handlers

    private void OnPlay()
    {
        _moviePlayer.PlayMovie(_movieParams.FramesPerSecond);
    }

    private bool CanPlay()
    {
        return _moviePlayer.CanPlayMovie();
    }

    private void OnStop()
    {
        _moviePlayer.StopMovie();
    }

    private bool CanStop()
    {
        return _moviePlayer.CanStopMovie();
    }

    private void OnForward()
    {
        var index = CurrentImage; // This is index +1
        if (index >= _movieParams.NumberOfImages)
            index = 0;

        SetMovieIndex(index);
    }

    private bool CanForward()
    {
        return _moviePlayer.CanUpdateCurrentImage();
    }

    private void OnReverse()
    {
        var index = CurrentImage - 2; 
        if (index < 0)
            index = _movieParams.NumberOfImages - 1;

        SetMovieIndex(index);
    }

    private bool CanReverse()
    {
        return _moviePlayer.CanUpdateCurrentImage();
    }

    private void OnMovieParamsChanged()
    {
        _moviePlayer.OnMovieParamsChanged(_movieParams);
    }

    private void OnMovieChanged(object? sender, MovieChangedEventArgs args)
    {
        if(args.ChangeType == MovieChangeType.ImageCountChange)
            OnPropertyChanged(nameof(IsMovie));

        if (args.ChangeType == MovieChangeType.CurrentImageChanged)
            CurrentImage = args.CurrentImageIndex + 1;
    }

    #endregion

    #region properties

    private ObservableCollection<MovieTypes> _allowedMovieTypes = null!;
    public ObservableCollection<MovieTypes> AllowedMovieTypes
    {
        get => _allowedMovieTypes;
        set => SetProperty(ref _allowedMovieTypes, value);
    }

    public bool IsMovie => _moviePlayer.IsMovie();

    private MovieTypes _selectedMovieType;
    public MovieTypes SelectedMovieType
    {
        get => _selectedMovieType;
        set
        {
            _selectedMovieType = value;
            OnPropertyChanged();
            OnMovieParamsChanged();
        }
    }

    public int CurrentImage
    {
        get => _movieParams.CurrentImage;
        set
        {
            _movieParams.CurrentImage = value;
            OnPropertyChanged();
            OnMovieParamsChanged();
            SetMovieIndex(_movieParams.CurrentImage - 1);
        }
    }

    public int NumberOfImages
    {
        get => _movieParams.NumberOfImages;
        set
        {
            _movieParams.NumberOfImages = value;
            OnPropertyChanged();
            OnMovieParamsChanged();
        }
    }

    public int FramesPerSecond
    {
        get => _movieParams.FramesPerSecond;
        set
        {
            _movieParams.FramesPerSecond = value; 
            OnPropertyChanged();
            OnMovieParamsChanged();
        }
    }

    public float FromAngleX
    {
        get => _movieParams.FromAngleX;
        set
        {
            _movieParams.FromAngleX = value;
            OnPropertyChanged();
            OnMovieParamsChanged();
        }
    }

    public float ToAngleX
    {
        get => _movieParams.ToAngleX;
        set
        {
            _movieParams.ToAngleX = value;
            OnPropertyChanged();
            OnMovieParamsChanged();
        }
    }

    public float FromAngleY
    {
        get => _movieParams.FromAngleY;
        set
        {
            _movieParams.FromAngleY = value;
            OnPropertyChanged();
            OnMovieParamsChanged();
        }
    }

    public float ToAngleY
    {
        get => _movieParams.ToAngleY;
        set
        {
            _movieParams.ToAngleY = value;
            OnPropertyChanged();
            OnMovieParamsChanged();
        }
    }

    public float FromAngleZ
    {
        get => _movieParams.FromAngleZ;
        set
        {
            _movieParams.FromAngleZ = value;
            OnPropertyChanged();
            OnMovieParamsChanged();
        }
    }

    public float ToAngleZ
    {
        get => _movieParams.ToAngleZ;
        set
        {
            _movieParams.ToAngleZ = value;
            OnPropertyChanged();
            OnMovieParamsChanged();
        }
    }

    #endregion
}

