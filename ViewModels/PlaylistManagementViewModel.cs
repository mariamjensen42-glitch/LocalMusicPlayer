using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalMusicPlayer.Models;
using LocalMusicPlayer.Services;

namespace LocalMusicPlayer.ViewModels;

public partial class PlaylistManagementViewModel : ViewModelBase
{
    private readonly IUserPlaylistService _playlistService;
    private readonly IMusicPlayerService _musicPlayerService;
    private readonly IPlaylistService _playbackService;
    private readonly IStatisticsService _statisticsService;
    private readonly IMusicLibraryService _libraryService;
    private readonly IDialogService _dialogService;

    [ObservableProperty] private UserPlaylist? _selectedPlaylist;

    [ObservableProperty] private string _newPlaylistName = string.Empty;

    [ObservableProperty] private bool _isCreatingPlaylist;

    [ObservableProperty] private ObservableCollection<Song> _playlistSongs = new();

    public ObservableCollection<UserPlaylist> UserPlaylists => _playlistService.UserPlaylists;

    public string? CoverArtPath => SelectedPlaylist?.CoverArtPath ?? PlaylistSongs.FirstOrDefault()?.AlbumArtPath;

    public string Subtitle => PlaylistSongs.Count > 0 ? $"{PlaylistSongs.Count} 首歌曲" : "暂无歌曲";

    public bool CanDeletePlaylist => SelectedPlaylist != null &&
                                     SelectedPlaylist.Id != "favorites";

    public bool CanRenamePlaylist => SelectedPlaylist != null &&
                                     SelectedPlaylist.Id != "favorites";

    partial void OnSelectedPlaylistChanged(UserPlaylist? value)
    {
        OnPropertyChanged(nameof(CanDeletePlaylist));
        OnPropertyChanged(nameof(CanRenamePlaylist));
        OnPropertyChanged(nameof(CoverArtPath));
        OnPropertyChanged(nameof(Subtitle));
        UpdatePlaylistSongs(value);
    }

    [RelayCommand]
    private void SelectPlaylist(UserPlaylist playlist)
    {
        SelectedPlaylist = playlist;
    }

    public void SetSelectedPlaylist(UserPlaylist playlist)
    {
        SelectPlaylist(playlist);
    }

    [RelayCommand]
    private async Task CreatePlaylistAsync()
    {
        var name = await _dialogService.ShowInputDialogAsync("New Playlist", "");
        if (!string.IsNullOrWhiteSpace(name))
        {
            await _playlistService.CreatePlaylistAsync(name);
        }
    }

    [RelayCommand]
    private async Task DeletePlaylistAsync()
    {
        if (SelectedPlaylist != null && CanDeletePlaylist)
        {
            await _playlistService.DeletePlaylistAsync(SelectedPlaylist.Id);
            SelectedPlaylist = null;
        }
    }

    [RelayCommand]
    private async Task RenamePlaylistAsync()
    {
        if (SelectedPlaylist != null && CanRenamePlaylist)
        {
            var newName = await _dialogService.ShowInputDialogAsync("Rename Playlist", SelectedPlaylist.Name);
            if (!string.IsNullOrWhiteSpace(newName))
            {
                await _playlistService.RenamePlaylistAsync(SelectedPlaylist.Id, newName);
            }
        }
    }

    [RelayCommand]
    private async Task ExportPlaylistAsync()
    {
        if (SelectedPlaylist == null)
            return;

        var filePath = await _dialogService.ShowSaveFileDialogAsync("ExportPlaylist", ["JSON Files (*.json)|*.json"]);
        if (!string.IsNullOrEmpty(filePath))
        {
            await _playlistService.ExportPlaylistAsync(SelectedPlaylist.Id, filePath);
        }
    }

    [RelayCommand]
    private async Task ImportPlaylistAsync()
    {
        var filePath = await _dialogService.ShowOpenFileDialogAsync("ImportPlaylist", ["JSON Files (*.json)|*.json"]);
        if (!string.IsNullOrEmpty(filePath))
        {
            await _playlistService.ImportPlaylistAsync(filePath);
        }
    }

    [RelayCommand]
    private void PlaySong(string path)
    {
        var song = PlaylistSongs.FirstOrDefault(s => s.FilePath == path);
        if (song != null)
        {
            _statisticsService.RecordPlayStart(song);
            _musicPlayerService.Play(song);
        }
    }

    [RelayCommand]
    private async Task RemoveSongAsync(string path)
    {
        if (SelectedPlaylist != null)
        {
            await _playlistService.RemoveSongFromPlaylistAsync(SelectedPlaylist.Id, path);
            UpdatePlaylistSongs(SelectedPlaylist);
        }
    }

    [RelayCommand]
    private void PlayAll()
    {
        if (PlaylistSongs.Count == 0) return;

        var currentPlaylist = _playbackService.CreatePlaylist("TempPlay");
        _playbackService.SetCurrentPlaylist(currentPlaylist);
        _playbackService.ClearPlaylist();

        foreach (var song in PlaylistSongs)
        {
            _playbackService.AddSongToPlaylist(currentPlaylist, song);
        }

        _playbackService.PlayNext();
        if (_playbackService.CurrentSong != null)
        {
            _statisticsService.RecordPlayStart(_playbackService.CurrentSong);
            _musicPlayerService.Play(_playbackService.CurrentSong);
        }
    }

    [RelayCommand]
    private void ShufflePlay()
    {
        if (PlaylistSongs.Count == 0) return;

        var currentPlaylist = _playbackService.CreatePlaylist("TempPlay");
        _playbackService.SetCurrentPlaylist(currentPlaylist);
        _playbackService.ClearPlaylist();

        var random = new Random();
        var shuffled = PlaylistSongs.OrderBy(_ => random.Next()).ToList();

        foreach (var song in shuffled)
        {
            _playbackService.AddSongToPlaylist(currentPlaylist, song);
        }

        _playbackService.PlayNext();
        if (_playbackService.CurrentSong != null)
        {
            _statisticsService.RecordPlayStart(_playbackService.CurrentSong);
            _musicPlayerService.Play(_playbackService.CurrentSong);
        }
    }

    [RelayCommand]
    private async Task MoveSongAsync((int OldIndex, int NewIndex) param)
    {
        if (SelectedPlaylist != null)
        {
            await _playlistService.MoveSongInPlaylistAsync(SelectedPlaylist.Id, param.OldIndex, param.NewIndex);
            UpdatePlaylistSongs(SelectedPlaylist);
        }
    }

    [RelayCommand]
    private async Task EditSongMetadataAsync(Song song)
    {
        await _dialogService.ShowMetadataEditorDialogAsync(song, () => { UpdatePlaylistSongs(SelectedPlaylist); });
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

        await _dialogService.ShowBatchMetadataEditorDialogAsync(songs,
            () => { UpdatePlaylistSongs(SelectedPlaylist); });
    }

    [RelayCommand]
    private async Task AddToPlaylistAsync(string filePath)
    {
        var song = PlaylistSongs.FirstOrDefault(s => s.FilePath == filePath);
        if (song != null)
        {
            await _dialogService.ShowAddToPlaylistDialogAsync(song);
        }
    }

    public PlaylistManagementViewModel(
        IUserPlaylistService playlistService,
        IMusicPlayerService musicPlayerService,
        IPlaylistService playbackService,
        IStatisticsService statisticsService,
        IMusicLibraryService libraryService,
        IDialogService dialogService)
    {
        _playlistService = playlistService;
        _musicPlayerService = musicPlayerService;
        _playbackService = playbackService;
        _statisticsService = statisticsService;
        _libraryService = libraryService;
        _dialogService = dialogService;

        EnsureFavoritesPlaylist();

        _playlistService.PlaylistsChanged += OnPlaylistsChanged;
    }

    private void OnPlaylistsChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(UserPlaylists));
        if (SelectedPlaylist != null)
        {
            var updated = _playlistService.UserPlaylists.FirstOrDefault(p => p.Id == SelectedPlaylist.Id);
            if (updated != null)
                UpdatePlaylistSongs(updated);
        }
    }

    protected override void DisposeCore()
    {
        _playlistService.PlaylistsChanged -= OnPlaylistsChanged;
        base.DisposeCore();
    }

    private void UpdatePlaylistSongs(UserPlaylist? playlist)
    {
        PlaylistSongs.Clear();
        if (playlist != null)
        {
            foreach (var song in _playlistService.GetPlaylistSongs(playlist.Id))
            {
                PlaylistSongs.Add(song);
            }
        }
    }

    private void EnsureFavoritesPlaylist()
    {
        var favorites = _playlistService.UserPlaylists.FirstOrDefault(p => p.Id == "favorites");
        if (favorites == null)
        {
            favorites = new UserPlaylist
            {
                Id = "favorites",
                Name = "Favorites",
                SongFilePaths = _libraryService.Songs.Where(s => s.IsFavorite).Select(s => s.FilePath).ToList()
            };
            _playlistService.UserPlaylists.Insert(0, favorites);
        }
    }

    public async Task AddSongToSelectedPlaylistAsync(Song song)
    {
        if (SelectedPlaylist != null)
        {
            await _playlistService.AddSongToPlaylistAsync(SelectedPlaylist.Id, song);
            UpdatePlaylistSongs(SelectedPlaylist);
        }
    }
}
