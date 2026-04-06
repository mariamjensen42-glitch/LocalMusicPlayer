using Avalonia.Controls;
using Avalonia.Input;
using LocalMusicPlayer.Models;
using LocalMusicPlayer.ViewModels;

namespace LocalMusicPlayer.Views;

public partial class LibraryCategoryView : UserControl
{
    public LibraryCategoryView()
    {
        InitializeComponent();
    }

    private void ArtistItem_Tapped(object? sender, TappedEventArgs e)
    {
        if (sender is Border border && border.DataContext is ArtistGroup artist)
        {
            if (DataContext is LibraryCategoryViewModel vm)
            {
                vm.SelectedItem = artist;
            }
        }
    }

    private void AlbumItem_Tapped(object? sender, TappedEventArgs e)
    {
        if (sender is Border border && border.DataContext is AlbumGroup album)
        {
            if (DataContext is LibraryCategoryViewModel vm)
            {
                vm.SelectedItem = album;
            }
        }
    }

    private void FolderItem_Tapped(object? sender, TappedEventArgs e)
    {
        if (sender is Border border && border.DataContext is FolderGroup folder)
        {
            if (DataContext is LibraryCategoryViewModel vm)
            {
                vm.SelectedItem = folder;
            }
        }
    }
}