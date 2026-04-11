using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using LocalMusicPlayer.ViewModels;
using LocalMusicPlayer.Services;

namespace LocalMusicPlayer.Views.Main;

public partial class MainWindow : Window
{
    private readonly IKeyboardShortcutService _keyboardShortcutService;
    private readonly IDropHandlerService _dropHandlerService;
    private MainWindowViewModel? _mainWindowViewModel;

    public MainWindow(IDropHandlerService dropHandlerService, IKeyboardShortcutService keyboardShortcutService)
    {
        _dropHandlerService = dropHandlerService;
        _keyboardShortcutService = keyboardShortcutService;

        InitializeComponent();

        TitleBar.PointerPressed += OnTitleBarPointerPressed;
        PropertyChanged += OnPropertyChanged;
        UpdateMaximizeRestoreIcons();
        DataContextChanged += OnDataContextChanged;
        KeyDown += OnKeyDown;

        AddHandler(DragDrop.DragOverEvent, OnDragOver);
        AddHandler(DragDrop.DropEvent, OnDrop);
    }

    private void OnDragOver(object? sender, DragEventArgs e)
    {
#pragma warning disable CS0618
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
#pragma warning disable CS0618
        if (!e.Data.Contains(DataFormats.FileNames))
            return;

        var files = e.Data.GetFileNames();
#pragma warning restore CS0618
        if (files == null || !files.Any())
            return;

        await _dropHandlerService.HandleDroppedFilesAsync(files);
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            _mainWindowViewModel = vm;
        }
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Source is TextBox)
            return;

        if (_mainWindowViewModel == null)
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
        ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.NoChrome;
        ExtendClientAreaToDecorationsHint = true;

        TitleBar.Background = isPlayerPage
            ? FindResource<SolidColorBrush>("BgSidebarBrush")
            : FindResource<SolidColorBrush>("BgPrimaryBrush");
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

    private T? FindResource<T>(string key) where T : class
    {
        return (T?)this.FindResource(key);
    }
}
