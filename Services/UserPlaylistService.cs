using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LocalMusicPlayer.Models;

namespace LocalMusicPlayer.Services;

public class UserPlaylistService : IUserPlaylistService
{
    private readonly IConfigurationService _configService;
    private readonly IMusicLibraryService _libraryService;
    private readonly ObservableCollection<UserPlaylist> _userPlaylists = new();
    private List<string> _favoriteFilePaths = new();

    public ObservableCollection<UserPlaylist> UserPlaylists => _userPlaylists;

    public event EventHandler? PlaylistsChanged;
    public event EventHandler? FavoritesChanged;

    public UserPlaylistService(IConfigurationService configService, IMusicLibraryService libraryService)
    {
        _configService = configService;
        _libraryService = libraryService;

        // 加载已保存的播放列表
        LoadPlaylistsAsync().ConfigureAwait(false);

        // 监听歌曲库变更，清理失效引用
        _libraryService.Songs.CollectionChanged += (_, _) => CleanupInvalidReferences();
    }

    public UserPlaylist CreatePlaylist(string name)
    {
        var playlist = new UserPlaylist { Name = name };
        _userPlaylists.Add(playlist);
        PlaylistsChanged?.Invoke(this, EventArgs.Empty);
        SavePlaylistsAsync().ConfigureAwait(false);
        return playlist;
    }

    public void DeletePlaylist(string playlistId)
    {
        var playlist = _userPlaylists.FirstOrDefault(p => p.Id == playlistId);
        if (playlist != null)
        {
            _userPlaylists.Remove(playlist);
            PlaylistsChanged?.Invoke(this, EventArgs.Empty);
            SavePlaylistsAsync().ConfigureAwait(false);
        }
    }

    public void RenamePlaylist(string playlistId, string newName)
    {
        var playlist = _userPlaylists.FirstOrDefault(p => p.Id == playlistId);
        if (playlist != null)
        {
            playlist.Name = newName;
            playlist.ModifiedTime = DateTime.Now;
            PlaylistsChanged?.Invoke(this, EventArgs.Empty);
            SavePlaylistsAsync().ConfigureAwait(false);
        }
    }

    public void AddSongToPlaylist(string playlistId, Song song)
    {
        var playlist = _userPlaylists.FirstOrDefault(p => p.Id == playlistId);
        if (playlist != null && !playlist.SongFilePaths.Contains(song.FilePath))
        {
            playlist.SongFilePaths.Add(song.FilePath);
            playlist.ModifiedTime = DateTime.Now;
            PlaylistsChanged?.Invoke(this, EventArgs.Empty);
            SavePlaylistsAsync().ConfigureAwait(false);
        }
    }

    public void RemoveSongFromPlaylist(string playlistId, string filePath)
    {
        var playlist = _userPlaylists.FirstOrDefault(p => p.Id == playlistId);
        if (playlist != null && playlist.SongFilePaths.Remove(filePath))
        {
            playlist.ModifiedTime = DateTime.Now;
            PlaylistsChanged?.Invoke(this, EventArgs.Empty);
            SavePlaylistsAsync().ConfigureAwait(false);
        }
    }

    public List<Song> GetPlaylistSongs(string playlistId)
    {
        var playlist = _userPlaylists.FirstOrDefault(p => p.Id == playlistId);
        if (playlist == null) return new List<Song>();

        return playlist.SongFilePaths
            .Select(path => _libraryService.Songs.FirstOrDefault(s => s.FilePath == path))
            .Where(s => s != null)
            .ToList()!;
    }

    public void MoveSongInPlaylist(string playlistId, int oldIndex, int newIndex)
    {
        var playlist = _userPlaylists.FirstOrDefault(p => p.Id == playlistId);
        if (playlist == null) return;

        if (oldIndex < 0 || oldIndex >= playlist.SongFilePaths.Count)
            return;

        if (newIndex < 0 || newIndex >= playlist.SongFilePaths.Count)
            return;

        var item = playlist.SongFilePaths[oldIndex];
        playlist.SongFilePaths.RemoveAt(oldIndex);
        playlist.SongFilePaths.Insert(newIndex, item);
        playlist.ModifiedTime = DateTime.Now;
        PlaylistsChanged?.Invoke(this, EventArgs.Empty);
        SavePlaylistsAsync().ConfigureAwait(false);
    }

    public void AddToFavorites(Song song)
    {
        if (!_favoriteFilePaths.Contains(song.FilePath))
        {
            _favoriteFilePaths.Add(song.FilePath);
            song.IsFavorite = true;
            FavoritesChanged?.Invoke(this, EventArgs.Empty);
            SavePlaylistsAsync().ConfigureAwait(false);
        }
    }

    public void RemoveFromFavorites(Song song)
    {
        if (_favoriteFilePaths.Remove(song.FilePath))
        {
            song.IsFavorite = false;
            FavoritesChanged?.Invoke(this, EventArgs.Empty);
            SavePlaylistsAsync().ConfigureAwait(false);
        }
    }

    public bool IsFavorite(string filePath)
    {
        return _favoriteFilePaths.Contains(filePath);
    }

    public List<Song> GetFavoriteSongs()
    {
        return _favoriteFilePaths
            .Select(path => _libraryService.Songs.FirstOrDefault(s => s.FilePath == path))
            .Where(s => s != null)
            .ToList()!;
    }

    public async Task SavePlaylistsAsync()
    {
        _configService.CurrentSettings.UserPlaylists = _userPlaylists.ToList();
        _configService.CurrentSettings.FavoriteFilePaths = _favoriteFilePaths;
        await _configService.SaveSettingsAsync();
    }

    public async Task LoadPlaylistsAsync()
    {
        await _configService.LoadSettingsAsync();

        _userPlaylists.Clear();
        foreach (var playlist in _configService.CurrentSettings.UserPlaylists)
        {
            _userPlaylists.Add(playlist);
        }

        _favoriteFilePaths = _configService.CurrentSettings.FavoriteFilePaths ?? new List<string>();

        // 同步 Song.IsFavorite 状态
        foreach (var song in _libraryService.Songs)
        {
            song.IsFavorite = _favoriteFilePaths.Contains(song.FilePath);
        }

        PlaylistsChanged?.Invoke(this, EventArgs.Empty);
        FavoritesChanged?.Invoke(this, EventArgs.Empty);
    }

    private void CleanupInvalidReferences()
    {
        var validPaths = _libraryService.Songs.Select(s => s.FilePath).ToList();

        // 清理播放列表中的无效引用
        foreach (var playlist in _userPlaylists)
        {
            var invalidPaths = playlist.SongFilePaths.Where(p => !validPaths.Contains(p)).ToList();
            foreach (var path in invalidPaths)
            {
                playlist.SongFilePaths.Remove(path);
            }
        }

        // 清理收藏中的无效引用
        var invalidFavorites = _favoriteFilePaths.Where(p => !validPaths.Contains(p)).ToList();
        foreach (var path in invalidFavorites)
        {
            _favoriteFilePaths.Remove(path);
        }
    }

    public async Task ExportPlaylistAsync(string playlistId, string filePath)
    {
        var playlist = _userPlaylists.FirstOrDefault(p => p.Id == playlistId);
        if (playlist == null) return;

        var playlistData = new
        {
            playlist.Name,
            playlist.SongFilePaths,
            playlist.CreatedTime,
            playlist.ModifiedTime
        };

        var json = System.Text.Json.JsonSerializer.Serialize(playlistData, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });

        await System.IO.File.WriteAllTextAsync(filePath, json);
    }

    public async Task ImportPlaylistAsync(string filePath)
    {
        if (!System.IO.File.Exists(filePath)) return;

        var json = await System.IO.File.ReadAllTextAsync(filePath);
        var playlistData = System.Text.Json.JsonSerializer.Deserialize<dynamic>(json);

        if (playlistData == null || playlistData.Name == null)
            return;

        var playlist = CreatePlaylist(playlistData.Name?.ToString() ?? string.Empty);
        playlist.CreatedTime = playlistData.CreatedTime ?? DateTime.Now;
        playlist.ModifiedTime = playlistData.ModifiedTime ?? DateTime.Now;

        if (playlistData.SongFilePaths != null)
        {
            foreach (var path in playlistData.SongFilePaths)
            {
                var song = _libraryService.Songs.FirstOrDefault(s => s.FilePath == path.ToString());
                if (song != null)
                {
                    AddSongToPlaylist(playlist.Id, song);
                }
            }
        }
    }
}