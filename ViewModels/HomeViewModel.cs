using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalMusicPlayer.Models;
using LocalMusicPlayer.Services;

namespace LocalMusicPlayer.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    private readonly IMusicLibraryService _musicLibraryService;
    private readonly IPlaylistService _playlistService;
    private readonly IPlaybackStateService _playbackStateService;
    private readonly IStatisticsService _statisticsService;
    private readonly IPlayHistoryService _playHistoryService;
    private readonly IUserPlaylistService _userPlaylistService;
    private readonly IDialogService _dialogService;

    [ObservableProperty] private string _searchText = string.Empty;

    [ObservableProperty] private string _libraryStats = string.Empty;

    [ObservableProperty] private string? _coverArtPath;

    [ObservableProperty] private bool _isGridView;

    public bool IsGridEmptyVisible => IsGridView && Songs.Count == 0;

    public ObservableCollection<Song> Songs => _musicLibraryService.FilteredSongs;

    public Song? CurrentSong => _playbackStateService.CurrentSong;

    public bool IsPlaying => _playbackStateService.IsPlaying;

    public Playlist? CurrentPlaylist { get; set; }

    public HomeViewModel(
        IMusicLibraryService musicLibraryService,
        IPlaylistService playlistService,
        IPlaybackStateService playbackStateService,
        IStatisticsService statisticsService,
        IPlayHistoryService playHistoryService,
        IUserPlaylistService userPlaylistService,
        IDialogService dialogService)
    {
        _musicLibraryService = musicLibraryService;
        _playlistService = playlistService;
        _playbackStateService = playbackStateService;
        _statisticsService = statisticsService;
        _playHistoryService = playHistoryService;
        _userPlaylistService = userPlaylistService;
        _dialogService = dialogService;

        UpdateLibraryStats();

        SubscribeEvent(
            () => _playbackStateService.PlaybackStateChanged += OnPlaybackStateChanged,
            () => _playbackStateService.PlaybackStateChanged -= OnPlaybackStateChanged);

        SubscribeEvent(
            () => _playbackStateService.CurrentSongChanged += OnCurrentSongChanged,
            () => _playbackStateService.CurrentSongChanged -= OnCurrentSongChanged);

        SubscribeEvent(
            () => _musicLibraryService.Songs.CollectionChanged += OnSongsCollectionChanged,
            () => _musicLibraryService.Songs.CollectionChanged -= OnSongsCollectionChanged);
    }

    partial void OnSearchTextChanged(string value)
    {
        FilterSongs();
    }

    partial void OnIsGridViewChanged(bool value)
    {
        OnPropertyChanged(nameof(IsGridEmptyVisible));
    }

    private void FilterSongs()
    {
        _musicLibraryService.FilteredSongs.Clear();
        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? _musicLibraryService.Songs
            : _musicLibraryService.Songs.Where(s =>
                s.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                s.Artist.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                s.Album.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        var trackNumber = 1;
        foreach (var song in filtered)
        {
            song.TrackNumber = trackNumber++;
            _musicLibraryService.FilteredSongs.Add(song);
        }
    }

    private void UpdateLibraryStats()
    {
        var count = Songs.Count;
        LibraryStats = $"本地: {count} 首";
        CoverArtPath = _musicLibraryService.Songs.FirstOrDefault()?.AlbumArtPath;
    }

    [RelayCommand]
    private void ToggleView()
    {
        IsGridView = !IsGridView;
    }

    [RelayCommand]
    private void PlayAll()
    {
        if (Songs.Count == 0 || CurrentPlaylist == null)
            return;

        _playlistService.ClearPlaylist();
        foreach (var song in Songs)
        {
            _playlistService.AddSongToPlaylist(CurrentPlaylist, song);
        }

        if (_playlistService.PlayNext())
        {
            var song = _playlistService.CurrentSong;
            if (song != null)
            {
                _statisticsService.RecordPlayStart(song);
                _playHistoryService.AddToHistory(song);
                _playbackStateService.Play(song);
            }
        }
    }

    [RelayCommand]
    private void ShufflePlay()
    {
        if (Songs.Count == 0 || CurrentPlaylist == null)
            return;

        var shuffled = Songs.OrderBy(_ => Guid.NewGuid()).ToList();

        _playlistService.ClearPlaylist();
        foreach (var song in shuffled)
        {
            _playlistService.AddSongToPlaylist(CurrentPlaylist, song);
        }

        _playbackStateService.PlaybackMode = PlaybackMode.Shuffle;

        if (_playlistService.PlayNext())
        {
            var song = _playlistService.CurrentSong;
            if (song != null)
            {
                _statisticsService.RecordPlayStart(song);
                _playHistoryService.AddToHistory(song);
                _playbackStateService.Play(song);
            }
        }
    }

    [RelayCommand]
    private void PlaySong(string filePath)
    {
        var song = Songs.FirstOrDefault(s => s.FilePath == filePath);
        if (song != null && CurrentPlaylist != null)
        {
            _playlistService.ClearPlaylist();
            _playlistService.AddSongToPlaylist(CurrentPlaylist, song);
            _playlistService.SetCurrentPlaylist(CurrentPlaylist);
            _playlistService.PlaySong(song);

            _statisticsService.RecordPlayStart(song);
            _playHistoryService.AddToHistory(song);
            _playbackStateService.Play(song);
        }
    }

    [RelayCommand]
    private async Task ToggleFavoriteAsync(string filePath)
    {
        var song = _musicLibraryService.Songs.FirstOrDefault(s => s.FilePath == filePath);
        if (song != null)
        {
            if (song.IsFavorite)
                await _userPlaylistService.RemoveFromFavoritesAsync(song);
            else
                await _userPlaylistService.AddToFavoritesAsync(song);
        }
    }

    [RelayCommand]
    private async Task AddToPlaylistAsync(string filePath)
    {
        var song = _musicLibraryService.Songs.FirstOrDefault(s => s.FilePath == filePath);
        if (song != null)
        {
            await _dialogService.ShowAddToPlaylistDialogAsync(song);
        }
    }

    [RelayCommand]
    private async Task EditSongMetadataAsync(Song song)
    {
        await _dialogService.ShowMetadataEditorDialogAsync(song, () => { FilterSongs(); });
    }

    [RelayCommand]
    private async Task BatchEditMetadataAsync(System.Collections.IList selectedItems)
    {
        var songs = selectedItems.OfType<Song>().ToList();
        if (songs.Count < 2)
        {
            await _dialogService.ShowMessageDialogAsync("Batch Edit", "Please select at least 2 songs to batch edit.");
            return;
        }

        await _dialogService.ShowBatchMetadataEditorDialogAsync(songs, () => { FilterSongs(); });
    }

    private void OnPlaybackStateChanged(object? sender, PlayState state)
    {
        OnPropertyChanged(nameof(IsPlaying));
    }

    private void OnCurrentSongChanged(object? sender, Song? song)
    {
        OnPropertyChanged(nameof(CurrentSong));
    }

    private void OnSongsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateLibraryStats();
        FilterSongs();
        OnPropertyChanged(nameof(IsGridEmptyVisible));
    }

    protected override void DisposeCore()
    {
        base.DisposeCore();
    }
}
