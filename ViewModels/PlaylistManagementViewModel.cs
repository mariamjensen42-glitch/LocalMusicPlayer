using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using ReactiveUI;
using LocalMusicPlayer.Models;
using LocalMusicPlayer.Services;

namespace LocalMusicPlayer.ViewModels;

public class PlaylistManagementViewModel : ViewModelBase
{
    private readonly IUserPlaylistService _playlistService;
    private readonly IMusicPlayerService _musicPlayerService;
    private readonly IPlaylistService _playbackService;
    private readonly IStatisticsService _statisticsService;
    private readonly IMusicLibraryService _libraryService;

    private UserPlaylist? _selectedPlaylist;
    private string _newPlaylistName = string.Empty;
    private bool _isCreatingPlaylist;

    public ObservableCollection<UserPlaylist> UserPlaylists => _playlistService.UserPlaylists;

    public UserPlaylist? SelectedPlaylist
    {
        get => _selectedPlaylist;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedPlaylist, value);
            this.RaisePropertyChanged(nameof(PlaylistSongs));
            this.RaisePropertyChanged(nameof(CanDeletePlaylist));
        }
    }

    public ObservableCollection<Song> PlaylistSongs
    {
        get
        {
            var songs = new ObservableCollection<Song>();
            if (_selectedPlaylist != null)
            {
                foreach (var song in _playlistService.GetPlaylistSongs(_selectedPlaylist.Id))
                {
                    songs.Add(song);
                }
            }

            return songs;
        }
    }

    public bool CanDeletePlaylist => _selectedPlaylist != null &&
                                     _selectedPlaylist.Id != "favorites"; // 不能删除收藏列表

    public string NewPlaylistName
    {
        get => _newPlaylistName;
        set => this.RaiseAndSetIfChanged(ref _newPlaylistName, value);
    }

    public bool IsCreatingPlaylist
    {
        get => _isCreatingPlaylist;
        set => this.RaiseAndSetIfChanged(ref _isCreatingPlaylist, value);
    }

    public ReactiveCommand<Unit, Unit> CreatePlaylistCommand { get; }
    public ReactiveCommand<Unit, Unit> DeletePlaylistCommand { get; }
    public ReactiveCommand<Unit, Unit> RenamePlaylistCommand { get; }
    public ReactiveCommand<string, Unit> PlaySongCommand { get; }
    public ReactiveCommand<string, Unit> RemoveSongCommand { get; }
    public ReactiveCommand<Unit, Unit> PlayAllCommand { get; }

    public PlaylistManagementViewModel(
        IUserPlaylistService playlistService,
        IMusicPlayerService musicPlayerService,
        IPlaylistService playbackService,
        IStatisticsService statisticsService,
        IMusicLibraryService libraryService)
    {
        _playlistService = playlistService;
        _musicPlayerService = musicPlayerService;
        _playbackService = playbackService;
        _statisticsService = statisticsService;
        _libraryService = libraryService;

        // 确保收藏列表存在
        EnsureFavoritesPlaylist();

        CreatePlaylistCommand = ReactiveCommand.Create(() =>
        {
            if (!string.IsNullOrWhiteSpace(_newPlaylistName))
            {
                _playlistService.CreatePlaylist(_newPlaylistName);
                NewPlaylistName = string.Empty;
                IsCreatingPlaylist = false;
            }
        });

        DeletePlaylistCommand = ReactiveCommand.Create(() =>
        {
            if (_selectedPlaylist != null && CanDeletePlaylist)
            {
                _playlistService.DeletePlaylist(_selectedPlaylist.Id);
                SelectedPlaylist = null;
            }
        });

        RenamePlaylistCommand = ReactiveCommand.Create(() =>
        {
            // 需要弹窗输入新名称，这里简化处理
            // 实际实现时可以通过 IWindowProvider 打开对话框
        });

        PlaySongCommand = ReactiveCommand.Create<string>(path =>
        {
            var song = PlaylistSongs.FirstOrDefault(s => s.FilePath == path);
            if (song != null)
            {
                _statisticsService.RecordPlayStart(song);
                _musicPlayerService.Play(song);
            }
        });

        RemoveSongCommand = ReactiveCommand.Create<string>(path =>
        {
            if (_selectedPlaylist != null)
            {
                _playlistService.RemoveSongFromPlaylist(_selectedPlaylist.Id, path);
                this.RaisePropertyChanged(nameof(PlaylistSongs));
            }
        });

        PlayAllCommand = ReactiveCommand.Create(() =>
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
        });

        // 监听播放列表变更
        _playlistService.PlaylistsChanged += (_, _) => { this.RaisePropertyChanged(nameof(UserPlaylists)); };
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
            this.RaisePropertyChanged(nameof(PlaylistSongs));
        }
    }
}