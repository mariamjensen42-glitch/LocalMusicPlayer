using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
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

            desktop.MainWindow.Loaded += (_, _) =>
            {
                var mainWindowViewModel = Services.GetRequiredService<MainWindowViewModel>();
                desktop.MainWindow!.DataContext = mainWindowViewModel;
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
        services.AddSingleton<IFileScannerService, FileScannerService>();
        services.AddSingleton<IFileWatcherService, FileWatcherService>();
        services.AddSingleton<IMusicPlayerService, MusicPlayerService>();
        services.AddSingleton<IPlaylistService, PlaylistService>();
        services.AddSingleton<IMusicLibraryService, MusicLibraryService>();
        services.AddSingleton<IScanService, ScanService>();
        services.AddSingleton<ILyricsService, LyricsService>();
        services.AddSingleton<IStatisticsService, StatisticsService>();
        services.AddSingleton<IUserPlaylistService, UserPlaylistService>();
        services.AddSingleton<IKeyboardShortcutService, KeyboardShortcutService>();
        services.AddSingleton<ILibraryCategoryService, LibraryCategoryService>();
        services.AddSingleton<IPlaybackStateService, PlaybackStateService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IViewModelFactory, ViewModelFactory>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<StatisticsViewModel>();
        services.AddTransient<MainWindowViewModel>();
    }
}
