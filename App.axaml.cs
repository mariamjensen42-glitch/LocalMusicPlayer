using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using LocalMusicPlayer.Helpers;
using LocalMusicPlayer.Models;
using LocalMusicPlayer.Services;
using LocalMusicPlayer.ViewModels;
using LocalMusicPlayer.Views.Main;
using Microsoft.Extensions.DependencyInjection;

namespace LocalMusicPlayer;

public partial class App : Application
{
    private IServiceProvider _services = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _services = new ServiceCollection().AddMusicServices().BuildServiceProvider();

            var windowProvider = _services.GetRequiredService<IWindowProvider>();
            var dropHandlerService = _services.GetRequiredService<IDropHandlerService>();
            var keyboardShortcutService = _services.GetRequiredService<IKeyboardShortcutService>();
            var configService = _services.GetRequiredService<IConfigurationService>();

            desktop.MainWindow = new MainWindow(dropHandlerService, keyboardShortcutService, configService);
            windowProvider.CurrentWindow = desktop.MainWindow;

            desktop.MainWindow.Loaded += async (_, _) =>
            {
                var databaseService = _services.GetRequiredService<IDatabaseService>();
                await databaseService.InitializeAsync();

                var mainWindowViewModel = _services.GetRequiredService<MainWindowViewModel>();
                desktop.MainWindow!.DataContext = mainWindowViewModel;

                var systemTrayService = _services.GetRequiredService<ISystemTrayService>();
                systemTrayService.Initialize();

                if (configService.CurrentSettings.ResumeLastPlayback)
                {
                    var settings = configService.CurrentSettings;
                    if (!string.IsNullOrEmpty(settings.LastSongFilePath) && settings.QueueFilePaths.Count > 0)
                    {
                        var musicLibraryService = _services.GetRequiredService<IMusicLibraryService>();
                        var playlistService = _services.GetRequiredService<IPlaylistService>();
                        var playbackStateService = _services.GetRequiredService<IPlaybackStateService>();

                        var songs = settings.QueueFilePaths
                            .Select(p => musicLibraryService.Songs.FirstOrDefault(s => s.FilePath == p))
                            .Where(s => s != null)
                            .ToList();

                        if (songs.Count > 0)
                        {
                            var lastSong = songs.FirstOrDefault(s => s?.FilePath == settings.LastSongFilePath) ?? songs[0];
                            var playlist = new Playlist();
                            foreach (var song in songs)
                            {
                                if (song != null) playlist.Songs.Add(song);
                            }
                            playlistService.SetCurrentPlaylist(playlist);
                            playlistService.PlaySong(lastSong);
                            playbackStateService.Seek(TimeSpan.FromSeconds(settings.LastPlaybackPosition));
                        }
                    }
                }
            };

            desktop.MainWindow.Closing += async (_, _) =>
            {
                var cfgService = _services.GetRequiredService<IConfigurationService>();
                var playlistService = _services.GetRequiredService<IPlaylistService>();
                var playbackStateService = _services.GetRequiredService<IPlaybackStateService>();

                var currentSong = playlistService.CurrentSong;
                var queueFilePaths = playlistService.CurrentPlaylist?.Songs
                    .Select(s => s.FilePath)
                    .ToList() ?? new List<string>();
                var lastPosition = playbackStateService.Position.TotalSeconds;

                await cfgService.SavePlaybackStateAsync(
                    currentSong?.FilePath,
                    queueFilePaths,
                    lastPosition);

                await cfgService.SaveSettingsAsync();
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
