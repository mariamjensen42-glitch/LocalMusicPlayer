using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalMusicPlayer.Models;
using LocalMusicPlayer.Services;

namespace LocalMusicPlayer.ViewModels;

public enum NavigationPage
{
    None,
    Home,
    Library,
    Artists,
    Albums,
    Favorites,
    Statistics,
    Settings,
    History,
    Playlist
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
    private readonly IDialogService _dialogService;
    private readonly ISmartPlaylistService _smartPlaylistService;

    [ObservableProperty] private ViewModelBase _currentPage = null!;

    [ObservableProperty] private bool _isPlayerOverlayOpen;

    public const int SidebarWidth = 220;

    [ObservableProperty] private NavigationPage _currentNavPage = NavigationPage.Home;

    [ObservableProperty] private BrowserCategory _currentLibraryCategory = BrowserCategory.Artists;

    public IMusicLibraryService Library => _musicLibraryService;

    [ObservableProperty] private string _libraryStats = "本地: 0 首";

    private void UpdateLibraryStats()
    {
        var songCount = Library.Songs.Count;
        LibraryStats = $"本地: {songCount} 首";
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

    public bool IsGridView => HomeViewModel.IsGridView;

    [RelayCommand]
    private void ToggleView()
    {
        HomeViewModel.ToggleViewCommand.Execute(null);
        OnPropertyChanged(nameof(IsGridView));
    }

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
    private void Seek(TimeSpan position)
    {
        _playbackStateService.Seek(position);
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
        CurrentPage = HomeViewModel;
        _navigationService.NavigateTo<HomeViewModel>();
    }

    [RelayCommand]
    private void NavigateToPlayer()
    {
        IsPlayerOverlayOpen = true;
    }

    [RelayCommand]
    private void ClosePlayerOverlay()
    {
        IsPlayerOverlayOpen = false;
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
        PlaylistListViewModel = _viewModelFactory.CreatePlaylistListViewModel();
        PlaylistListViewModel.OnPlaylistSelected = NavigateToPlaylistDetail;
        CurrentPage = PlaylistListViewModel;
        _navigationService.NavigateTo<PlaylistListViewModel>();
    }

    private void NavigateToPlaylistDetail(UserPlaylist playlist)
    {
        PlaylistManagementViewModel = _viewModelFactory.CreatePlaylistManagementViewModel();
        PlaylistManagementViewModel.SetSelectedPlaylist(playlist);
        CurrentPage = PlaylistManagementViewModel;
        _navigationService.NavigateTo<PlaylistManagementViewModel>();
    }

    [RelayCommand]
    private async Task CreatePlaylistAsync()
    {
        var name = await _dialogService.ShowInputDialogAsync("New Playlist", "");
        if (!string.IsNullOrWhiteSpace(name))
        {
            await _userPlaylistService.CreatePlaylistAsync(name);
        }
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
    private void NavigateToArtistsPage()
    {
        CurrentNavPage = NavigationPage.Artists;
        ArtistsPageViewModel = _viewModelFactory.CreateArtistsPageViewModel();
        ArtistsPageViewModel.OnNavigateToDetail = NavigateToArtistDetail;
        CurrentPage = ArtistsPageViewModel;
        _navigationService.NavigateTo<ArtistsPageViewModel>();
    }

    [RelayCommand]
    private void NavigateToAlbumsPage()
    {
        CurrentNavPage = NavigationPage.Albums;
        AlbumsPageViewModel = _viewModelFactory.CreateAlbumsPageViewModel();
        AlbumsPageViewModel.OnNavigateToDetail = NavigateToAlbumDetail;
        CurrentPage = AlbumsPageViewModel;
        _navigationService.NavigateTo<AlbumsPageViewModel>();
    }

    [RelayCommand]
    private void NavigateToLibraryBrowser(BrowserCategory? category = null)
    {
        CurrentNavPage = NavigationPage.Library;
        CurrentLibraryCategory = category ?? BrowserCategory.Artists;
        LibraryBrowserViewModel = _viewModelFactory.CreateLibraryBrowserViewModel(CurrentLibraryCategory);
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
        CurrentNavPage = NavigationPage.None;
        ArtistDetailViewModel = _viewModelFactory.CreateArtistDetailViewModel(artistGroup);
        ArtistDetailViewModel.OnNavigateBack = () => CurrentPage = LibraryCategoryViewModel!;
        CurrentPage = ArtistDetailViewModel;
        _navigationService.NavigateTo<ArtistDetailViewModel>();
    }

    public void NavigateToAlbumDetail(AlbumGroup albumGroup)
    {
        CurrentNavPage = NavigationPage.None;
        AlbumDetailViewModel = _viewModelFactory.CreateAlbumDetailViewModel(albumGroup);
        AlbumDetailViewModel.OnNavigateBack = () => CurrentPage = LibraryCategoryViewModel!;
        CurrentPage = AlbumDetailViewModel;
        _navigationService.NavigateTo<AlbumDetailViewModel>();
    }

    [RelayCommand]
    private void ToggleQueuePanel() => _navigationService.ToggleQueuePanel();

    [RelayCommand]
    private void GoBack() => NavigateToLibrary();

    [RelayCommand]
    private void ToggleFullScreen()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime
            {
                MainWindow: var window
            })
        {
            window!.WindowState = window.WindowState == WindowState.FullScreen
                ? WindowState.Normal
                : WindowState.FullScreen;
        }
    }

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
    private void ShufflePlayAll()
    {
        if (Library.FilteredSongs.Count == 0 || CurrentPlaylist == null)
            return;

        var shuffled = Library.FilteredSongs.OrderBy(_ => Guid.NewGuid()).ToList();

        _playlistService.ClearPlaylist();
        foreach (var song in shuffled)
        {
            _playlistService.AddSongToPlaylist(CurrentPlaylist, song);
        }

        _playbackStateService.PlaybackMode = PlaybackMode.Shuffle;

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
    public PlaylistListViewModel? PlaylistListViewModel { get; private set; }
    public LibraryCategoryViewModel? LibraryCategoryViewModel { get; private set; }
    public StatisticsViewModel? StatisticsViewModel { get; private set; }
    public ArtistDetailViewModel? ArtistDetailViewModel { get; private set; }
    public AlbumDetailViewModel? AlbumDetailViewModel { get; private set; }
    public LibraryBrowserViewModel? LibraryBrowserViewModel { get; private set; }
    public StatisticsReportViewModel? StatisticsReportViewModel { get; private set; }
    public HomeViewModel HomeViewModel { get; }
    public ArtistsPageViewModel? ArtistsPageViewModel { get; private set; }
    public AlbumsPageViewModel? AlbumsPageViewModel { get; private set; }

    [ObservableProperty] private bool _isPlayerBarVisible = true;

    public bool IsBottomPlayerBarVisible => IsPlayerBarVisible;

    partial void OnIsPlayerBarVisibleChanged(bool value) => OnPropertyChanged(nameof(IsBottomPlayerBarVisible));

    public ObservableCollection<UserPlaylist> UserPlaylists => _userPlaylistService.UserPlaylists;

    public ObservableCollection<SmartPlaylist> SmartPlaylists { get; } = new();

    public SmartPlaylistSongsViewModel? SmartPlaylistSongsViewModel { get; private set; }

    [ObservableProperty] private bool _isPlaylistSectionExpanded = true;

    [RelayCommand]
    private void NavigateToPlaylistDetailFromSidebar(UserPlaylist playlist)
    {
        NavigateToPlaylistDetail(playlist);
    }

    [RelayCommand]
    private void NavigateToSmartPlaylist(SmartPlaylist smartPlaylist)
    {
        SmartPlaylistSongsViewModel = _viewModelFactory.CreateSmartPlaylistSongsViewModel(smartPlaylist);
        CurrentPage = SmartPlaylistSongsViewModel;
    }

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
        IPlayHistoryService playHistoryService,
        IDialogService dialogService,
        ISmartPlaylistService smartPlaylistService)
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
        _dialogService = dialogService;
        _smartPlaylistService = smartPlaylistService;

        foreach (var sp in _smartPlaylistService.GetSmartPlaylists())
            SmartPlaylists.Add(sp);

        // 初始化页面查找字典
        _pageLookup = CreatePageLookup();

        CurrentPlaylist = _playlistService.CreatePlaylist("DefaultPlaylist");
        _playlistService.SetCurrentPlaylist(CurrentPlaylist);

        QueueViewModel = _viewModelFactory.CreateQueueViewModel();
        PlayerPageViewModel = _viewModelFactory.CreatePlayerPageViewModel();
        PlayerPageViewModel.OnClose = () => IsPlayerOverlayOpen = false;
        PlayerPageViewModel.OnToggleFullScreen = () => ToggleFullScreenCommand.Execute(null);
        HomeViewModel = _viewModelFactory.CreateHomeViewModel();
        HomeViewModel.CurrentPlaylist = CurrentPlaylist;
        HomeViewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(HomeViewModel.IsGridView))
                OnPropertyChanged(nameof(IsGridView));
        };
        CurrentPage = HomeViewModel;
        _navigationService.NavigateTo<HomeViewModel>();

        // 使用 SubscribeEvent 辅助方法进行事件订阅，便于自动清理
        SubscribeEvent(
            () =>
            {
                _playbackStateService.PlaybackStateChanged += OnPlaybackStateChanged;
                _playbackStateService.CurrentSongChanged += OnCurrentSongChanged;
                _playbackStateService.PositionChanged += OnPositionChanged;
                _playbackStateService.PlaybackModeChanged += OnPlaybackModeChanged;

                _navigationService.QueuePanelChanged += OnQueuePanelChanged;
                _navigationService.CurrentPageChanged += OnCurrentPageChanged;

                Library.Songs.CollectionChanged += OnSongsCollectionChanged;
            },
            () =>
            {
                _playbackStateService.PlaybackStateChanged -= OnPlaybackStateChanged;
                _playbackStateService.CurrentSongChanged -= OnCurrentSongChanged;
                _playbackStateService.PositionChanged -= OnPositionChanged;
                _playbackStateService.PlaybackModeChanged -= OnPlaybackModeChanged;

                _navigationService.QueuePanelChanged -= OnQueuePanelChanged;
                _navigationService.CurrentPageChanged -= OnCurrentPageChanged;

                Library.Songs.CollectionChanged -= OnSongsCollectionChanged;
            }
        );

        _ = InitializeAsync(scanService);
    }

    private void OnPlaybackStateChanged(object? sender, PlayState state)
    {
        OnPropertyChanged(nameof(IsPlaying));
        _systemTrayService.UpdateTrayIcon(IsPlaying);
    }

    private void OnPlaybackModeChanged(object? sender, PlaybackMode mode)
    {
        _configService.CurrentSettings.PlaybackMode = mode.ToString();
        _ = _configService.SaveSettingsAsync().ContinueWith(_ => { }, TaskContinuationOptions.OnlyOnFaulted);
    }

    private void OnCurrentSongChanged(object? sender, Song? song)
    {
        OnPropertyChanged(nameof(CurrentSong));
        if (CurrentSong != null && _configService.CurrentSettings.ShowSongChangeNotification)
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
        if (pageType == null) return;

        // 使用字典查找替代 if-else 链，提高可维护性
        if (_pageLookup.TryGetValue(pageType, out var getViewModel))
        {
            CurrentPage = getViewModel() ?? CurrentPage;
        }
    }

    // 页面类型到 ViewModel 的映射字典
    private Dictionary<Type, Func<ViewModelBase?>> CreatePageLookup()
    {
        return new Dictionary<Type, Func<ViewModelBase?>>
        {
            [typeof(HomeViewModel)] = () => HomeViewModel,
            [typeof(PlayerPageViewModel)] = () => PlayerPageViewModel,
            [typeof(SettingsViewModel)] = () => SettingsViewModel,
            [typeof(StatisticsViewModel)] = () => StatisticsViewModel,
            [typeof(PlaylistManagementViewModel)] = () => PlaylistManagementViewModel,
            [typeof(PlaylistListViewModel)] = () => PlaylistListViewModel,
            [typeof(LibraryCategoryViewModel)] = () => LibraryCategoryViewModel,
            [typeof(ArtistDetailViewModel)] = () => ArtistDetailViewModel,
            [typeof(AlbumDetailViewModel)] = () => AlbumDetailViewModel,
            [typeof(PlayHistoryViewModel)] = () => PlayHistoryViewModel,
            [typeof(LibraryBrowserViewModel)] = () => LibraryBrowserViewModel,
            [typeof(ArtistsPageViewModel)] = () => ArtistsPageViewModel,
            [typeof(AlbumsPageViewModel)] = () => AlbumsPageViewModel,
            [typeof(StatisticsReportViewModel)] = () => StatisticsReportViewModel
        };
    }

    private readonly Dictionary<Type, Func<ViewModelBase?>> _pageLookup;

    private void OnSongsCollectionChanged(object? sender,
        System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        UpdateLibraryStats();
        FilterSongs();
    }

    protected override void DisposeCore()
    {
        // 事件订阅已在 SubscribeEvent 中自动管理
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
        try
        {
            await _configService.LoadSettingsAsync();
            Volume = _configService.CurrentSettings.Volume;
            IsMuted = _configService.CurrentSettings.IsMuted;
            _playbackStateService.SetVolume(Volume);
            if (IsMuted) _playbackStateService.Mute();
            _playbackStateService.SetPlaybackRate(_configService.CurrentSettings.PlaybackRate);
            if (Enum.TryParse<PlaybackMode>(_configService.CurrentSettings.PlaybackMode, out var playbackMode))
                _playbackStateService.PlaybackMode = playbackMode;

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
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MainWindowViewModel] Initialization failed: {ex}");
        }
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
