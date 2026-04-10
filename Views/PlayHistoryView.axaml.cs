using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using LocalMusicPlayer.ViewModels;

namespace LocalMusicPlayer.Views;

public partial class PlayHistoryView : UserControl
{
    public PlayHistoryView()
    {
        InitializeComponent();
    }

    private void OnHistoryEntryClick(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border border && border.DataContext is not null)
        {
            e.Handled = false;
        }
    }
}
