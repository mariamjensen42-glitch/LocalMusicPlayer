using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
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

        // 设置标题栏可拖动移动窗口
        TitleBar.PointerPressed += OnTitleBarPointerPressed;

        // 监听窗口状态变化以更新最大化/还原图标
        PropertyChanged += OnPropertyChanged;
        UpdateMaximizeRestoreIcons();

        // 监听播放页面切换，调整标题栏区域扩展
        DataContextChanged += OnDataContextChanged;

        // 监听键盘事件
        KeyDown += OnKeyDown;
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
        // 如果焦点在输入控件上，不处理快捷键
        if (e.Source is TextBox)
            return;

        _keyboardShortcutService ??= ((App.Current as App)?.Services?.GetService<IKeyboardShortcutService>());

        if (_keyboardShortcutService == null || _mainWindowViewModel == null)
            return;

        // 设置导航返回动作
        _keyboardShortcutService.SetNavigateBackAction(() =>
        {
            _mainWindowViewModel.NavigateToLibraryCommand.Execute().Subscribe();
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
        // 播放页面时完全隐藏标题栏，其他页面显示自定义标题栏
        ExtendClientAreaChromeHints = isPlayerPage
            ? Avalonia.Platform.ExtendClientAreaChromeHints.NoChrome
            : Avalonia.Platform.ExtendClientAreaChromeHints.NoChrome;
        ExtendClientAreaToDecorationsHint = true;
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
        Close();
    }
}