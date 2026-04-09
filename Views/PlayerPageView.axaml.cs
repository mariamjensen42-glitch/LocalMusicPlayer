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

                // 监听当前歌词索引变化，自动滚动
                viewModel.PropertyChanged += (_, e) =>
                {
                    if (e.PropertyName == nameof(PlayerPageViewModel.CurrentLyricIndex))
                    {
                        ScrollToCurrentLyric(viewModel.CurrentLyricIndex);
                    }
                };
            }
        };
    }

    private void ScrollToCurrentLyric(int index)
    {
        if (_lyricsScrollViewer == null || index < 0)
            return;

        // 估算每个歌词项的高度（包括间距）
        const double estimatedItemHeight = 80;
        var targetOffset = (index * estimatedItemHeight) - (_lyricsScrollViewer.Viewport.Height / 2) +
                           (estimatedItemHeight / 2);

        // 确保偏移量在有效范围内
        targetOffset = double.Max(0, targetOffset);

        _lyricsScrollViewer.Offset = new Avalonia.Vector(0, targetOffset);
    }
}
