using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalMusicPlayer.Models;
using LocalMusicPlayer.Services;

namespace LocalMusicPlayer.ViewModels;

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
        INavigationService navigationService,
        BrowserCategory initialCategory = BrowserCategory.Songs)
    {
        _musicLibraryService = musicLibraryService;
        _musicPlayerService = musicPlayerService;
        _playlistService = playlistService;
        _navigationService = navigationService;
        _currentCategory = initialCategory;

        _ = LoadDataAsync().ContinueWith(_ => { }, TaskContinuationOptions.OnlyOnFaulted);
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
                Songs.Clear();
                foreach (var song in _musicLibraryService.GetSongsByArtist(artist.Name))
                    Songs.Add(song);
                CurrentCategory = BrowserCategory.Songs;
                break;
            case AlbumInfo album:
                Songs.Clear();
                foreach (var song in _musicLibraryService.GetSongsByAlbum(album.Title))
                    Songs.Add(song);
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
                Songs.Clear();
                foreach (var song in _musicLibraryService.Songs.Where(s =>
                             s.Title.ToLowerInvariant().Contains(searchLower) ||
                             s.Artist.ToLowerInvariant().Contains(searchLower) ||
                             s.Album.ToLowerInvariant().Contains(searchLower)))
                    Songs.Add(song);
                break;
            case BrowserCategory.Artists:
                Artists.Clear();
                foreach (var artist in _musicLibraryService.GetArtists()
                             .Where(a => a.Name.ToLowerInvariant().Contains(searchLower)))
                    Artists.Add(artist);
                break;
            case BrowserCategory.Albums:
                Albums.Clear();
                foreach (var album in _musicLibraryService.GetAlbums()
                             .Where(a =>
                                 a.Title.ToLowerInvariant().Contains(searchLower) ||
                                 a.Artist.ToLowerInvariant().Contains(searchLower)))
                    Albums.Add(album);
                break;
            case BrowserCategory.Genres:
                Genres.Clear();
                foreach (var genre in _musicLibraryService.GetGenres()
                             .Where(g => g.Name.ToLowerInvariant().Contains(searchLower)))
                    Genres.Add(genre);
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

            Artists.Clear();
            foreach (var artist in artistList)
                Artists.Add(artist);
            Albums.Clear();
            foreach (var album in albumList)
                Albums.Add(album);
            Genres.Clear();
            foreach (var genre in genreList)
                Genres.Add(genre);
            Folders.Clear();
            foreach (var folder in folderList)
                Folders.Add(folder);
        });

        RefreshCurrentView();
    }

    private void RefreshCurrentView()
    {
        switch (CurrentCategory)
        {
            case BrowserCategory.Songs:
                Songs.Clear();
                foreach (var song in _musicLibraryService.Songs)
                    Songs.Add(song);
                break;
            case BrowserCategory.Artists:
                Artists.Clear();
                foreach (var artist in _musicLibraryService.GetArtists())
                    Artists.Add(artist);
                break;
            case BrowserCategory.Albums:
                Albums.Clear();
                foreach (var album in _musicLibraryService.GetAlbums())
                    Albums.Add(album);
                break;
            case BrowserCategory.Genres:
                Genres.Clear();
                foreach (var genre in _musicLibraryService.GetGenres())
                    Genres.Add(genre);
                break;
            case BrowserCategory.Folders:
                Folders.Clear();
                foreach (var folder in _musicLibraryService.GetFolderStructure())
                    Folders.Add(folder);
                break;
        }
    }

    private void FilterSongsByGenre(string genre)
    {
        Songs.Clear();
        foreach (var song in _musicLibraryService.GetSongsByGenre(genre))
            Songs.Add(song);
        CurrentCategory = BrowserCategory.Songs;
    }

    private void ShowFolderSongs(FolderNode folder)
    {
        Songs.Clear();
        foreach (var song in folder.Songs)
            Songs.Add(song);
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
