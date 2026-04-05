using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using LocalMusicPlayer.ViewModels;

namespace LocalMusicPlayer.Views;

public partial class MainWindow : Window
{
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
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.PropertyChanged += (s, args) =>
            {
                if (args.PropertyName == nameof(MainWindowViewModel.IsPlayerPageVisible))
                {
                    UpdateTitleBarChrome(vm.IsPlayerPageVisible);
                }
            };
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