using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using LocalMusicPlayer.Data;
using LocalMusicPlayer.Services;
using LocalMusicPlayer.ViewModels;
using LocalMusicPlayer.Views;

namespace LocalMusicPlayer;

public partial class App : Application
{
    public IServiceProvider? Services { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            Services = services.BuildServiceProvider();

            var windowProvider = Services.GetRequiredService<IWindowProvider>();

            desktop.MainWindow = new MainWindow();
            windowProvider.CurrentWindow = desktop.MainWindow;

            desktop.MainWindow.Loaded += async (_, _) =>
            {
                var databaseService = Services.GetRequiredService<IDatabaseService>();
                await databaseService.InitializeAsync();

                var mainWindowViewModel = Services.GetRequiredService<MainWindowViewModel>();
                desktop.MainWindow!.DataContext = mainWindowViewModel;
                
                var systemTrayService = Services.GetRequiredService<ISystemTrayService>();
                systemTrayService.Initialize();
            };

            desktop.MainWindow.Closing += async (_, _) =>
            {
                var playerService = Services.GetRequiredService<IMusicPlayerService>();
                if (playerService is IDisposable disposable)
                    disposable.Dispose();

                var configService = Services.GetRequiredService<IConfigurationService>();
                var playlistService = Services.GetRequiredService<IPlaylistService>();
                var playbackStateService = Services.GetRequiredService<IPlaybackStateService>();

                var currentSong = playlistService.CurrentSong;
                var queueFilePaths = playlistService.CurrentPlaylist?.Songs
                    .Select(s => s.FilePath)
                    .ToList() ?? new List<string>();
                var lastPosition = playbackStateService.Position.TotalSeconds;

                await configService.SavePlaybackStateAsync(
                    currentSong?.FilePath,
                    queueFilePaths,
                    lastPosition);
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IWindowProvider, WindowProvider>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<IConfigurationService, ConfigurationService>();
        services.AddSingleton<IAlbumArtService, AlbumArtService>();
        services.AddSingleton<ICoverManagerService, CoverManagerService>();
        services.AddSingleton<IFileScannerService, FileScannerService>();
        services.AddSingleton<IFileWatcherService, FileWatcherService>();
        services.AddSingleton<IMusicPlayerService, MusicPlayerService>();
        services.AddSingleton<IPlaylistService, PlaylistService>();
        services.AddSingleton<IMusicLibraryService, MusicLibraryService>();
        services.AddSingleton<IScanService, ScanService>();
        services.AddSingleton<ILyricsService, LyricsService>();
        services.AddSingleton<IStatisticsService, StatisticsService>();
        services.AddSingleton<AppDbContext>();
        services.AddSingleton<IDatabaseService, DatabaseService>();
        services.AddSingleton<IUserPlaylistService, UserPlaylistService>();
        services.AddSingleton<IKeyboardShortcutService, KeyboardShortcutService>();
        services.AddSingleton<ILibraryCategoryService, LibraryCategoryService>();
        services.AddSingleton<IPlaybackStateService, PlaybackStateService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<ISystemTrayService, SystemTrayService>();
        services.AddSingleton<IPlayHistoryService, PlayHistoryService>();
        services.AddSingleton<IDedupService, DedupService>();
        services.AddSingleton<IViewModelFactory, ViewModelFactory>();
        services.AddSingleton<IFileManagerService, FileManagerService>();
        services.AddSingleton<IDropHandlerService, DropHandlerService>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<StatisticsViewModel>();
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<LibraryBrowserViewModel>();
        services.AddTransient<StatisticsReportViewModel>();
    }
}
