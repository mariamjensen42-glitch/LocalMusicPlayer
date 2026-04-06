using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using LocalMusicPlayer.Models;
using LocalMusicPlayer.ViewModels;

namespace LocalMusicPlayer.Views;

public partial class PlaylistManagementView : UserControl
{
    public PlaylistManagementView()
    {
        InitializeComponent();
    }

    private void PlaylistItem_Tapped(object? sender, TappedEventArgs e)
    {
        if (sender is Border border && border.DataContext is UserPlaylist playlist)
        {
            if (DataContext is PlaylistManagementViewModel vm)
            {
                vm.SelectedPlaylist = playlist;
            }
        }
    }
}