using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using LocalMusicPlayer.ViewModels;

namespace LocalMusicPlayer.Views.Player;

public partial class PlayerPageView : UserControl
{
    private ScrollViewer? _lyricsScrollViewer;
    private Popup? _speedPopup;

    public PlayerPageView()
    {
        InitializeComponent();

        DataContextChanged += (_, _) =>
        {
            if (DataContext is PlayerPageViewModel viewModel)
            {
                _lyricsScrollViewer = this.FindControl<ScrollViewer>("LyricsScrollViewer");
                _speedPopup = this.FindControl<Popup>("SpeedPopup");

                viewModel.OnScrollToLyric = ScrollToCurrentLyric;
            }
        };
    }

    public void OnSpeedButtonClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_speedPopup != null)
        {
            _speedPopup.IsOpen = !_speedPopup.IsOpen;
        }
    }

    private void ScrollToCurrentLyric(int index)
    {
        if (_lyricsScrollViewer == null || index < 0)
            return;

        Dispatcher.UIThread.Post(() =>
        {
            var scrollViewer = _lyricsScrollViewer;
            var viewportHeight = scrollViewer.Viewport.Height;
            var scrollableHeight = scrollViewer.Extent.Height - viewportHeight;

            var lyricItemHeight = 80.0;
            var margin = 300.0;

            var targetPosition = margin + (index * lyricItemHeight) - (viewportHeight / 2) + (lyricItemHeight / 2);

            targetPosition = double.Max(0, double.Min(scrollableHeight, targetPosition));

            scrollViewer.Offset = new Avalonia.Vector(0, targetPosition);
        });
    }
}
