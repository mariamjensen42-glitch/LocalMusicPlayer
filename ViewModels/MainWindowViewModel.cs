using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Threading;
using ReactiveUI;
using LocalMusicPlayer.Models;
using LocalMusicPlayer.Services;

namespace LocalMusicPlayer.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IMusicPlayerService _musicPlayerService;
    private readonly IPlaylistService _playlistService;
    private readonly IMusicLibraryService _musicLibraryService;
    private readonly IWindowProvider _windowProvider;
    private readonly IConfigurationService _configService;
    private readonly IScanService _scanService;
    private readonly ILyricsService _lyricsService;

    private ViewModelBase _currentPage = null!;

    public ViewModelBase CurrentPage
    {
        get => _currentPage;
        set
        {
            this.RaiseAndSetIfChanged(ref _currentPage, value);
            this.RaisePropertyChanged(nameof(IsPlayerPageVisible));
            this.RaisePropertyChanged(nameof(SidebarWidth));
        }
    }

    public bool IsPlayerPageVisible => CurrentPage is PlayerPageViewModel;
    public int SidebarWidth => IsPlayerPageVisible ? 0 : 72;

    private PlayerPageViewModel? _playerPageViewModel;

    public PlayerPageViewModel? PlayerPageViewModel
    {
        get => _playerPageViewModel;
        set => this.RaiseAndSetIfChanged(ref _playerPageViewModel, value);
    }

    public IMusicLibraryService Library => _musicLibraryService;

    private string _libraryStats = "0 songs · 0 albums · 0h 0m";

    public string LibraryStats
    {
        get => _libraryStats;
        set => this.RaiseAndSetIfChanged(ref _libraryStats, value);
    }

    private void UpdateLibraryStats()
    {
        var songCount = Library.Songs.Count;
        var albumCount = Library.Songs.Select(s => s.Album).Distinct().Count();
        var totalDuration = Library.Songs.Aggregate(TimeSpan.Zero, (acc, s) => acc + s.Duration);
        var hours = (int)totalDuration.TotalHours;
        var minutes = totalDuration.Minutes;
        LibraryStats = $"{songCount} songs · {albumCount} albums · {hours}h {minutes}m";
    }

    private Playlist? _currentPlaylist;

    public Playlist? CurrentPlaylist
    {
        get => _currentPlaylist;
        set => this.RaiseAndSetIfChanged(ref _currentPlaylist, value);
    }

    private Song? _currentSong;

    public Song? CurrentSong
    {
        get => _currentSong;
        set => this.RaiseAndSetIfChanged(ref _currentSong, value);
    }

    private string _searchText = string.Empty;

    public string SearchText
    {
        get => _searchText;
        set
        {
            this.RaiseAndSetIfChanged(ref _searchText, value);
            FilterSongs();
        }
    }

    private TimeSpan _position;

    public TimeSpan Position
    {
        get => _position;
        set => this.RaiseAndSetIfChanged(ref _position, value);
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

    private int _volume = 100;

    public int Volume
    {
        get => _volume;
        set
        {
            this.RaiseAndSetIfChanged(ref _volume, value);
            _musicPlayerService.SetVolume(value);
        }
    }

    private bool _isPlaying;

    public bool IsPlaying
    {
        get => _isPlaying;
        set => this.RaiseAndSetIfChanged(ref _isPlaying, value);
    }

    private bool _isMuted;

    public bool IsMuted
    {
        get => _isMuted;
        set => this.RaiseAndSetIfChanged(ref _isMuted, value);
    }

    private bool _isScanning;

    public bool IsScanning
    {
        get => _isScanning;
        set => this.RaiseAndSetIfChanged(ref _isScanning, value);
    }

    public ReactiveCommand<Unit, Unit> PlayCommand { get; }
    public ReactiveCommand<Unit, Unit> PauseCommand { get; }
    public ReactiveCommand<Unit, Unit> StopCommand { get; }
    public ReactiveCommand<Unit, Unit> NextCommand { get; }
    public ReactiveCommand<Unit, Unit> PreviousCommand { get; }
    public ReactiveCommand<Unit, Unit> MuteCommand { get; }
    public ReactiveCommand<Unit, Unit> ShuffleCommand { get; }
    public ReactiveCommand<Unit, Unit> RepeatCommand { get; }
    public ReactiveCommand<string, Unit> PlaySongCommand { get; }
    public ReactiveCommand<string, Unit> ToggleFavoriteCommand { get; }
    public ReactiveCommand<Unit, Unit> NavigateToSettingsCommand { get; }
    public ReactiveCommand<Unit, Unit> NavigateToLibraryCommand { get; }
    public ReactiveCommand<Unit, Unit> NavigateToPlayerCommand { get; }
    public ReactiveCommand<Unit, Unit> ToggleQueuePanelCommand { get; }
    public ReactiveCommand<Unit, Unit> PlayAllCommand { get; }

    private QueueViewModel? _queueViewModel;

    public QueueViewModel? QueueViewModel
    {
        get => _queueViewModel;
        set => this.RaiseAndSetIfChanged(ref _queueViewModel, value);
    }

    public bool IsQueuePanelOpen => QueueViewModel?.IsPanelOpen ?? false;

    public MainWindowViewModel(
        IMusicPlayerService musicPlayerService,
        IPlaylistService playlistService,
        IMusicLibraryService musicLibraryService,
        IWindowProvider windowProvider,
        IScanService scanService,
        IConfigurationService configService,
        ILyricsService lyricsService)
    {
        _musicPlayerService = musicPlayerService;
        _playlistService = playlistService;
        _musicLibraryService = musicLibraryService;
        _windowProvider = windowProvider;
        _scanService = scanService;
        _configService = configService;
        _lyricsService = lyricsService;

        // 加载配置并自动扫描
        InitializeAsync();

        var settingsViewModel = new SettingsViewModel(windowProvider, scanService, musicLibraryService, configService);
        CurrentPage = this;

        CurrentPlaylist = _playlistService.CreatePlaylist("默认播放列表");
        _playlistService.SetCurrentPlaylist(CurrentPlaylist);

        // 创建 PlayerPageViewModel
        PlayerPageViewModel = new PlayerPageViewModel(
            musicPlayerService,
            playlistService,
            musicLibraryService,
            lyricsService,
            this);

        // 创建 QueueViewModel
        QueueViewModel = new QueueViewModel(playlistService, musicPlayerService);
        QueueViewModel.WhenAnyValue(x => x.IsPanelOpen)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(IsQueuePanelOpen)));

        NavigateToSettingsCommand = ReactiveCommand.Create(() => { CurrentPage = settingsViewModel; });
        NavigateToLibraryCommand = ReactiveCommand.Create(() => { CurrentPage = this; });
        NavigateToPlayerCommand = ReactiveCommand.Create(() => { CurrentPage = PlayerPageViewModel!; });
        ToggleQueuePanelCommand = ReactiveCommand.Create(() =>
        {
            if (QueueViewModel != null)
                QueueViewModel.IsPanelOpen = !QueueViewModel.IsPanelOpen;
        });
        PlayAllCommand = ReactiveCommand.Create(() =>
        {
            if (Library.FilteredSongs.Count == 0 || CurrentPlaylist == null)
                return;

            // Clear current playlist and add all filtered songs
            _playlistService.ClearPlaylist();
            foreach (var song in Library.FilteredSongs)
            {
                _playlistService.AddSongToPlaylist(CurrentPlaylist, song);
            }

            // Start playing from the first song
            _playlistService.PlayNext();
            CurrentSong = _playlistService.CurrentSong;
            if (CurrentSong != null)
                _musicPlayerService.Play(CurrentSong);
        });

        PlayCommand = ReactiveCommand.Create(() => _musicPlayerService.Resume());
        PauseCommand = ReactiveCommand.Create(() => _musicPlayerService.Pause());
        StopCommand = ReactiveCommand.Create(() => _musicPlayerService.Stop());
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
        MuteCommand = ReactiveCommand.Create(() =>
        {
            _musicPlayerService.Mute();
            IsMuted = _musicPlayerService.IsMuted;
        });
        ShuffleCommand = ReactiveCommand.Create(() =>
        {
            _playlistService.PlaybackMode = _playlistService.PlaybackMode == PlaybackMode.Shuffle
                ? PlaybackMode.Normal
                : PlaybackMode.Shuffle;
        });
        RepeatCommand = ReactiveCommand.Create(() =>
        {
            _playlistService.PlaybackMode = _playlistService.PlaybackMode == PlaybackMode.Loop
                ? PlaybackMode.Normal
                : PlaybackMode.Loop;
        });
        PlaySongCommand = ReactiveCommand.Create<string>(path =>
        {
            var song = Library.FilteredSongs.FirstOrDefault(s => s.FilePath == path);
            if (song != null)
            {
                var index = Library.FilteredSongs.IndexOf(song);
                if (CurrentPlaylist != null)
                {
                    _playlistService.RemoveSongFromPlaylist(CurrentPlaylist, index);
                    _playlistService.AddSongToPlaylist(CurrentPlaylist, song);
                }

                CurrentSong = song;
                _musicPlayerService.Play(song);
                IsPlaying = true;
            }
        });
        ToggleFavoriteCommand = ReactiveCommand.Create<string>(path =>
        {
            var song = Library.Songs.FirstOrDefault(s => s.FilePath == path);
            if (song != null)
            {
                song.IsFavorite = !song.IsFavorite;
            }
        });

        _musicPlayerService.PlaybackEnded += (_, _) =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (_playlistService.PlayNext())
                {
                    CurrentSong = _playlistService.CurrentSong;
                    if (CurrentSong != null)
                        _musicPlayerService.Play(CurrentSong);
                }
            });
        };

        _musicPlayerService.PlaybackStateChanged += (_, state) => { IsPlaying = state == PlayState.Playing; };

        Observable.Interval(TimeSpan.FromMilliseconds(500))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ =>
            {
                if (!_isSeeking)
                {
                    Position = _musicPlayerService.Position;
                    Duration = _musicPlayerService.Duration;
                    _isUpdatingPosition = true;
                    PositionSeconds = _musicPlayerService.Position.TotalSeconds;
                    _isUpdatingPosition = false;
                    DurationSeconds = _musicPlayerService.Duration.TotalSeconds;
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

        // Subscribe to library changes to update stats
        Library.Songs.CollectionChanged += (_, _) => UpdateLibraryStats();
    }

    private void FilterSongs()
    {
        Library.FilteredSongs.Clear();
        var filtered = string.IsNullOrWhiteSpace(_searchText)
            ? Library.Songs
            : Library.Songs.Where(s =>
                s.Title.Contains(_searchText, StringComparison.OrdinalIgnoreCase) ||
                s.Artist.Contains(_searchText, StringComparison.OrdinalIgnoreCase));

        var trackNumber = 1;
        foreach (var song in filtered)
        {
            song.TrackNumber = trackNumber++;
            Library.FilteredSongs.Add(song);
        }
    }

    private async void InitializeAsync()
    {
        await _configService.LoadSettingsAsync();

        // 设置音量
        Volume = _configService.CurrentSettings.Volume;
        IsMuted = _configService.CurrentSettings.IsMuted;
        _musicPlayerService.SetVolume(Volume);
        if (IsMuted) _musicPlayerService.Mute();

        // 如果有保存的文件夹路径，自动扫描
        var folders = _configService.GetScanFolders();
        if (folders.Count > 0)
        {
            var firstFolder = folders[0];
            if (!string.IsNullOrEmpty(firstFolder))
            {
                await _scanService.ScanAsync(firstFolder, _configService.CurrentSettings.IncludeSubfolders);
                UpdateLibraryStats();
            }
        }
    }
}
