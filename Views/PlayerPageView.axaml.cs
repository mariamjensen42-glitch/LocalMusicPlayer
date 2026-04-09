using Avalonia.Controls;
using LocalMusicPlayer.ViewModels;

namespace LocalMusicPlayer.Views;

public partial class PlayerPageView : UserControl
{
    private ScrollViewer? _lyricsScrollViewer;

    public PlayerPageView()
    {
        InitializeComponent();

        DataContextChanged += (_, _) =>
        {
            if (DataContext is PlayerPageViewModel viewModel)
            {
                _lyricsScrollViewer = this.FindControl<ScrollViewer>("LyricsScrollViewer");

                viewModel.OnScrollToLyric = ScrollToCurrentLyric;
            }
        };
    }

    private void ScrollToCurrentLyric(int index)
    {
        if (_lyricsScrollViewer == null || index < 0)
            return;

        var scrollViewer = _lyricsScrollViewer;
        var viewportHeight = scrollViewer.Viewport.Height;
        var scrollableHeight = scrollViewer.Extent.Height - viewportHeight;

        var lyricItemHeight = 80.0;
        var margin = 300.0;

        var targetPosition = margin + (index * lyricItemHeight) - (viewportHeight / 2) + (lyricItemHeight / 2);

        targetPosition = double.Max(0, double.Min(scrollableHeight, targetPosition));

        scrollViewer.Offset = new Avalonia.Vector(0, targetPosition);
    }
}
