namespace Fractal3d;

using BasicWpfLibrary;
using ImageCalculator.Movie;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

public class MovieParamVm : ViewModelBase
{
    private readonly MovieParams _movieParams;
    private readonly IMoviePlayer _moviePlayer;

    private const float MinFloatDifference = 0.001f;

    public MovieParamVm(MovieParams movieParams, IMoviePlayer moviePlayer)
    {
        _movieParams = movieParams;
        _moviePlayer = moviePlayer;

        _playCommand = new RelayCommand(_ => OnPlay(), _ => CanPlay());
        _stopCommand = new RelayCommand(_ => OnStop(), _ => CanStop());
        _forwardCommand = new RelayCommand(_ => OnForward(), _ => CanForward());
        _reverseCommand = new RelayCommand(_ => OnReverse(), _ => CanReverse());
        _moveImageToQueueCommand = new RelayCommand(_ => OnMoveImageToQueue(), _ => CanMoveImageToQueue());

        AllowedMovieTypes = new ObservableCollection<MovieTypes>
        {
            MovieTypes.Angles, MovieTypes.Bailout, MovieTypes.ConstantC
        };
        SelectedMovieType = MovieTypes.Angles;

        AllowedDistributionTypes = new ObservableCollection<DistributionTypes>
        {
            DistributionTypes.Linear, DistributionTypes.Exponential
        };
        SelectedDistributionType = DistributionTypes.Linear;

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

    private readonly RelayCommand _moveImageToQueueCommand;
    public ICommand MoveImageToQueueCommand => _moveImageToQueueCommand;

    #endregion

    #region public methods

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

    private void OnMoveImageToQueue()
    {
        _moviePlayer.MoveImageToQueue(CurrentImage-1);
    }

    private bool CanMoveImageToQueue()
    {
        return _moviePlayer.CanMoveImageToQueue();
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

    public MovieTypes SelectedMovieType
    {
        get => _movieParams.MovieType;
        set
        {
            _movieParams.MovieType = value;
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
            if (Math.Abs(_movieParams.FromAngleX - value) < MinFloatDifference) return;
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
            if (Math.Abs(_movieParams.ToAngleX - value) < MinFloatDifference) return;
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
            if (Math.Abs(_movieParams.FromAngleY - value) < MinFloatDifference) return;
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
            if (Math.Abs(_movieParams.ToAngleY - value) < MinFloatDifference) return;
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
            if (Math.Abs(_movieParams.FromAngleZ - value) < MinFloatDifference) return;
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
            if (Math.Abs(_movieParams.ToAngleZ- value) < MinFloatDifference) return;
            _movieParams.ToAngleZ = value;
            OnPropertyChanged();
            OnMovieParamsChanged();
        }
    }

    private ObservableCollection<DistributionTypes> _allowedDistributionTypes = null!;
    public ObservableCollection<DistributionTypes> AllowedDistributionTypes
    {
        get => _allowedDistributionTypes;
        set => SetProperty(ref _allowedDistributionTypes, value);
    }

    public DistributionTypes SelectedDistributionType
    {
        get => _movieParams.DistributionType;
        set
        {
            _movieParams.DistributionType = value;
            OnPropertyChanged();
            OnMovieParamsChanged();
        }
    }

    public float StartBailout
    {
        get => _movieParams.StartBailout;
        set
        {
            if (Math.Abs(_movieParams.StartBailout - value) < MinFloatDifference) return;
            _movieParams.StartBailout = value;
            OnPropertyChanged();
            OnMovieParamsChanged();
        }
    }

    public float EndBailout
    {
        get => _movieParams.EndBailout;
        set
        {
            if (Math.Abs(_movieParams.EndBailout - value) < MinFloatDifference) return;
            _movieParams.EndBailout = value;
            OnPropertyChanged();
            OnMovieParamsChanged();
        }
    }

    public float StartCW
    {
        get => _movieParams.ConstantCStartW;
        set
        {
            if (Math.Abs(_movieParams.ConstantCStartW - value) < MinFloatDifference) return;
            _movieParams.ConstantCStartW = value;
            OnPropertyChanged();
            OnMovieParamsChanged();
        }
    }

    public float EndCW
    {
        get => _movieParams.ConstantCEndW;
        set
        {
            if (Math.Abs(_movieParams.ConstantCEndW - value) < MinFloatDifference) return;
            _movieParams.ConstantCEndW = value;
            OnPropertyChanged();
            OnMovieParamsChanged();
        }
    }

    public float StartCX
    {
        get => _movieParams.ConstantCStartX;
        set
        {
            if (Math.Abs(_movieParams.ConstantCStartX - value) < MinFloatDifference) return;
            _movieParams.ConstantCStartX = value;
            OnPropertyChanged();
            OnMovieParamsChanged();
        }
    }

    public float EndCX
    {
        get => _movieParams.ConstantCEndX;
        set
        {
            if (Math.Abs(_movieParams.ConstantCEndX - value) < MinFloatDifference) return;
            _movieParams.ConstantCEndX = value;
            OnPropertyChanged();
            OnMovieParamsChanged();
        }
    }

    public float StartCY
    {
        get => _movieParams.ConstantCStartY;
        set
        {
            if (Math.Abs(_movieParams.ConstantCStartY - value) < MinFloatDifference) return;
            _movieParams.ConstantCStartY = value;
            OnPropertyChanged();
            OnMovieParamsChanged();
        }
    }

    public float EndCY
    {
        get => _movieParams.ConstantCEndY;
        set
        {
            if (Math.Abs(_movieParams.ConstantCEndY - value) < MinFloatDifference) return;
            _movieParams.ConstantCEndY = value;
            OnPropertyChanged();
            OnMovieParamsChanged();
        }
    }

    public float StartCZ
    {
        get => _movieParams.ConstantCStartZ;
        set
        {
            if (Math.Abs(_movieParams.ConstantCStartZ - value) < MinFloatDifference) return;
            _movieParams.ConstantCStartZ = value;
            OnPropertyChanged();
            OnMovieParamsChanged();
        }
    }

    public float EndCZ
    {
        get => _movieParams.ConstantCEndZ;
        set
        {
            if (Math.Abs(_movieParams.ConstantCEndZ - value) < MinFloatDifference) return;
            _movieParams.ConstantCEndZ = value;
            OnPropertyChanged();
            OnMovieParamsChanged();
        }
    }

    #endregion
}

