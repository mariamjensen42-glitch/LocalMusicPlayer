using Avalonia.Controls;
using Avalonia.Input;
using LocalMusicPlayer.Models;
using LocalMusicPlayer.ViewModels;

namespace LocalMusicPlayer.Views;

public partial class QueueView : UserControl
{
    public QueueView()
    {
        InitializeComponent();
    }

    private void QueueListBox_DoubleTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is QueueViewModel viewModel && QueueListBox.SelectedItem is Song song)
        {
            viewModel.PlaySongCommand.Execute(song);
        }
    }
}
