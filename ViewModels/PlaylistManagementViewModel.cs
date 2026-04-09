using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalMusicPlayer.Models;
using LocalMusicPlayer.Services;
using LocalMusicPlayer.Views;

namespace LocalMusicPlayer.ViewModels;

public partial class PlaylistManagementViewModel : ViewModelBase
{
    private readonly IUserPlaylistService _playlistService;
    private readonly IMusicPlayerService _musicPlayerService;
    private readonly IPlaylistService _playbackService;
    private readonly IStatisticsService _statisticsService;
    private readonly IMusicLibraryService _libraryService;
    private readonly IDialogService _dialogService;

    private UserPlaylist? _selectedPlaylist;
    private string _newPlaylistName = string.Empty;
    private bool _isCreatingPlaylist;
    private ObservableCollection<Song> _playlistSongs = new();

    public ObservableCollection<UserPlaylist> UserPlaylists => _playlistService.UserPlaylists;

    public UserPlaylist? SelectedPlaylist
    {
        get => _selectedPlaylist;
        set
        {
            if (SetProperty(ref _selectedPlaylist, value))
            {
                OnPropertyChanged(nameof(PlaylistSongs));
                OnPropertyChanged(nameof(CanDeletePlaylist));
                OnPropertyChanged(nameof(CanRenamePlaylist));
                UpdatePlaylistSongs(value);
            }
        }
    }

    public ObservableCollection<Song> PlaylistSongs
    {
        get => _playlistSongs;
        private set => SetProperty(ref _playlistSongs, value);
    }

    public bool CanDeletePlaylist => _selectedPlaylist != null &&
                                     _selectedPlaylist.Id != "favorites";

    public bool CanRenamePlaylist => _selectedPlaylist != null &&
                                     _selectedPlaylist.Id != "favorites";

    public string NewPlaylistName
    {
        get => _newPlaylistName;
        set => SetProperty(ref _newPlaylistName, value);
    }

    public bool IsCreatingPlaylist
    {
        get => _isCreatingPlaylist;
        set => SetProperty(ref _isCreatingPlaylist, value);
    }

    [RelayCommand]
    private void CreatePlaylist()
    {
        if (!string.IsNullOrWhiteSpace(_newPlaylistName))
        {
            _playlistService.CreatePlaylist(_newPlaylistName);
            NewPlaylistName = string.Empty;
            IsCreatingPlaylist = false;
        }
    }

    [RelayCommand]
    private void DeletePlaylist()
    {
        if (_selectedPlaylist != null && CanDeletePlaylist)
        {
            _playlistService.DeletePlaylist(_selectedPlaylist.Id);
            SelectedPlaylist = null;
        }
    }

    [RelayCommand]
    private async Task RenamePlaylistAsync()
    {
        if (_selectedPlaylist != null && CanRenamePlaylist)
        {
            var newName = await _dialogService.ShowInputDialogAsync("Rename Playlist", _selectedPlaylist.Name);
            if (!string.IsNullOrWhiteSpace(newName))
            {
                _playlistService.RenamePlaylist(_selectedPlaylist.Id, newName);
            }
        }
    }

    [RelayCommand]
    private async Task ExportPlaylistAsync()
    {
        if (_selectedPlaylist == null)
            return;

        var filePath = await _dialogService.SaveFileAsync("导出播放列表", "JSON Files (*.json)|*.json");
        if (!string.IsNullOrEmpty(filePath))
        {
            await _playlistService.ExportPlaylistAsync(_selectedPlaylist.Id, filePath);
        }
    }

    [RelayCommand]
    private async Task ImportPlaylistAsync()
    {
        var filePath = await _dialogService.OpenFileAsync("导入播放列表", "JSON Files (*.json)|*.json");
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
    private void RemoveSong(string path)
    {
        if (_selectedPlaylist != null)
        {
            _playlistService.RemoveSongFromPlaylist(_selectedPlaylist.Id, path);
            UpdatePlaylistSongs(_selectedPlaylist);
        }
    }

    [RelayCommand]
    private void PlayAll()
    {
        if (PlaylistSongs.Count == 0) return;

        var currentPlaylist = _playbackService.CreatePlaylist("临时播放");
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
    private void MoveSong((int OldIndex, int NewIndex) param)
    {
        if (_selectedPlaylist != null)
        {
            _playlistService.MoveSongInPlaylist(_selectedPlaylist.Id, param.OldIndex, param.NewIndex);
            UpdatePlaylistSongs(_selectedPlaylist);
        }
    }

    [RelayCommand]
    private void EditSongMetadata(Song song)
    {
        var dialog = new MetadataEditorView
        {
            DataContext =
                new MetadataEditorViewModel(song, _dialogService, () => { UpdatePlaylistSongs(_selectedPlaylist); })
        };
        dialog.Show();
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

        _playlistService.PlaylistsChanged += (_, _) => { OnPropertyChanged(nameof(UserPlaylists)); };
    }

    private void UpdatePlaylistSongs(UserPlaylist? playlist)
    {
        var songs = new ObservableCollection<Song>();
        if (playlist != null)
        {
            foreach (var song in _playlistService.GetPlaylistSongs(playlist.Id))
            {
                songs.Add(song);
            }
        }

        PlaylistSongs = songs;
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

    public void AddSongToSelectedPlaylist(Song song)
    {
        if (_selectedPlaylist != null)
        {
            _playlistService.AddSongToPlaylist(_selectedPlaylist.Id, song);
            UpdatePlaylistSongs(_selectedPlaylist);
        }
    }
}
