using System;
using System.Timers;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using LocalMusicPlayer.ViewModels;
using Timer = System.Timers.Timer;

namespace LocalMusicPlayer.Views.Player;

public partial class MusicProgressBar : UserControl
{
    private Panel? _trackPanel;
    private Border? _progressFill;
    private IPlaybackProgress? _viewModel;
    private Timer? _updateTimer;

    public MusicProgressBar()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Avalonia.Markup.Xaml.AvaloniaXamlLoader.Load(this);
        _trackPanel = this.FindControl<Panel>("TrackPanel");
        _progressFill = this.FindControl<Border>("ProgressFill");

        _trackPanel?.AddHandler(Panel.PointerPressedEvent, OnTrackPointerPressed, RoutingStrategies.Bubble);

        DataContextChanged += OnDataContextChanged;
        DetachedFromVisualTree += (_, _) => StopUpdateTimer();
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is IPlaybackProgress vm)
        {
            _viewModel = vm;
            StartUpdateTimer();
            UpdateProgressUI();
        }
        else
        {
            StopUpdateTimer();
        }
    }

    private void StartUpdateTimer()
    {
        StopUpdateTimer();
        _updateTimer = new Timer(250);
        _updateTimer.Elapsed += (_, _) => Avalonia.Threading.Dispatcher.UIThread.Post(UpdateProgressUI);
        _updateTimer.Start();
    }

    private void StopUpdateTimer()
    {
        if (_updateTimer != null)
        {
            _updateTimer.Stop();
            _updateTimer.Dispose();
            _updateTimer = null;
        }
    }

    private void OnTrackPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_viewModel == null || _trackPanel == null) return;

        var point = e.GetPosition(_trackPanel);
        var width = _trackPanel.Bounds.Width;

        if (width <= 0 || _viewModel.DurationSeconds <= 0) return;

        var ratio = Math.Clamp(point.X / width, 0.0, 1.0);
        var seekSeconds = ratio * _viewModel.DurationSeconds;

        if (_viewModel is MainWindowViewModel mainVm)
        {
            mainVm.SeekCommand.Execute(TimeSpan.FromSeconds(seekSeconds));
        }

        UpdateProgressUI();
    }

    private void UpdateProgressUI()
    {
        if (_viewModel == null || _trackPanel == null || _progressFill == null)
            return;

        var width = _trackPanel.Bounds.Width;
        if (width <= 0 || _viewModel.DurationSeconds <= 0) return;

        var ratio = Math.Clamp(_viewModel.PositionSeconds / _viewModel.DurationSeconds, 0.0, 1.0);
        var fillWidth = ratio * width;

        _progressFill.Width = fillWidth;
    }
}