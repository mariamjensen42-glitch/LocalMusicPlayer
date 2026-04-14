using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalMusicPlayer.Models;
using LocalMusicPlayer.Services;

namespace LocalMusicPlayer.ViewModels;

public partial class MusicLibraryViewModel : ViewModelBase
{
    private readonly IMusicLibraryService _musicLibraryService;
    private readonly IPlaybackStateService _playbackStateService;
    private readonly IUserPlaylistService _userPlaylistService;
    private readonly IPlaylistService _playlistService;
    private readonly IPlayHistoryService _playHistoryService;

    public IMusicLibraryService Library => _musicLibraryService;

    [ObservableProperty] private string _searchQuery = string.Empty;

    [ObservableProperty] private int _selectedTabIndex;

    [ObservableProperty] private bool _isGridView = true;

    [ObservableProperty] private ObservableCollection<Song> _displayedSongs = new();

    [ObservableProperty] private ObservableCollection<AlbumGroup> _albums = new();

    [ObservableProperty] private ObservableCollection<ArtistGroup> _artists = new();

    [ObservableProperty] private ObservableCollection<FolderGroup> _folders = new();

    public Playlist? CurrentPlaylist { get; set; }

    partial void OnSearchQueryChanged(string value)
    {
        FilterSongs();
    }

    partial void OnSelectedTabIndexChanged(int value)
    {
        switch (value)
        {
            case 0:
                FilterSongs();
                break;
            case 1:
                GroupByAlbum();
                break;
            case 2:
                GroupByArtist();
                break;
            case 3:
                GroupByFolder();
                break;
        }
    }

    private void FilterSongs()
    {
        DisplayedSongs.Clear();
        var songs = string.IsNullOrWhiteSpace(SearchQuery)
            ? Library.Songs
            : Library.Songs.Where(s =>
                s.Title.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                s.Artist.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                s.Album.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));

        var trackNumber = 1;
        foreach (var song in songs)
        {
            song.TrackNumber = trackNumber++;
            DisplayedSongs.Add(song);
        }
    }

    private void GroupByAlbum()
    {
        Albums.Clear();
        var albumGroups = Library.Songs
            .GroupBy(s => s.Album)
            .Select(g => new AlbumGroup(g.Key, g.ToList()))
            .OrderBy(a => a.Name);

        foreach (var album in albumGroups)
        {
            Albums.Add(album);
        }
    }

    private void GroupByArtist()
    {
        Artists.Clear();
        var artistGroups = Library.Songs
            .GroupBy(s => s.Artist)
            .Select(g => new ArtistGroup(g.Key, g.ToList()))
            .OrderBy(a => a.Name);

        foreach (var artist in artistGroups)
        {
            Artists.Add(artist);
        }
    }

    private void GroupByFolder()
    {
        Folders.Clear();
        var folderGroups = Library.Songs
            .GroupBy(s => System.IO.Path.GetDirectoryName(s.FilePath) ?? "")
            .Select(g => new FolderGroup(g.Key, g.ToList()))
            .OrderBy(f => f.Name);

        foreach (var folder in folderGroups)
        {
            Folders.Add(folder);
        }
    }

    [RelayCommand]
    private void PlaySong(Song? song)
    {
        if (song == null || CurrentPlaylist == null)
            return;

        _playlistService.ClearPlaylist();
        _playlistService.SetCurrentPlaylist(CurrentPlaylist);
        _playlistService.AddSongToPlaylist(CurrentPlaylist, song);
        _playlistService.PlaySong(song);
        _playHistoryService.AddToHistory(song);
        _playbackStateService.Play(song);
    }

    [RelayCommand]
    private void PlayAll()
    {
        if (DisplayedSongs.Count == 0 || CurrentPlaylist == null)
            return;

        _playlistService.ClearPlaylist();
        foreach (var song in DisplayedSongs)
        {
            _playlistService.AddSongToPlaylist(CurrentPlaylist, song);
        }

        if (_playlistService.PlayNext())
        {
            var firstSong = _playlistService.CurrentSong;
            if (firstSong != null)
            {
                _playHistoryService.AddToHistory(firstSong);
                _playbackStateService.Play(firstSong);
            }
        }
    }

    [RelayCommand]
    private void ShufflePlayAll()
    {
        if (DisplayedSongs.Count == 0 || CurrentPlaylist == null)
            return;

        var shuffled = DisplayedSongs.OrderBy(_ => Guid.NewGuid()).ToList();

        _playlistService.ClearPlaylist();
        foreach (var song in shuffled)
        {
            _playlistService.AddSongToPlaylist(CurrentPlaylist, song);
        }

        _playbackStateService.PlaybackMode = PlaybackMode.Shuffle;

        if (_playlistService.PlayNext())
        {
            var firstSong = _playlistService.CurrentSong;
            if (firstSong != null)
            {
                _playHistoryService.AddToHistory(firstSong);
                _playbackStateService.Play(firstSong);
            }
        }
    }

    [RelayCommand]
    private async Task ToggleFavoriteAsync(Song? song)
    {
        if (song == null)
            return;

        if (song.IsFavorite)
            await _userPlaylistService.RemoveFromFavoritesAsync(song);
        else
            await _userPlaylistService.AddToFavoritesAsync(song);
    }

    [RelayCommand]
    private void AddToPlaylist(Song? song)
    {
        if (song == null || CurrentPlaylist == null)
            return;

        _playlistService.AddSongToPlaylist(CurrentPlaylist, song);
    }

    public MusicLibraryViewModel(
        IMusicLibraryService musicLibraryService,
        IPlaybackStateService playbackStateService,
        IUserPlaylistService userPlaylistService,
        IPlaylistService playlistService,
        IPlayHistoryService playHistoryService)
    {
        _musicLibraryService = musicLibraryService;
        _playbackStateService = playbackStateService;
        _userPlaylistService = userPlaylistService;
        _playlistService = playlistService;
        _playHistoryService = playHistoryService;

        _musicLibraryService.Songs.CollectionChanged += (s, e) => FilterSongs();
        FilterSongs();
    }
}
