using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using ReactiveUI;
using LocalMusicPlayer.Models;
using LocalMusicPlayer.Services;

namespace LocalMusicPlayer.ViewModels;

public class PlayerPageViewModel : ViewModelBase
{
    private readonly IMusicPlayerService _musicPlayerService;
    private readonly IPlaylistService _playlistService;
    private readonly IMusicLibraryService _musicLibraryService;
    private readonly ILyricsService _lyricsService;
    private readonly MainWindowViewModel _mainWindowViewModel;

    private Song? _currentSong;

    public Song? CurrentSong
    {
        get => _currentSong;
        set
        {
            this.RaiseAndSetIfChanged(ref _currentSong, value);
            LoadLyrics();
        }
    }

    private bool _isPlaying;

    public bool IsPlaying
    {
        get => _isPlaying;
        set => this.RaiseAndSetIfChanged(ref _isPlaying, value);
    }

    private TimeSpan _position;

    public TimeSpan Position
    {
        get => _position;
        set
        {
            this.RaiseAndSetIfChanged(ref _position, value);
            UpdateCurrentLyricIndex();
        }
    }

    private TimeSpan _duration;

    public TimeSpan Duration
    {
        get => _duration;
        set => this.RaiseAndSetIfChanged(ref _duration, value);
    }

    private double _positionSeconds;
    private bool _isUpdatingPosition;
    private bool _isSeeking;

    public double PositionSeconds
    {
        get => _positionSeconds;
        set
        {
            this.RaiseAndSetIfChanged(ref _positionSeconds, value);
            if (!_isUpdatingPosition)
            {
                _isSeeking = true;
            }
        }
    }

    private double _durationSeconds;

    public double DurationSeconds
    {
        get => _durationSeconds;
        set => this.RaiseAndSetIfChanged(ref _durationSeconds, value);
    }

    private int _volume;

    public int Volume
    {
        get => _volume;
        set
        {
            this.RaiseAndSetIfChanged(ref _volume, value);
            _musicPlayerService.SetVolume(value);
        }
    }

    private bool _isMuted;

    public bool IsMuted
    {
        get => _isMuted;
        set => this.RaiseAndSetIfChanged(ref _isMuted, value);
    }

    private bool _isShuffle;

    public bool IsShuffle
    {
        get => _isShuffle;
        set
        {
            this.RaiseAndSetIfChanged(ref _isShuffle, value);
            _playlistService.PlaybackMode = value ? PlaybackMode.Shuffle : PlaybackMode.Normal;
        }
    }

    private bool _isRepeat;

    public bool IsRepeat
    {
        get => _isRepeat;
        set
        {
            this.RaiseAndSetIfChanged(ref _isRepeat, value);
            _playlistService.PlaybackMode = value ? PlaybackMode.Loop : PlaybackMode.Normal;
        }
    }

    private ObservableCollection<LyricLine> _lyrics = new();

    public ObservableCollection<LyricLine> Lyrics
    {
        get => _lyrics;
        set => this.RaiseAndSetIfChanged(ref _lyrics, value);
    }

    private int _currentLyricIndex = -1;

    public int CurrentLyricIndex
    {
        get => _currentLyricIndex;
        set => this.RaiseAndSetIfChanged(ref _currentLyricIndex, value);
    }

    private bool _hasLyrics;

    public bool HasLyrics
    {
        get => _hasLyrics;
        set => this.RaiseAndSetIfChanged(ref _hasLyrics, value);
    }

    public ReactiveCommand<Unit, Unit> PlayCommand { get; }
    public ReactiveCommand<Unit, Unit> PauseCommand { get; }
    public ReactiveCommand<Unit, Unit> NextCommand { get; }
    public ReactiveCommand<Unit, Unit> PreviousCommand { get; }
    public ReactiveCommand<Unit, Unit> ShuffleCommand { get; }
    public ReactiveCommand<Unit, Unit> RepeatCommand { get; }
    public ReactiveCommand<Unit, Unit> ToggleFavoriteCommand { get; }
    public ReactiveCommand<Unit, Unit> NavigateBackCommand { get; }
    public ReactiveCommand<Unit, Unit> ToggleMuteCommand { get; }

    // Expose ToggleQueuePanelCommand from MainWindowViewModel
    public ReactiveCommand<Unit, Unit> ToggleQueuePanelCommand => _mainWindowViewModel.ToggleQueuePanelCommand;

    public PlayerPageViewModel(
        IMusicPlayerService musicPlayerService,
        IPlaylistService playlistService,
        IMusicLibraryService musicLibraryService,
        ILyricsService lyricsService,
        MainWindowViewModel mainWindowViewModel)
    {
        _musicPlayerService = musicPlayerService;
        _playlistService = playlistService;
        _musicLibraryService = musicLibraryService;
        _lyricsService = lyricsService;
        _mainWindowViewModel = mainWindowViewModel;

        // 初始化状态
        CurrentSong = _playlistService.CurrentSong;
        IsPlaying = _musicPlayerService.IsPlaying;
        Position = _musicPlayerService.Position;
        Duration = _musicPlayerService.Duration;
        PositionSeconds = Position.TotalSeconds;
        DurationSeconds = Duration.TotalSeconds;
        IsShuffle = _playlistService.PlaybackMode == PlaybackMode.Shuffle;
        IsRepeat = _playlistService.PlaybackMode == PlaybackMode.Loop;

        // 命令
        PlayCommand = ReactiveCommand.Create(() => _musicPlayerService.Resume());
        PauseCommand = ReactiveCommand.Create(() => _musicPlayerService.Pause());
        NextCommand = ReactiveCommand.Create(() =>
        {
            if (_playlistService.PlayNext())
            {
                CurrentSong = _playlistService.CurrentSong;
                if (CurrentSong != null)
                    _musicPlayerService.Play(CurrentSong);
            }
        });
        PreviousCommand = ReactiveCommand.Create(() =>
        {
            if (_playlistService.PlayPrevious())
            {
                CurrentSong = _playlistService.CurrentSong;
                if (CurrentSong != null)
                    _musicPlayerService.Play(CurrentSong);
            }
        });
        ShuffleCommand = ReactiveCommand.Create(() =>
        {
            IsShuffle = !IsShuffle;
            if (IsShuffle)
                IsRepeat = false;
        });
        RepeatCommand = ReactiveCommand.Create(() =>
        {
            IsRepeat = !IsRepeat;
            if (IsRepeat)
                IsShuffle = false;
        });
        ToggleFavoriteCommand = ReactiveCommand.Create(() =>
        {
            if (CurrentSong != null)
            {
                CurrentSong.IsFavorite = !CurrentSong.IsFavorite;
            }
        });
        NavigateBackCommand = ReactiveCommand.Create(() =>
        {
            _mainWindowViewModel.NavigateToLibraryCommand.Execute(System.Reactive.Unit.Default);
        });
        ToggleMuteCommand = ReactiveCommand.Create(() =>
        {
            _musicPlayerService.Mute();
            IsMuted = _musicPlayerService.IsMuted;
        });

        // 订阅播放器事件
        _musicPlayerService.PlaybackStateChanged += (_, state) => { IsPlaying = state == PlayState.Playing; };

        _playlistService.CurrentSongChanged += (_, song) =>
        {
            CurrentSong = song;
            LoadLyrics();
        };

        // 同步 MainWindowViewModel 的 CurrentSong 变化
        _mainWindowViewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MainWindowViewModel.CurrentSong))
            {
                if (_mainWindowViewModel.CurrentSong != CurrentSong)
                {
                    CurrentSong = _mainWindowViewModel.CurrentSong;
                    LoadLyrics();
                }
            }
        };

        // 定时更新位置
        Observable.Interval(TimeSpan.FromMilliseconds(500))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ =>
            {
                if (!_isSeeking)
                {
                    Position = _musicPlayerService.Position;
                    Duration = _musicPlayerService.Duration;
                    _isUpdatingPosition = true;
                    PositionSeconds = Position.TotalSeconds;
                    _isUpdatingPosition = false;
                    DurationSeconds = Duration.TotalSeconds;
                }
            });

        // 防抖处理进度条拖动
        this.WhenAnyValue(x => x.PositionSeconds)
            .Throttle(TimeSpan.FromMilliseconds(100))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(pos =>
            {
                if (_isSeeking)
                {
                    _musicPlayerService.Seek(TimeSpan.FromSeconds(pos));
                    _isSeeking = false;
                }
            });

        LoadLyrics();
    }

    private void LoadLyrics()
    {
        Lyrics.Clear();
        CurrentLyricIndex = -1;
        HasLyrics = false;

        if (CurrentSong != null && !string.IsNullOrEmpty(CurrentSong.FilePath))
        {
            var lyrics = _lyricsService.GetLyrics(CurrentSong.FilePath);
            foreach (var lyric in lyrics)
            {
                Lyrics.Add(lyric);
            }

            HasLyrics = Lyrics.Count > 0;
        }
    }

    private void UpdateCurrentLyricIndex()
    {
        if (Lyrics.Count > 0)
        {
            var newIndex = _lyricsService.GetCurrentLyricIndex(Lyrics.ToList(), Position);

            // 清除之前的活动状态
            if (CurrentLyricIndex >= 0 && CurrentLyricIndex < Lyrics.Count)
            {
                Lyrics[CurrentLyricIndex].IsActive = false;
            }

            // 设置新的活动状态
            if (newIndex >= 0 && newIndex < Lyrics.Count)
            {
                Lyrics[newIndex].IsActive = true;
            }

            CurrentLyricIndex = newIndex;
        }
    }
}
