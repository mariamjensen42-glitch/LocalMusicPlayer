using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using LocalMusicPlayer.Models;
using LocalMusicPlayer.ViewModels;

namespace LocalMusicPlayer.Views;

public partial class LibraryBrowserView : UserControl
{
    public LibraryBrowserView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void SongItem_Tapped(object? sender, TappedEventArgs e)
    {
        if (sender is Border border && border.DataContext is Song song)
        {
            if (DataContext is LibraryBrowserViewModel vm)
            {
                vm.SelectItemCommand.Execute(song);
            }
        }
    }

    private void ArtistItem_Tapped(object? sender, TappedEventArgs e)
    {
        if (sender is Border border && border.DataContext is ArtistInfo artist)
        {
            if (DataContext is LibraryBrowserViewModel vm)
            {
                vm.SelectItemCommand.Execute(artist);
            }
        }
    }

    private void AlbumItem_Tapped(object? sender, TappedEventArgs e)
    {
        if (sender is Border border && border.DataContext is AlbumInfo album)
        {
            if (DataContext is LibraryBrowserViewModel vm)
            {
                vm.SelectItemCommand.Execute(album);
            }
        }
    }

    private void GenreItem_Tapped(object? sender, TappedEventArgs e)
    {
        if (sender is Border border && border.DataContext is GenreInfo genre)
        {
            if (DataContext is LibraryBrowserViewModel vm)
            {
                vm.SelectItemCommand.Execute(genre);
            }
        }
    }

    private void FolderItem_Tapped(object? sender, TappedEventArgs e)
    {
        if (sender is Border border && border.DataContext is FolderNode folder)
        {
            if (DataContext is LibraryBrowserViewModel vm)
            {
                vm.SelectItemCommand.Execute(folder);
            }
        }
    }
}
