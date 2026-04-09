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

    private void RemoveButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is QueueViewModel viewModel && sender is Button button)
        {
            var song = button.DataContext as Song;
            if (song != null)
            {
                var index = viewModel.QueueSongs.IndexOf(song);
                if (index >= 0)
                {
                    viewModel.RemoveSongCommand.Execute(index);
                }
            }
        }
    }

    private void QueueListBox_DoubleTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is QueueViewModel viewModel && QueueListBox.SelectedItem is Song song)
        {
            viewModel.PlaySongCommand.Execute(song);
        }
    }
}
