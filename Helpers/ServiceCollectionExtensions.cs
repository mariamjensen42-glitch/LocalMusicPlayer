using LocalMusicPlayer.Data;
using LocalMusicPlayer.Services;
using LocalMusicPlayer.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LocalMusicPlayer.Helpers;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMusicServices(this IServiceCollection services)
    {
        AddLoggingServices(services);
        AddNavigationServices(services);
        AddPlaybackServices(services);
        AddLibraryServices(services);
        AddMediaServices(services);
        AddPlaylistServices(services);
        AddSystemServices(services);
        AddStatisticsServices(services);
        AddFileServices(services);
        AddViewModels(services);
        return services;
    }

    private static void AddLoggingServices(IServiceCollection services)
    {
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });
    }

    private static void AddNavigationServices(IServiceCollection services)
    {
        services.AddSingleton<IWindowProvider, WindowProvider>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IKeyboardShortcutService, KeyboardShortcutService>();
        services.AddSingleton<IDropHandlerService, DropHandlerService>();
        services.AddSingleton<IViewModelFactory, ViewModelFactory>();
    }

    private static void AddPlaybackServices(IServiceCollection services)
    {
        services.AddSingleton<IMusicPlayerService, MusicPlayerService>();
        services.AddSingleton<IPlaybackStateService, PlaybackStateService>();
    }

    private static void AddLibraryServices(IServiceCollection services)
    {
        services.AddSingleton<IFileScannerService, FileScannerService>();
        services.AddSingleton<IMusicLibraryService, MusicLibraryService>();
        services.AddSingleton<IScanService, ScanService>();
        services.AddSingleton<ILibraryCategoryService, LibraryCategoryService>();
        services.AddSingleton<IDedupService, DedupService>();
    }

    private static void AddMediaServices(IServiceCollection services)
    {
        services.AddSingleton<IAlbumArtService, AlbumArtService>();
        services.AddSingleton<ICoverManagerService, CoverManagerService>();
        services.AddSingleton<ILyricsService, LyricsService>();
        services.AddSingleton<IOnlineLyricsService, OnlineLyricsService>();
    }

    private static void AddPlaylistServices(IServiceCollection services)
    {
        services.AddSingleton<IPlaylistService, PlaylistService>();
        services.AddSingleton<IUserPlaylistService, UserPlaylistService>();
        services.AddSingleton<IPlayHistoryService, PlayHistoryService>();
        services.AddSingleton<ISmartPlaylistService, SmartPlaylistService>();
    }

    private static void AddSystemServices(IServiceCollection services)
    {
        services.AddSingleton<IConfigurationService, ConfigurationService>();
        services.AddSingleton<ISystemTrayService, SystemTrayService>();
#pragma warning disable CA1416
        services.AddSingleton<IAutoStartService, AutoStartService>();
#pragma warning restore CA1416
        services.AddSingleton<AppDbContext>();
        services.AddSingleton<IDatabaseService, DatabaseService>();
    }

    private static void AddStatisticsServices(IServiceCollection services)
    {
        services.AddSingleton<IStatisticsService, StatisticsService>();
    }

    private static void AddFileServices(IServiceCollection services)
    {
        services.AddSingleton<IFileWatcherService, FileWatcherService>();
        services.AddSingleton<IFileManagerService, FileManagerService>();
    }

    private static void AddViewModels(IServiceCollection services)
    {
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<StatisticsViewModel>();
        services.AddTransient<LibraryBrowserViewModel>();
        services.AddTransient<StatisticsReportViewModel>();
        services.AddTransient<PlayerPageViewModel>();
        services.AddTransient<PlaylistManagementViewModel>();
        services.AddTransient<PlaylistListViewModel>();
        services.AddTransient<LibraryCategoryViewModel>();
        services.AddTransient<QueueViewModel>();
        services.AddTransient<PlayHistoryViewModel>();
        services.AddTransient<ArtistDetailViewModel>();
        services.AddTransient<AlbumDetailViewModel>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<ArtistsPageViewModel>();
        services.AddTransient<AlbumsPageViewModel>();
        services.AddTransient<MetadataEditorViewModel>();
        services.AddTransient<BatchMetadataEditorViewModel>();
        services.AddTransient<SmartPlaylistSongsViewModel>();
    }
}
