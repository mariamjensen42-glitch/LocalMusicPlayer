using System;
using System.Linq;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalMusicPlayer.Models;
using LocalMusicPlayer.Services;

namespace LocalMusicPlayer.ViewModels;

public partial class MainWindowViewModel : ViewModelBase, IPlaybackProgress
{
    private readonly IPlaybackStateService _playbackStateService;
    private readonly INavigationService _navigationService;
    private readonly IViewModelFactory _viewModelFactory;
    private readonly IPlaylistService _playlistService;
    private readonly IMusicLibraryService _musicLibraryService;
    private readonly IStatisticsService _statisticsService;
    private readonly IConfigurationService _configService;
    private readonly IUserPlaylistService _userPlaylistService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPlayerPageVisible))]
    [NotifyPropertyChangedFor(nameof(SidebarWidth))]
    private ViewModelBase _currentPage = null!;

    public bool IsPlayerPageVisible => CurrentPage is PlayerPageViewModel;
    public int SidebarWidth => IsPlayerPageVisible ? 0 : 72;

    [ObservableProperty] private bool _isLibrarySelected = true;
    [ObservableProperty] private bool _isCategorySelected;
    [ObservableProperty] private bool _isStatisticsSelected;
    [ObservableProperty] private bool _isSettingsSelected;

    public IMusicLibraryService Library => _musicLibraryService;

    [ObservableProperty] private string _libraryStats = "0 songs · 0 albums · 0h 0m";

    private void UpdateLibraryStats()
    {
        var songCount = Library.Songs.Count;
        var albumCount = Library.Songs.Select(s => s.Album).Distinct().Count();
        var totalDuration = Library.Songs.Aggregate(TimeSpan.Zero, (acc, s) => acc + s.Duration);
        var hours = (int)totalDuration.TotalHours;
        var minutes = totalDuration.Minutes;
        LibraryStats = $"{songCount} songs · {albumCount} albums · {hours}h {minutes}m";
    }

    [ObservableProperty] private Playlist? _currentPlaylist;

    public Song? CurrentSong => _playbackStateService.CurrentSong;

    [ObservableProperty] private string _searchText = string.Empty;

    public TimeSpan Position => _playbackStateService.Position;
    public TimeSpan Duration => _playbackStateService.Duration;
    public double PositionSeconds => _playbackStateService.PositionSeconds;
    public double DurationSeconds => _playbackStateService.DurationSeconds;

    [ObservableProperty] private int _volume;

    public bool IsPlaying => _playbackStateService.IsPlaying;

    [ObservableProperty] private bool _isMuted;

    public bool IsQueuePanelOpen => _navigationService.IsQueuePanelOpen;

    [RelayCommand]
    private void Play() => _playbackStateService.Resume();

    [RelayCommand]
    private void Pause() => _playbackStateService.Pause();

    [RelayCommand]
    private void Stop() => _playbackStateService.Stop();

    [RelayCommand]
    private void Next() => _playbackStateService.PlayNext();

    [RelayCommand]
    private void Previous() => _playbackStateService.PlayPrevious();

    [RelayCommand]
    private void Mute()
    {
        _playbackStateService.Mute();
        IsMuted = _playbackStateService.IsMuted;
    }

    [RelayCommand]
    private void Shuffle()
    {
        _playbackStateService.PlaybackMode = _playbackStateService.PlaybackMode == PlaybackMode.Shuffle
            ? PlaybackMode.Normal
            : PlaybackMode.Shuffle;
    }

    [RelayCommand]
    private void Repeat()
    {
        _playbackStateService.PlaybackMode = _playbackStateService.PlaybackMode == PlaybackMode.Loop
            ? PlaybackMode.Normal
            : PlaybackMode.Loop;
    }

    [RelayCommand]
    private void PlaySong(string path)
    {
        var song = Library.FilteredSongs.FirstOrDefault(s => s.FilePath == path);
        if (song != null && CurrentPlaylist != null)
        {
            _playlistService.ClearPlaylist();
            _playlistService.AddSongToPlaylist(CurrentPlaylist, song);
            _playlistService.SetCurrentPlaylist(CurrentPlaylist);
            _playlistService.PlaySong(song);
            _statisticsService.RecordPlayStart(song);
            _playbackStateService.Play(song);
        }
    }

    [RelayCommand]
    private void ToggleFavorite(string path)
    {
        var song = Library.Songs.FirstOrDefault(s => s.FilePath == path);
        if (song != null)
        {
            if (song.IsFavorite)
                _userPlaylistService.RemoveFromFavorites(song);
            else
                _userPlaylistService.AddToFavorites(song);
        }
    }

    [RelayCommand]
    private void NavigateToSettings()
    {
        IsLibrarySelected = false;
        IsCategorySelected = false;
        IsStatisticsSelected = false;
        IsSettingsSelected = true;
        SettingsViewModel = _viewModelFactory.CreateSettingsViewModel();
        CurrentPage = SettingsViewModel;
        _navigationService.NavigateTo<SettingsViewModel>();
    }

    [RelayCommand]
    private void NavigateToLibrary()
    {
        IsLibrarySelected = true;
        IsCategorySelected = false;
        IsStatisticsSelected = false;
        IsSettingsSelected = false;
        CurrentPage = this;
        _navigationService.NavigateTo<MainWindowViewModel>();
    }

    [RelayCommand]
    private void NavigateToPlayer()
    {
        CurrentPage = PlayerPageViewModel;
        _navigationService.NavigateTo<PlayerPageViewModel>();
    }

    [RelayCommand]
    private void NavigateToStatistics()
    {
        IsLibrarySelected = false;
        IsCategorySelected = false;
        IsStatisticsSelected = true;
        IsSettingsSelected = false;
        StatisticsViewModel = _viewModelFactory.CreateStatisticsViewModel();
        CurrentPage = StatisticsViewModel;
        _navigationService.NavigateTo<StatisticsViewModel>();
    }

    [RelayCommand]
    private void NavigateToPlaylist()
    {
        PlaylistManagementViewModel = _viewModelFactory.CreatePlaylistManagementViewModel();
        CurrentPage = PlaylistManagementViewModel;
        _navigationService.NavigateTo<PlaylistManagementViewModel>();
    }

    [RelayCommand]
    private void NavigateToCategory()
    {
        IsLibrarySelected = false;
        IsCategorySelected = true;
        IsStatisticsSelected = false;
        IsSettingsSelected = false;
        LibraryCategoryViewModel = _viewModelFactory.CreateLibraryCategoryViewModel();
        LibraryCategoryViewModel.OnNavigateToArtistDetail = NavigateToArtistDetail;
        LibraryCategoryViewModel.OnNavigateToAlbumDetail = NavigateToAlbumDetail;
        CurrentPage = LibraryCategoryViewModel;
        _navigationService.NavigateTo<LibraryCategoryViewModel>();
    }

    public void NavigateToArtistDetail(ArtistGroup artistGroup)
    {
        ArtistDetailViewModel = new ArtistDetailViewModel(
            artistGroup,
            _musicLibraryService as IMusicPlayerService ?? throw new InvalidOperationException(),
            _playlistService,
            _statisticsService);
        ArtistDetailViewModel.OnNavigateBack = () => CurrentPage = LibraryCategoryViewModel!;
        CurrentPage = ArtistDetailViewModel;
        _navigationService.NavigateTo<ArtistDetailViewModel>();
    }

    public void NavigateToAlbumDetail(AlbumGroup albumGroup)
    {
        AlbumDetailViewModel = new AlbumDetailViewModel(
            albumGroup,
            _musicLibraryService as IMusicPlayerService ?? throw new InvalidOperationException(),
            _playlistService,
            _statisticsService);
        AlbumDetailViewModel.OnNavigateBack = () => CurrentPage = LibraryCategoryViewModel!;
        CurrentPage = AlbumDetailViewModel;
        _navigationService.NavigateTo<AlbumDetailViewModel>();
    }

    [RelayCommand]
    private void ToggleQueuePanel() => _navigationService.ToggleQueuePanel();

    [RelayCommand]
    private void PlayAll()
    {
        if (Library.FilteredSongs.Count == 0 || CurrentPlaylist == null)
            return;

        _playlistService.ClearPlaylist();
        foreach (var song in Library.FilteredSongs)
        {
            _playlistService.AddSongToPlaylist(CurrentPlaylist, song);
        }

        if (_playlistService.PlayNext())
        {
            var song = _playlistService.CurrentSong;
            if (song != null)
            {
                _statisticsService.RecordPlayStart(song);
                _playbackStateService.Play(song);
            }
        }
    }

    public PlayerPageViewModel PlayerPageViewModel { get; }
    public QueueViewModel QueueViewModel { get; }
    public SettingsViewModel? SettingsViewModel { get; private set; }
    public PlaylistManagementViewModel? PlaylistManagementViewModel { get; private set; }
    public LibraryCategoryViewModel? LibraryCategoryViewModel { get; private set; }
    public StatisticsViewModel? StatisticsViewModel { get; private set; }
    public ArtistDetailViewModel? ArtistDetailViewModel { get; private set; }
    public AlbumDetailViewModel? AlbumDetailViewModel { get; private set; }

    private readonly DispatcherTimer _positionTimer;

    public MainWindowViewModel(
        IPlaybackStateService playbackStateService,
        INavigationService navigationService,
        IViewModelFactory viewModelFactory,
        IPlaylistService playlistService,
        IMusicLibraryService musicLibraryService,
        IStatisticsService statisticsService,
        IConfigurationService configService,
        IUserPlaylistService userPlaylistService,
        IScanService scanService)
    {
        _playbackStateService = playbackStateService;
        _navigationService = navigationService;
        _viewModelFactory = viewModelFactory;
        _playlistService = playlistService;
        _musicLibraryService = musicLibraryService;
        _statisticsService = statisticsService;
        _configService = configService;
        _userPlaylistService = userPlaylistService;

        CurrentPlaylist = _playlistService.CreatePlaylist("默认播放列表");
        _playlistService.SetCurrentPlaylist(CurrentPlaylist);

        QueueViewModel = _viewModelFactory.CreateQueueViewModel();
        PlayerPageViewModel = _viewModelFactory.CreatePlayerPageViewModel();
        PlayerPageViewModel.OnNavigateBack = () => CurrentPage = this;
        CurrentPage = this;

        _playbackStateService.PlaybackStateChanged += (_, _) => OnPropertyChanged(nameof(IsPlaying));

        _playbackStateService.CurrentSongChanged += (_, _) => OnPropertyChanged(nameof(CurrentSong));

        _navigationService.QueuePanelChanged += (_, _) => OnPropertyChanged(nameof(IsQueuePanelOpen));

        _navigationService.CurrentPageChanged += (_, pageType) =>
        {
            if (pageType == typeof(MainWindowViewModel))
                CurrentPage = this;
            else if (pageType == typeof(PlayerPageViewModel))
                CurrentPage = PlayerPageViewModel;
            else if (pageType == typeof(SettingsViewModel) && SettingsViewModel != null)
                CurrentPage = SettingsViewModel;
            else if (pageType == typeof(StatisticsViewModel) && StatisticsViewModel != null)
                CurrentPage = StatisticsViewModel;
            else if (pageType == typeof(PlaylistManagementViewModel) && PlaylistManagementViewModel != null)
                CurrentPage = PlaylistManagementViewModel;
            else if (pageType == typeof(LibraryCategoryViewModel) && LibraryCategoryViewModel != null)
                CurrentPage = LibraryCategoryViewModel;
            else if (pageType == typeof(ArtistDetailViewModel) && ArtistDetailViewModel != null)
                CurrentPage = ArtistDetailViewModel;
            else if (pageType == typeof(AlbumDetailViewModel) && AlbumDetailViewModel != null)
                CurrentPage = AlbumDetailViewModel;
        };

        _positionTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(250)
        };
        _positionTimer.Tick += (_, _) =>
        {
            OnPropertyChanged(nameof(Position));
            OnPropertyChanged(nameof(Duration));
            OnPropertyChanged(nameof(PositionSeconds));
            OnPropertyChanged(nameof(DurationSeconds));
            OnPropertyChanged(nameof(IsPlaying));
        };
        _positionTimer.Start();

        Library.Songs.CollectionChanged += (_, _) => UpdateLibraryStats();

        InitializeAsync(scanService);
    }

    partial void OnSearchTextChanged(string value)
    {
        FilterSongs();
    }

    partial void OnVolumeChanged(int value)
    {
        _playbackStateService.SetVolume(value);
    }

    private async void InitializeAsync(IScanService scanService)
    {
        await _configService.LoadSettingsAsync();
        Volume = _configService.CurrentSettings.Volume;
        IsMuted = _configService.CurrentSettings.IsMuted;
        _playbackStateService.SetVolume(Volume);
        if (IsMuted) _playbackStateService.Mute();

        var folders = _configService.GetScanFolders();
        if (folders.Count > 0)
        {
            if (_musicLibraryService.Songs.Count == 0)
                await scanService.ScanAllFoldersAsync();
            else
                await scanService.RescanLibraryAsync();
            UpdateLibraryStats();
        }
    }

    private void FilterSongs()
    {
        Library.FilteredSongs.Clear();
        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? Library.Songs
            : Library.Songs.Where(s =>
                s.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                s.Artist.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        var trackNumber = 1;
        foreach (var song in filtered)
        {
            song.TrackNumber = trackNumber++;
            Library.FilteredSongs.Add(song);
        }
    }
}