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
            var serviceProvider = services.BuildServiceProvider();

            var windowProvider = serviceProvider.GetRequiredService<IWindowProvider>();

            desktop.MainWindow = new MainWindow();
            windowProvider.CurrentWindow = desktop.MainWindow;

            desktop.MainWindow.Loaded += (_, _) =>
            {
                var mainWindowViewModel = serviceProvider.GetRequiredService<MainWindowViewModel>();
                desktop.MainWindow!.DataContext = mainWindowViewModel;
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IWindowProvider, WindowProvider>();
        services.AddSingleton<IConfigurationService, ConfigurationService>();
        services.AddSingleton<IAlbumArtService, AlbumArtService>();
        services.AddSingleton<IFileScannerService, FileScannerService>();
        services.AddSingleton<IMusicPlayerService, MusicPlayerService>();
        services.AddSingleton<IPlaylistService, PlaylistService>();
        services.AddSingleton<IMusicLibraryService, MusicLibraryService>();
        services.AddSingleton<IScanService, ScanService>();
        services.AddSingleton<ILyricsService, LyricsService>();
        services.AddSingleton<IStatisticsService, StatisticsService>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<StatisticsViewModel>();
        services.AddTransient<MainWindowViewModel>();
    }
}
