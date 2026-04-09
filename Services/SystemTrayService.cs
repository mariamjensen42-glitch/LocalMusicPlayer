using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using LocalMusicPlayer.Views;

namespace LocalMusicPlayer.Services;

internal class SystemTrayService : ISystemTrayService, IDisposable
{
    private MainWindow? _mainWindow;
    private WindowIcon? _trayIcon;

    public void Initialize()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop &&
            desktop.MainWindow is MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            SetupSystemTray();
        }
    }

    private void SetupSystemTray()
    {
        if (_mainWindow == null) return;

        _trayIcon = new WindowIcon(AssetLoader.Open(new Uri("avares://LocalMusicPlayer/Assets/avalonia-logo.ico")));

        _mainWindow.Closing += (_, e) =>
        {
            e.Cancel = true;
            _mainWindow.Hide();
        };
    }

    public void UpdateTrayIcon(bool isPlaying)
    {
        // 可以根据播放状态更换不同的图标
    }

    public void ShowNotification(string title, string message)
    {
        // 简单的通知实现
    }

    public void Dispose()
    {
    }
}
