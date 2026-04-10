using System;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using LocalMusicPlayer.ViewModels;
using LocalMusicPlayer.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LocalMusicPlayer.Views;

public partial class MainWindow : Window
{
    private IKeyboardShortcutService? _keyboardShortcutService;
    private MainWindowViewModel? _mainWindowViewModel;

    public MainWindow()
    {
        InitializeComponent();

        TitleBar.PointerPressed += OnTitleBarPointerPressed;
        PropertyChanged += OnPropertyChanged;
        UpdateMaximizeRestoreIcons();
        DataContextChanged += OnDataContextChanged;
        KeyDown += OnKeyDown;
        
        // 绑定搜索框
        SearchTextBox.TextChanged += (_, e) =>
        {
            if (DataContext is ViewModels.MainWindowViewModel vm)
            {
                vm.SearchText = SearchTextBox.Text ?? string.Empty;
            }
        };

        AddHandler(DragDrop.DragOverEvent, OnDragOver);
        AddHandler(DragDrop.DropEvent, OnDrop);
    }

    private void OnDragOver(object? sender, DragEventArgs e)
    {
#pragma warning disable CS0618 // DataFormats is obsolete
        if (e.Data.Contains(DataFormats.FileNames))
#pragma warning restore CS0618
        {
            e.DragEffects = DragDropEffects.Copy;
        }
        else
        {
            e.DragEffects = DragDropEffects.None;
        }

        e.Handled = true;
    }

    private async void OnDrop(object? sender, DragEventArgs e)
    {
#pragma warning disable CS0618 // DataFormats and GetFileNames are obsolete
        if (!e.Data.Contains(DataFormats.FileNames))
            return;

        var files = e.Data.GetFileNames();
#pragma warning restore CS0618
        if (files == null || !files.Any())
            return;

        var app = Avalonia.Application.Current as App;
        if (app?.Services == null)
            return;

        var fileScannerService = app.Services.GetService(typeof(IFileScannerService)) as IFileScannerService;
        var musicLibraryService = app.Services.GetService(typeof(IMusicLibraryService)) as IMusicLibraryService;

        if (fileScannerService == null || musicLibraryService == null)
            return;

        foreach (var path in files)
        {
            if (string.IsNullOrEmpty(path))
                continue;

            if (File.Exists(path) && fileScannerService.SupportedExtensions.Contains(
                    Path.GetExtension(path).ToLowerInvariant()))
            {
                await AddSingleFileAsync(path, fileScannerService, musicLibraryService);
            }
            else if (Directory.Exists(path))
            {
                await AddFolderAsync(path, fileScannerService, musicLibraryService);
            }
        }
    }

    private async System.Threading.Tasks.Task AddSingleFileAsync(string filePath,
        IFileScannerService fileScannerService, IMusicLibraryService musicLibraryService)
    {
        try
        {
            var directory = Path.GetDirectoryName(filePath);
            if (directory == null) return;

            var songs = await fileScannerService.ScanDirectoryAsync(directory, false);
            var song = songs.FirstOrDefault(s => s.FilePath == filePath);
            if (song != null && !musicLibraryService.Songs.Any(s => s.FilePath == filePath))
            {
                musicLibraryService.AddSong(song);
            }
        }
        catch
        {
        }
    }

    private async System.Threading.Tasks.Task AddFolderAsync(string folderPath, IFileScannerService fileScannerService,
        IMusicLibraryService musicLibraryService)
    {
        try
        {
            var songs = await fileScannerService.ScanDirectoryAsync(folderPath, true);
            foreach (var song in songs)
            {
                if (!musicLibraryService.Songs.Any(s => s.FilePath == song.FilePath))
                {
                    musicLibraryService.AddSong(song);
                }
            }
        }
        catch
        {
        }
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            _mainWindowViewModel = vm;
            vm.PropertyChanged += (s, args) =>
            {
                if (args.PropertyName == nameof(MainWindowViewModel.IsPlayerPageVisible))
                {
                    UpdateTitleBarChrome(vm.IsPlayerPageVisible);
                }
            };
        }
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Source is TextBox)
            return;

        _keyboardShortcutService ??= ((App.Current as App)?.Services?.GetService<IKeyboardShortcutService>());

        if (_keyboardShortcutService == null || _mainWindowViewModel == null)
            return;

        _keyboardShortcutService.SetNavigateBackAction(() =>
        {
            _mainWindowViewModel.NavigateToLibraryCommand.Execute(null);
        });

        switch (e.Key)
        {
            case Key.Space:
                _keyboardShortcutService.PlayPause();
                e.Handled = true;
                break;
            case Key.Left:
                _keyboardShortcutService.SeekBackward(5);
                e.Handled = true;
                break;
            case Key.Right:
                _keyboardShortcutService.SeekForward(5);
                e.Handled = true;
                break;
            case Key.Up:
                _keyboardShortcutService.VolumeUp(5);
                e.Handled = true;
                break;
            case Key.Down:
                _keyboardShortcutService.VolumeDown(5);
                e.Handled = true;
                break;
            case Key.Escape:
                _keyboardShortcutService.NavigateBack();
                e.Handled = true;
                break;
            case Key.M:
                _keyboardShortcutService.ToggleMute();
                e.Handled = true;
                break;
            case Key.N:
                _keyboardShortcutService.NextTrack();
                e.Handled = true;
                break;
            case Key.P:
                _keyboardShortcutService.PreviousTrack();
                e.Handled = true;
                break;
        }
    }

    private void UpdateTitleBarChrome(bool isPlayerPage)
    {
        ExtendClientAreaChromeHints = isPlayerPage
            ? Avalonia.Platform.ExtendClientAreaChromeHints.NoChrome
            : Avalonia.Platform.ExtendClientAreaChromeHints.NoChrome;
        ExtendClientAreaToDecorationsHint = true;

        TitleBar.Background = isPlayerPage
            ? new SolidColorBrush(Color.FromRgb(30, 30, 46))
            : new SolidColorBrush(Color.FromRgb(10, 10, 10));
    }

    private void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == WindowStateProperty)
        {
            UpdateMaximizeRestoreIcons();
        }
    }

    private void UpdateMaximizeRestoreIcons()
    {
        MaximizeIcon.IsVisible = WindowState != WindowState.Maximized;
        RestoreIcon.IsVisible = WindowState == WindowState.Maximized;
    }

    private void OnTitleBarPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(e);
        }
    }

    private void MinimizeButton_Click(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void MaximizeButton_Click(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        Hide();
    }

    public void PlayPause()
    {
        var vm = DataContext as ViewModels.MainWindowViewModel;
        if (vm != null)
        {
            if (vm.IsPlaying)
                vm.PauseCommand.Execute(null);
            else
                vm.PlayCommand.Execute(null);
        }
    }

    public void NextTrack()
    {
        var vm = DataContext as ViewModels.MainWindowViewModel;
        if (vm != null)
        {
            vm.NextCommand.Execute(null);
        }
    }

    public void PreviousTrack()
    {
        var vm = DataContext as ViewModels.MainWindowViewModel;
        if (vm != null)
        {
            vm.PreviousCommand.Execute(null);
        }
    }
}
