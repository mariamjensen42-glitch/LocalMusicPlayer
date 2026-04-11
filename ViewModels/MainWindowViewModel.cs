using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalMusicPlayer.Models;
using LocalMusicPlayer.Services;

namespace LocalMusicPlayer.ViewModels;

public enum NavigationPage
{
    Home,
    Library,
    Favorites,
    Statistics,
    Settings,
    History
}

public partial class MainWindowViewModel : ViewModelBase, IPlaybackProgress
{
    private readonly IPlaybackStateService _playbackStateService;
    private readonly INavigationService _navigationService;
    private readonly IViewModelFactory _viewModelFactory;
    private readonly IPlaylistService _playlistService;
    private readonly IMusicLibraryService _musicLibraryService;
    private readonly IMusicPlayerService _musicPlayerService;
    private readonly IStatisticsService _statisticsService;
    private readonly IConfigurationService _configService;
    private readonly IUserPlaylistService _userPlaylistService;
    private readonly ISystemTrayService _systemTrayService;
    private readonly IPlayHistoryService _playHistoryService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPlayerPageVisible))]
    [NotifyPropertyChangedFor(nameof(SidebarWidth))]
    private ViewModelBase _currentPage = null!;

    public bool IsPlayerPageVisible => CurrentPage is PlayerPageViewModel;
    public int SidebarWidth => IsPlayerPageVisible ? 0 : 72;

    [ObservableProperty] private NavigationPage _currentNavPage = NavigationPage.Home;

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

    [ObservableProperty] private ObservableCollection<Song> _selectedSongs = new();
    [ObservableProperty] private bool _isSelectAll;

    partial void OnIsSelectAllChanged(bool value)
    {
        if (value)
        {
            SelectedSongs.Clear();
            foreach (var song in Library.FilteredSongs)
            {
                SelectedSongs.Add(song);
            }
        }
        else
        {
            SelectedSongs.Clear();
        }
    }

    [RelayCommand]
    private void ToggleSongSelection(Song song)
    {
        if (SelectedSongs.Contains(song))
        {
            SelectedSongs.Remove(song);
        }
        else
        {
            SelectedSongs.Add(song);
        }

        IsSelectAll = SelectedSongs.Count == Library.FilteredSongs.Count;
    }

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
        _playbackStateService.PlaybackMode = _playbackStateService.PlaybackMode switch
        {
            PlaybackMode.Normal => PlaybackMode.Loop,
            PlaybackMode.Loop => PlaybackMode.SingleLoop,
            _ => PlaybackMode.Normal
        };
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
            _playHistoryService.AddToHistory(song);
            _playbackStateService.Play(song);
        }
    }

    [RelayCommand]
    private async Task ToggleFavoriteAsync(string path)
    {
        var song = Library.Songs.FirstOrDefault(s => s.FilePath == path);
        if (song != null)
        {
            if (song.IsFavorite)
                await _userPlaylistService.RemoveFromFavoritesAsync(song);
            else
                await _userPlaylistService.AddToFavoritesAsync(song);
        }
    }

    [RelayCommand]
    private void NavigateToSettings()
    {
        CurrentNavPage = NavigationPage.Settings;
        SettingsViewModel = _viewModelFactory.CreateSettingsViewModel();
        CurrentPage = SettingsViewModel;
        _navigationService.NavigateTo<SettingsViewModel>();
    }

    [RelayCommand]
    private void NavigateToLibrary()
    {
        CurrentNavPage = NavigationPage.Home;
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
        CurrentNavPage = NavigationPage.Statistics;
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
    private void NavigateToFavorites()
    {
        CurrentNavPage = NavigationPage.Favorites;
        LibraryCategoryViewModel = _viewModelFactory.CreateLibraryCategoryViewModel();
        LibraryCategoryViewModel.OnNavigateToArtistDetail = NavigateToArtistDetail;
        LibraryCategoryViewModel.OnNavigateToAlbumDetail = NavigateToAlbumDetail;
        LibraryCategoryViewModel.ShowFavoritesOnly();
        CurrentPage = LibraryCategoryViewModel;
        _navigationService.NavigateTo<LibraryCategoryViewModel>();
    }

    [RelayCommand]
    private void NavigateToHistory()
    {
        CurrentNavPage = NavigationPage.History;
        PlayHistoryViewModel = _viewModelFactory.CreatePlayHistoryViewModel();
        PlayHistoryViewModel.OnNavigateBack = () => CurrentPage = this;
        CurrentPage = PlayHistoryViewModel;
        _navigationService.NavigateTo<PlayHistoryViewModel>();
    }

    [RelayCommand]
    private void NavigateToLibraryBrowser()
    {
        CurrentNavPage = NavigationPage.Library;
        LibraryBrowserViewModel = _viewModelFactory.CreateLibraryBrowserViewModel();
        CurrentPage = LibraryBrowserViewModel;
        _navigationService.NavigateTo<LibraryBrowserViewModel>();
    }

    [RelayCommand]
    private void NavigateToStatisticsReport()
    {
        CurrentNavPage = NavigationPage.Statistics;
        StatisticsReportViewModel = _viewModelFactory.CreateStatisticsReportViewModel();
        CurrentPage = StatisticsReportViewModel;
        _navigationService.NavigateTo<StatisticsReportViewModel>();
    }

    [RelayCommand]
    public void NavigateToArtistDetail(ArtistGroup artistGroup)
    {
        ArtistDetailViewModel = _viewModelFactory.CreateArtistDetailViewModel(artistGroup);
        ArtistDetailViewModel.OnNavigateBack = () => CurrentPage = LibraryCategoryViewModel!;
        CurrentPage = ArtistDetailViewModel;
        _navigationService.NavigateTo<ArtistDetailViewModel>();
    }

    public void NavigateToAlbumDetail(AlbumGroup albumGroup)
    {
        AlbumDetailViewModel = _viewModelFactory.CreateAlbumDetailViewModel(albumGroup);
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
                _playHistoryService.AddToHistory(song);
                _playbackStateService.Play(song);
            }
        }
    }

    [RelayCommand]
    private void PlaySelected()
    {
        if (SelectedSongs.Count == 0 || CurrentPlaylist == null)
            return;

        _playlistService.ClearPlaylist();
        foreach (var song in SelectedSongs)
        {
            _playlistService.AddSongToPlaylist(CurrentPlaylist, song);
        }

        if (_playlistService.PlayNext())
        {
            var song = _playlistService.CurrentSong;
            if (song != null)
            {
                _statisticsService.RecordPlayStart(song);
                _playHistoryService.AddToHistory(song);
                _playbackStateService.Play(song);
            }
        }
    }

    [RelayCommand]
    private void AddSelectedToPlaylist()
    {
        if (SelectedSongs.Count == 0 || CurrentPlaylist == null)
            return;

        foreach (var song in SelectedSongs)
        {
            _playlistService.AddSongToPlaylist(CurrentPlaylist, song);
        }
    }

    [RelayCommand]
    private void RemoveSelectedFromLibrary()
    {
        foreach (var song in SelectedSongs.ToList())
        {
            _musicLibraryService.RemoveSong(song);
        }

        SelectedSongs.Clear();
        IsSelectAll = false;
    }

    public PlayHistoryViewModel? PlayHistoryViewModel { get; private set; }

    public PlayerPageViewModel PlayerPageViewModel { get; }
    public QueueViewModel QueueViewModel { get; }
    public SettingsViewModel? SettingsViewModel { get; private set; }
    public PlaylistManagementViewModel? PlaylistManagementViewModel { get; private set; }
    public LibraryCategoryViewModel? LibraryCategoryViewModel { get; private set; }
    public StatisticsViewModel? StatisticsViewModel { get; private set; }
    public ArtistDetailViewModel? ArtistDetailViewModel { get; private set; }
    public AlbumDetailViewModel? AlbumDetailViewModel { get; private set; }
    public LibraryBrowserViewModel? LibraryBrowserViewModel { get; private set; }
    public StatisticsReportViewModel? StatisticsReportViewModel { get; private set; }

    public MainWindowViewModel(
        IPlaybackStateService playbackStateService,
        INavigationService navigationService,
        IViewModelFactory viewModelFactory,
        IPlaylistService playlistService,
        IMusicLibraryService musicLibraryService,
        IMusicPlayerService musicPlayerService,
        IStatisticsService statisticsService,
        IConfigurationService configService,
        IUserPlaylistService userPlaylistService,
        IScanService scanService,
        ISystemTrayService systemTrayService,
        IPlayHistoryService playHistoryService)
    {
        _playbackStateService = playbackStateService;
        _navigationService = navigationService;
        _viewModelFactory = viewModelFactory;
        _playlistService = playlistService;
        _musicLibraryService = musicLibraryService;
        _musicPlayerService = musicPlayerService;
        _statisticsService = statisticsService;
        _configService = configService;
        _userPlaylistService = userPlaylistService;
        _systemTrayService = systemTrayService;
        _playHistoryService = playHistoryService;

        CurrentPlaylist = _playlistService.CreatePlaylist("DefaultPlaylist");
        _playlistService.SetCurrentPlaylist(CurrentPlaylist);

        QueueViewModel = _viewModelFactory.CreateQueueViewModel();
        PlayerPageViewModel = _viewModelFactory.CreatePlayerPageViewModel();
        CurrentPage = this;
        _navigationService.NavigateTo<MainWindowViewModel>();

        _playbackStateService.PlaybackStateChanged += OnPlaybackStateChanged;
        _playbackStateService.CurrentSongChanged += OnCurrentSongChanged;
        _playbackStateService.PositionChanged += OnPositionChanged;

        _navigationService.QueuePanelChanged += OnQueuePanelChanged;
        _navigationService.CurrentPageChanged += OnCurrentPageChanged;

        Library.Songs.CollectionChanged += OnSongsCollectionChanged;

        _ = InitializeAsync(scanService).ContinueWith(_ => { }, TaskContinuationOptions.OnlyOnFaulted);
    }

    private void OnPlaybackStateChanged(object? sender, PlayState state)
    {
        OnPropertyChanged(nameof(IsPlaying));
        _systemTrayService.UpdateTrayIcon(IsPlaying);
    }

    private void OnCurrentSongChanged(object? sender, Song? song)
    {
        OnPropertyChanged(nameof(CurrentSong));
        if (CurrentSong != null)
        {
            _systemTrayService.ShowNotification("NowPlaying", $"{CurrentSong.Title} - {CurrentSong.Artist}");
        }
    }

    private void OnPositionChanged(object? sender, TimeSpan position)
    {
        OnPropertyChanged(nameof(Position));
        OnPropertyChanged(nameof(Duration));
        OnPropertyChanged(nameof(PositionSeconds));
        OnPropertyChanged(nameof(DurationSeconds));
        OnPropertyChanged(nameof(IsPlaying));
    }

    private void OnQueuePanelChanged(object? sender, bool isOpen)
    {
        OnPropertyChanged(nameof(IsQueuePanelOpen));
    }

    private void OnCurrentPageChanged(object? sender, Type? pageType)
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
        else if (pageType == typeof(PlayHistoryViewModel) && PlayHistoryViewModel != null)
            CurrentPage = PlayHistoryViewModel;
        else if (pageType == typeof(LibraryBrowserViewModel) && LibraryBrowserViewModel != null)
            CurrentPage = LibraryBrowserViewModel;
        else if (pageType == typeof(StatisticsReportViewModel) && StatisticsReportViewModel != null)
            CurrentPage = StatisticsReportViewModel;
    }

    private void OnSongsCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        UpdateLibraryStats();
        FilterSongs();
    }

    protected override void DisposeCore()
    {
        _playbackStateService.PlaybackStateChanged -= OnPlaybackStateChanged;
        _playbackStateService.CurrentSongChanged -= OnCurrentSongChanged;
        _playbackStateService.PositionChanged -= OnPositionChanged;
        _navigationService.QueuePanelChanged -= OnQueuePanelChanged;
        _navigationService.CurrentPageChanged -= OnCurrentPageChanged;
        Library.Songs.CollectionChanged -= OnSongsCollectionChanged;
        base.DisposeCore();
    }

    partial void OnSearchTextChanged(string value)
    {
        FilterSongs();
    }

    partial void OnVolumeChanged(int value)
    {
        _playbackStateService.SetVolume(value);
    }

    private async Task InitializeAsync(IScanService scanService)
    {
        await _configService.LoadSettingsAsync();
        Volume = _configService.CurrentSettings.Volume;
        IsMuted = _configService.CurrentSettings.IsMuted;
        _playbackStateService.SetVolume(Volume);
        if (IsMuted) _playbackStateService.Mute();
        _playbackStateService.SetPlaybackRate(_configService.CurrentSettings.PlaybackRate);

        var folders = _configService.GetScanFolders();
        if (folders.Count > 0)
        {
            if (_musicLibraryService.Songs.Count == 0)
                await scanService.ScanAllFoldersAsync();
            else
                await scanService.RescanLibraryAsync();
            UpdateLibraryStats();
        }

        await _userPlaylistService.LoadPlaylistsAsync();

        await RestorePlaybackStateAsync();
    }

    private Task RestorePlaybackStateAsync()
    {
        var settings = _configService.CurrentSettings;
        if (settings.QueueFilePaths.Count == 0 || string.IsNullOrEmpty(settings.LastSongFilePath))
            return Task.CompletedTask;

        var lastSong = _musicLibraryService.Songs.FirstOrDefault(s => s.FilePath == settings.LastSongFilePath);
        if (lastSong == null)
            return Task.CompletedTask;

        _playlistService.ClearPlaylist();
        foreach (var filePath in settings.QueueFilePaths)
        {
            var song = _musicLibraryService.Songs.FirstOrDefault(s => s.FilePath == filePath);
            if (song != null)
            {
                _playlistService.AddSongToPlaylist(CurrentPlaylist!, song);
            }
        }

        if (CurrentPlaylist!.Songs.Count > 0)
        {
            _playlistService.PlaySong(lastSong);
            _playbackStateService.Play(lastSong);

            if (settings.LastPlaybackPosition > 0)
            {
                _playbackStateService.Seek(TimeSpan.FromSeconds(settings.LastPlaybackPosition));
            }
        }

        return Task.CompletedTask;
    }

    private void FilterSongs()
    {
        Library.FilteredSongs.Clear();
        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? Library.Songs
            : Library.Songs.Where(s =>
                s.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                s.Artist.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                s.Album.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        var trackNumber = 1;
        foreach (var song in filtered)
        {
            song.TrackNumber = trackNumber++;
            Library.FilteredSongs.Add(song);
        }
    }
}
