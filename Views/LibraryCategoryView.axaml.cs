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
        if (DataContext is not LibraryCategoryViewModel vm) return;
        if (vm.OnNavigateToArtistDetail == null) return;
        if (sender is Border border && border.DataContext is ArtistGroup artist)
        {
            vm.OnNavigateToArtistDetail.Invoke(artist);
        }
    }

    private void AlbumItem_Tapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is not LibraryCategoryViewModel vm) return;
        if (vm.OnNavigateToAlbumDetail == null) return;
        if (sender is Border border && border.DataContext is AlbumGroup album)
        {
            vm.OnNavigateToAlbumDetail.Invoke(album);
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