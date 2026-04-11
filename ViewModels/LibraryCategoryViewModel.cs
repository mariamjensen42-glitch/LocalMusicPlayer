using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalMusicPlayer.Models;
using LocalMusicPlayer.Services;

namespace LocalMusicPlayer.ViewModels;

public partial class LibraryCategoryViewModel : ViewModelBase
{
    private readonly ILibraryCategoryService _categoryService;
    private readonly IMusicPlayerService _musicPlayerService;
    private readonly IPlaylistService _playlistService;
    private readonly IStatisticsService _statisticsService;

    [ObservableProperty] private LibraryCategory _currentCategory = LibraryCategory.Songs;

    [ObservableProperty] private object? _selectedItem;

    public Action<ArtistGroup>? OnNavigateToArtistDetail { get; set; }
    public Action<AlbumGroup>? OnNavigateToAlbumDetail { get; set; }

    partial void OnCurrentCategoryChanged(LibraryCategory value)
    {
        OnPropertyChanged(nameof(ShowArtistGroups));
        OnPropertyChanged(nameof(ShowAlbumGroups));
        OnPropertyChanged(nameof(ShowFolderGroups));
        OnPropertyChanged(nameof(ShowFavorites));
        _ = RefreshItemsAsync();
    }

    partial void OnSelectedItemChanged(object? value)
    {
        UpdateSelectedGroupSongs();
    }

    public void ShowFavoritesOnly()
    {
        CurrentCategory = LibraryCategory.Favorites;
    }

    public bool ShowArtistGroups => CurrentCategory == LibraryCategory.Artists;
    public bool ShowAlbumGroups => CurrentCategory == LibraryCategory.Albums;
    public bool ShowFolderGroups => CurrentCategory == LibraryCategory.Folders;
    public bool ShowFavorites => CurrentCategory == LibraryCategory.Favorites;

    public ObservableCollection<ArtistGroup> ArtistGroups { get; } = new();
    public ObservableCollection<AlbumGroup> AlbumGroups { get; } = new();
    public ObservableCollection<FolderGroup> FolderGroups { get; } = new();
    public ObservableCollection<Song> FavoriteSongs { get; } = new();
    public ObservableCollection<Song> SelectedGroupSongs { get; } = new();

    [RelayCommand]
    private void SwitchCategory(LibraryCategory category)
    {
        CurrentCategory = category;
        _categoryService.CurrentCategory = category;
    }

    [RelayCommand]
    private void SelectItem(object? item)
    {
        if (item is ArtistGroup artistGroup)
        {
            SelectedItem = artistGroup;
            OnNavigateToArtistDetail?.Invoke(artistGroup);
        }
        else if (item is AlbumGroup albumGroup)
        {
            SelectedItem = albumGroup;
            OnNavigateToAlbumDetail?.Invoke(albumGroup);
        }
        else if (item is FolderGroup folderGroup)
        {
            SelectedItem = folderGroup;
        }
    }

    [RelayCommand]
    private void PlaySong(string path)
    {
        var song = SelectedGroupSongs.FirstOrDefault(s => s.FilePath == path);
        if (song != null)
        {
            PlaySongInternal(song);
        }
    }

    [RelayCommand]
    private void PlayAllSelected()
    {
        if (SelectedGroupSongs.Count == 0) return;

        var playlist = _playlistService.CreatePlaylist("TempPlay");
        _playlistService.SetCurrentPlaylist(playlist);

        foreach (var song in SelectedGroupSongs)
        {
            _playlistService.AddSongToPlaylist(playlist, song);
        }

        _playlistService.PlayNext();
        if (_playlistService.CurrentSong != null)
        {
            PlaySongInternal(_playlistService.CurrentSong);
        }
    }

    public LibraryCategoryViewModel(
        ILibraryCategoryService categoryService,
        IMusicPlayerService musicPlayerService,
        IPlaylistService playlistService,
        IStatisticsService statisticsService)
    {
        _categoryService = categoryService;
        _musicPlayerService = musicPlayerService;
        _playlistService = playlistService;
        _statisticsService = statisticsService;

        _categoryService.CategoryChanged += OnCategoryChanged;

        _ = RefreshItemsAsync();
    }

    private void OnCategoryChanged(object? sender, LibraryCategory category)
    {
        if (CurrentCategory != category)
        {
            CurrentCategory = category;
        }
    }

    protected override void DisposeCore()
    {
        _categoryService.CategoryChanged -= OnCategoryChanged;
        base.DisposeCore();
    }

    private async Task RefreshItemsAsync()
    {
        ArtistGroups.Clear();
        AlbumGroups.Clear();
        FolderGroups.Clear();
        FavoriteSongs.Clear();
        SelectedGroupSongs.Clear();

        switch (CurrentCategory)
        {
            case LibraryCategory.Artists:
                foreach (var group in _categoryService.GetArtistGroups())
                    ArtistGroups.Add(group);
                break;
            case LibraryCategory.Albums:
                foreach (var group in _categoryService.GetAlbumGroups())
                    AlbumGroups.Add(group);
                break;
            case LibraryCategory.Folders:
                foreach (var group in _categoryService.GetFolderGroups())
                    FolderGroups.Add(group);
                break;
            case LibraryCategory.Favorites:
                var favSongs = await _categoryService.GetFavoriteSongsAsync();
                foreach (var song in favSongs)
                    FavoriteSongs.Add(song);
                break;
        }
    }

    private void UpdateSelectedGroupSongs()
    {
        SelectedGroupSongs.Clear();

        switch (CurrentCategory)
        {
            case LibraryCategory.Artists:
                if (SelectedItem is ArtistGroup artistGroup)
                    foreach (var song in artistGroup.Songs)
                        SelectedGroupSongs.Add(song);
                break;
            case LibraryCategory.Albums:
                if (SelectedItem is AlbumGroup albumGroup)
                    foreach (var song in albumGroup.Songs)
                        SelectedGroupSongs.Add(song);
                break;
            case LibraryCategory.Folders:
                if (SelectedItem is FolderGroup folderGroup)
                    foreach (var song in folderGroup.Songs)
                        SelectedGroupSongs.Add(song);
                break;
            case LibraryCategory.Favorites:
                foreach (var song in FavoriteSongs)
                    SelectedGroupSongs.Add(song);
                break;
        }
    }

    private void PlaySongInternal(Song song)
    {
        _statisticsService.RecordPlayStart(song);
        _musicPlayerService.Play(song);
    }
}
