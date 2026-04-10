using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using Avalonia.Styling;
using LocalMusicPlayer.Views;

namespace LocalMusicPlayer.Services;

internal class SystemTrayService : ISystemTrayService, IDisposable
{
    private MainWindow? _mainWindow;
    private TrayIcon? _trayIcon;

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

        var icon = new WindowIcon(AssetLoader.Open(new Uri("avares://LocalMusicPlayer/Assets/avalonia-logo.ico")));

        var showMenuItem = new NativeMenuItem("显示主窗口");
        showMenuItem.Click += (_, _) => ShowMainWindow();

        var exitMenuItem = new NativeMenuItem("退出");
        exitMenuItem.Click += (_, _) => ExitApplication();

        var menu = new NativeMenu();
        menu.Add(showMenuItem);
        menu.Add(new NativeMenuItemSeparator());
        menu.Add(exitMenuItem);

        _trayIcon = new TrayIcon
        {
            Icon = icon,
            ToolTipText = "LocalMusicPlayer",
            Menu = menu
        };

        _trayIcon.Clicked += (_, _) => ShowMainWindow();

        _mainWindow.Closing += (_, e) =>
        {
            e.Cancel = true;
            _mainWindow.Hide();
        };
    }

    private void ShowMainWindow()
    {
        if (_mainWindow == null) return;

        _mainWindow.Show();
        _mainWindow.WindowState = WindowState.Normal;
        _mainWindow.Activate();
    }

    private void ExitApplication()
    {
        if (_mainWindow == null) return;
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _mainWindow.Closing -= OnMainWindowClosing;
            desktop.Shutdown();
        }
    }

    private void OnMainWindowClosing(object? sender, WindowClosingEventArgs e)
    {
        e.Cancel = true;
        _mainWindow?.Hide();
    }

    public void UpdateTrayIcon(bool isPlaying)
    {
    }

    public void ShowNotification(string title, string message)
    {
    }

    public void Dispose()
    {
        _trayIcon?.Dispose();
    }
}
