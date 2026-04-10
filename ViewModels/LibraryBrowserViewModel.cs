using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalMusicPlayer.Models;
using LocalMusicPlayer.Services;

namespace LocalMusicPlayer.ViewModels;

public enum BrowserCategory
{
    Songs,
    Artists,
    Albums,
    Genres,
    Folders
}

public partial class LibraryBrowserViewModel : ViewModelBase
{
    private readonly IMusicLibraryService _musicLibraryService;
    private readonly IMusicPlayerService _musicPlayerService;
    private readonly IPlaylistService _playlistService;
    private readonly INavigationService _navigationService;

    [ObservableProperty] private BrowserCategory _currentCategory = BrowserCategory.Songs;

    [ObservableProperty] private ObservableCollection<Song> _songs = [];

    [ObservableProperty] private ObservableCollection<ArtistInfo> _artists = [];

    [ObservableProperty] private ObservableCollection<AlbumInfo> _albums = [];

    [ObservableProperty] private ObservableCollection<GenreInfo> _genres = [];

    [ObservableProperty] private ObservableCollection<FolderNode> _folders = [];

    [ObservableProperty] private object? _selectedItem;

    [ObservableProperty] private string _searchText = string.Empty;

    public LibraryBrowserViewModel(
        IMusicLibraryService musicLibraryService,
        IMusicPlayerService musicPlayerService,
        IPlaylistService playlistService,
        INavigationService navigationService)
    {
        _musicLibraryService = musicLibraryService;
        _musicPlayerService = musicPlayerService;
        _playlistService = playlistService;
        _navigationService = navigationService;

        _ = LoadDataAsync();
    }

    [RelayCommand]
    private void SwitchCategory(BrowserCategory category)
    {
        CurrentCategory = category;
        RefreshCurrentView();
    }

    [RelayCommand]
    private void SelectItem(object? item)
    {
        SelectedItem = item;

        switch (item)
        {
            case ArtistInfo artist:
                Songs = new ObservableCollection<Song>(_musicLibraryService.GetSongsByArtist(artist.Name));
                CurrentCategory = BrowserCategory.Songs;
                break;
            case AlbumInfo album:
                Songs = new ObservableCollection<Song>(_musicLibraryService.GetSongsByAlbum(album.Title));
                CurrentCategory = BrowserCategory.Songs;
                break;
            case GenreInfo genre:
                FilterSongsByGenre(genre.Name);
                break;
            case FolderNode folder:
                ShowFolderSongs(folder);
                break;
            case Song song:
                PlaySong(song);
                break;
        }
    }

    [RelayCommand]
    private void PlaySelected()
    {
        if (SelectedItem is Song song)
        {
            PlaySong(song);
        }
    }

    [RelayCommand]
    private void AddToQueue()
    {
        if (SelectedItem is Song song)
        {
            _playlistService.AddSongToPlaylist(_playlistService.CurrentPlaylist!, song);
        }
    }

    [RelayCommand]
    private void Search()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            RefreshCurrentView();
            return;
        }

        var searchLower = SearchText.ToLowerInvariant();

        switch (CurrentCategory)
        {
            case BrowserCategory.Songs:
                Songs = new ObservableCollection<Song>(
                    _musicLibraryService.Songs.Where(s =>
                        s.Title.ToLowerInvariant().Contains(searchLower) ||
                        s.Artist.ToLowerInvariant().Contains(searchLower) ||
                        s.Album.ToLowerInvariant().Contains(searchLower)));
                break;
            case BrowserCategory.Artists:
                var artistList = _musicLibraryService.GetArtists();
                Artists = new ObservableCollection<ArtistInfo>(
                    artistList.Where(a => a.Name.ToLowerInvariant().Contains(searchLower)));
                break;
            case BrowserCategory.Albums:
                var albumList = _musicLibraryService.GetAlbums();
                Albums = new ObservableCollection<AlbumInfo>(
                    albumList.Where(a =>
                        a.Title.ToLowerInvariant().Contains(searchLower) ||
                        a.Artist.ToLowerInvariant().Contains(searchLower)));
                break;
            case BrowserCategory.Genres:
                var genreList = _musicLibraryService.GetGenres();
                Genres = new ObservableCollection<GenreInfo>(
                    genreList.Where(g => g.Name.ToLowerInvariant().Contains(searchLower)));
                break;
        }
    }

    [RelayCommand]
    private void ClearSearch()
    {
        SearchText = string.Empty;
        RefreshCurrentView();
    }

    private async Task LoadDataAsync()
    {
        await Task.Run(() =>
        {
            var artistList = _musicLibraryService.GetArtists();
            var albumList = _musicLibraryService.GetAlbums();
            var genreList = _musicLibraryService.GetGenres();
            var folderList = _musicLibraryService.GetFolderStructure();

            Artists = new ObservableCollection<ArtistInfo>(artistList);
            Albums = new ObservableCollection<AlbumInfo>(albumList);
            Genres = new ObservableCollection<GenreInfo>(genreList);
            Folders = new ObservableCollection<FolderNode>(folderList);
        });

        RefreshCurrentView();
    }

    private void RefreshCurrentView()
    {
        switch (CurrentCategory)
        {
            case BrowserCategory.Songs:
                Songs = new ObservableCollection<Song>(_musicLibraryService.Songs);
                break;
            case BrowserCategory.Artists:
                Artists = new ObservableCollection<ArtistInfo>(_musicLibraryService.GetArtists());
                break;
            case BrowserCategory.Albums:
                Albums = new ObservableCollection<AlbumInfo>(_musicLibraryService.GetAlbums());
                break;
            case BrowserCategory.Genres:
                Genres = new ObservableCollection<GenreInfo>(_musicLibraryService.GetGenres());
                break;
            case BrowserCategory.Folders:
                Folders = new ObservableCollection<FolderNode>(_musicLibraryService.GetFolderStructure());
                break;
        }
    }

    private void FilterSongsByGenre(string genre)
    {
        var songs = _musicLibraryService.GetSongsByGenre(genre);
        Songs = new ObservableCollection<Song>(songs);
        CurrentCategory = BrowserCategory.Songs;
    }

    private void ShowFolderSongs(FolderNode folder)
    {
        Songs = new ObservableCollection<Song>(folder.Songs);
        CurrentCategory = BrowserCategory.Songs;
    }

    private void PlaySong(Song song)
    {
        _musicPlayerService.Play(song);
    }

    partial void OnSearchTextChanged(string value)
    {
        Search();
    }
}
