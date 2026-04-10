using LocalMusicPlayer.ViewModels;

namespace LocalMusicPlayer.Services;

public class ViewModelFactory : IViewModelFactory
{
    private readonly IMusicPlayerService _musicPlayerService;
    private readonly IPlaylistService _playlistService;
    private readonly IMusicLibraryService _musicLibraryService;
    private readonly ILyricsService _lyricsService;
    private readonly IStatisticsService _statisticsService;
    private readonly IUserPlaylistService _userPlaylistService;
    private readonly ILibraryCategoryService _libraryCategoryService;
    private readonly IDialogService _dialogService;
    private readonly IWindowProvider _windowProvider;
    private readonly IScanService _scanService;
    private readonly IConfigurationService _configService;
    private readonly IPlaybackStateService _playbackStateService;
    private readonly INavigationService _navigationService;
    private readonly IAlbumArtService _albumArtService;
    private readonly IPlayHistoryService _playHistoryService;

    public ViewModelFactory(
        IMusicPlayerService musicPlayerService,
        IPlaylistService playlistService,
        IMusicLibraryService musicLibraryService,
        ILyricsService lyricsService,
        IStatisticsService statisticsService,
        IUserPlaylistService userPlaylistService,
        ILibraryCategoryService libraryCategoryService,
        IDialogService dialogService,
        IWindowProvider windowProvider,
        IScanService scanService,
        IConfigurationService configService,
        IPlaybackStateService playbackStateService,
        INavigationService navigationService,
        IAlbumArtService albumArtService,
        IPlayHistoryService playHistoryService)
    {
        _musicPlayerService = musicPlayerService;
        _playlistService = playlistService;
        _musicLibraryService = musicLibraryService;
        _lyricsService = lyricsService;
        _statisticsService = statisticsService;
        _userPlaylistService = userPlaylistService;
        _libraryCategoryService = libraryCategoryService;
        _dialogService = dialogService;
        _windowProvider = windowProvider;
        _scanService = scanService;
        _configService = configService;
        _playbackStateService = playbackStateService;
        _navigationService = navigationService;
        _albumArtService = albumArtService;
        _playHistoryService = playHistoryService;
    }

    public PlayerPageViewModel CreatePlayerPageViewModel()
    {
        return new PlayerPageViewModel(
            _playbackStateService,
            _navigationService,
            _lyricsService,
            _musicLibraryService,
            _albumArtService,
            _configService);
    }

    public PlaylistManagementViewModel CreatePlaylistManagementViewModel()
    {
        return new PlaylistManagementViewModel(
            _userPlaylistService,
            _musicPlayerService,
            _playlistService,
            _statisticsService,
            _musicLibraryService,
            _dialogService);
    }

    public SettingsViewModel CreateSettingsViewModel()
    {
        return new SettingsViewModel(
            _windowProvider,
            _scanService,
            _musicLibraryService,
            _configService,
            _playbackStateService);
    }

    public StatisticsViewModel CreateStatisticsViewModel()
    {
        return new StatisticsViewModel(
            _statisticsService,
            _musicLibraryService);
    }

    public LibraryCategoryViewModel CreateLibraryCategoryViewModel()
    {
        return new LibraryCategoryViewModel(
            _libraryCategoryService,
            _musicPlayerService,
            _playlistService,
            _statisticsService);
    }

    public QueueViewModel CreateQueueViewModel()
    {
        return new QueueViewModel(
            _playlistService,
            _playbackStateService,
            _navigationService);
    }

    public PlayHistoryViewModel CreatePlayHistoryViewModel()
    {
        return new PlayHistoryViewModel(
            _playHistoryService,
            _playbackStateService,
            _playlistService,
            _statisticsService);
    }
}
