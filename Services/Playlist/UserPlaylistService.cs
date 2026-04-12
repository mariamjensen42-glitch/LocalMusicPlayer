using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LocalMusicPlayer.Data;
using LocalMusicPlayer.Models;

namespace LocalMusicPlayer.Services;

public class UserPlaylistService : IUserPlaylistService, IDisposable
{
    private readonly IMusicLibraryService _libraryService;
    private readonly ObservableCollection<UserPlaylist> _userPlaylists = new();
    private bool _disposed;

    public ObservableCollection<UserPlaylist> UserPlaylists => _userPlaylists;

    public event EventHandler? PlaylistsChanged;
    public event EventHandler? FavoritesChanged;

    public UserPlaylistService(IMusicLibraryService libraryService)
    {
        _libraryService = libraryService;

        _libraryService.Songs.CollectionChanged += OnSongsCollectionChanged;
    }

    private async Task SyncFavoritesToSongsAsync()
    {
        var favoritePaths = await Task.Run(() =>
        {
            using var db = new AppDbContext();
            return db.Favorites.Select(f => f.FilePath).ToList();
        });

        foreach (var song in _libraryService.Songs)
        {
            song.IsFavorite = favoritePaths.Contains(song.FilePath);
        }
    }

    public async Task<UserPlaylist> CreatePlaylistAsync(string name)
    {
        var playlist = new UserPlaylist { Name = name };

        await Task.Run(() =>
        {
            using var db = new AppDbContext();
            db.Playlists.Add(new PlaylistEntity
            {
                PlaylistId = playlist.Id,
                Name = name,
                CreatedTime = playlist.CreatedTime,
                ModifiedTime = playlist.ModifiedTime,
                CoverArtPath = playlist.CoverArtPath
            });
            db.SaveChanges();
        });

        _userPlaylists.Add(playlist);
        PlaylistsChanged?.Invoke(this, EventArgs.Empty);
        return playlist;
    }

    public async Task DeletePlaylistAsync(string playlistId)
    {
        var playlist = _userPlaylists.FirstOrDefault(p => p.Id == playlistId);
        if (playlist == null) return;

        await Task.Run(() =>
        {
            using var db = new AppDbContext();
            var songs = db.PlaylistSongs.Where(ps => ps.PlaylistId == playlistId).ToList();
            db.PlaylistSongs.RemoveRange(songs);

            var entity = db.Playlists.FirstOrDefault(p => p.PlaylistId == playlistId);
            if (entity != null) db.Playlists.Remove(entity);

            db.SaveChanges();
        });

        _userPlaylists.Remove(playlist);
        PlaylistsChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task RenamePlaylistAsync(string playlistId, string newName)
    {
        var playlist = _userPlaylists.FirstOrDefault(p => p.Id == playlistId);
        if (playlist == null) return;

        playlist.Name = newName;
        playlist.ModifiedTime = DateTime.Now;

        await Task.Run(() =>
        {
            using var db = new AppDbContext();
            var entity = db.Playlists.FirstOrDefault(p => p.PlaylistId == playlistId);
            if (entity != null)
            {
                entity.Name = newName;
                entity.ModifiedTime = DateTime.Now;
                db.SaveChanges();
            }
        });

        PlaylistsChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task AddSongToPlaylistAsync(string playlistId, Song song)
    {
        var playlist = _userPlaylists.FirstOrDefault(p => p.Id == playlistId);
        if (playlist == null || playlist.SongFilePaths.Contains(song.FilePath)) return;

        playlist.SongFilePaths.Add(song.FilePath);
        playlist.ModifiedTime = DateTime.Now;

        if (string.IsNullOrEmpty(playlist.CoverArtPath))
        {
            playlist.CoverArtPath = song.AlbumArtPath;
        }

        await Task.Run(() =>
        {
            using var db = new AppDbContext();
            var maxOrder = db.PlaylistSongs
                .Where(ps => ps.PlaylistId == playlistId)
                .Select(ps => (int?)ps.SortOrder)
                .Max() ?? -1;

            db.PlaylistSongs.Add(new PlaylistSongEntity
            {
                PlaylistId = playlistId,
                FilePath = song.FilePath,
                SortOrder = maxOrder + 1
            });

            var entity = db.Playlists.FirstOrDefault(p => p.PlaylistId == playlistId);
            if (entity != null) entity.ModifiedTime = DateTime.Now;

            db.SaveChanges();
        });

        PlaylistsChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task RemoveSongFromPlaylistAsync(string playlistId, string filePath)
    {
        var playlist = _userPlaylists.FirstOrDefault(p => p.Id == playlistId);
        if (playlist == null || !playlist.SongFilePaths.Remove(filePath)) return;

        playlist.ModifiedTime = DateTime.Now;

        await Task.Run(() =>
        {
            using var db = new AppDbContext();
            var entity = db.PlaylistSongs.FirstOrDefault(ps => ps.PlaylistId == playlistId && ps.FilePath == filePath);
            if (entity != null) db.PlaylistSongs.Remove(entity);

            var playlistEntity = db.Playlists.FirstOrDefault(p => p.PlaylistId == playlistId);
            if (playlistEntity != null) playlistEntity.ModifiedTime = DateTime.Now;

            db.SaveChanges();
        });

        PlaylistsChanged?.Invoke(this, EventArgs.Empty);
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

    public async Task MoveSongInPlaylistAsync(string playlistId, int oldIndex, int newIndex)
    {
        var playlist = _userPlaylists.FirstOrDefault(p => p.Id == playlistId);
        if (playlist == null) return;

        if (oldIndex < 0 || oldIndex >= playlist.SongFilePaths.Count) return;
        if (newIndex < 0 || newIndex >= playlist.SongFilePaths.Count) return;

        var item = playlist.SongFilePaths[oldIndex];
        playlist.SongFilePaths.RemoveAt(oldIndex);
        playlist.SongFilePaths.Insert(newIndex, item);
        playlist.ModifiedTime = DateTime.Now;

        await Task.Run(() =>
        {
            using var db = new AppDbContext();
            var songs = db.PlaylistSongs
                .Where(ps => ps.PlaylistId == playlistId)
                .OrderBy(ps => ps.SortOrder)
                .ToList();

            var reordered = playlist.SongFilePaths.ToList();
            for (var i = 0; i < songs.Count && i < reordered.Count; i++)
            {
                var entity = db.PlaylistSongs.FirstOrDefault(ps =>
                    ps.PlaylistId == playlistId && ps.FilePath == reordered[i]);
                if (entity != null) entity.SortOrder = i;
            }

            var playlistEntity = db.Playlists.FirstOrDefault(p => p.PlaylistId == playlistId);
            if (playlistEntity != null) playlistEntity.ModifiedTime = DateTime.Now;

            db.SaveChanges();
        });

        PlaylistsChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task AddToFavoritesAsync(Song song)
    {
        await Task.Run(() =>
        {
            using var db = new AppDbContext();
            if (!db.Favorites.Any(f => f.FilePath == song.FilePath))
            {
                db.Favorites.Add(new FavoriteEntity { FilePath = song.FilePath });
                db.SaveChanges();
            }
        });

        song.IsFavorite = true;
        FavoritesChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task RemoveFromFavoritesAsync(Song song)
    {
        await Task.Run(() =>
        {
            using var db = new AppDbContext();
            var entity = db.Favorites.FirstOrDefault(f => f.FilePath == song.FilePath);
            if (entity != null)
            {
                db.Favorites.Remove(entity);
                db.SaveChanges();
            }
        });

        song.IsFavorite = false;
        FavoritesChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task<bool> IsFavoriteAsync(string filePath)
    {
        return await Task.Run(() =>
        {
            using var db = new AppDbContext();
            return db.Favorites.Any(f => f.FilePath == filePath);
        });
    }

    public async Task<List<Song>> GetFavoriteSongsAsync()
    {
        var favoritePaths = await Task.Run(() =>
        {
            using var db = new AppDbContext();
            return db.Favorites.Select(f => f.FilePath).ToList();
        });

        return favoritePaths
            .Select(path => _libraryService.Songs.FirstOrDefault(s => s.FilePath == path))
            .Where(s => s != null)
            .ToList()!;
    }

    public async Task SavePlaylistsAsync()
    {
        await Task.Run(() =>
        {
            using var db = new AppDbContext();

            var existingPlaylists = db.Playlists.ToList();
            var existingSongs = db.PlaylistSongs.ToList();

            var currentIds = _userPlaylists.Select(p => p.Id).ToHashSet();

            foreach (var entity in existingPlaylists.Where(e => !currentIds.Contains(e.PlaylistId)))
            {
                db.Playlists.Remove(entity);
                var songsToRemove = existingSongs.Where(ps => ps.PlaylistId == entity.PlaylistId).ToList();
                db.PlaylistSongs.RemoveRange(songsToRemove);
            }

            foreach (var playlist in _userPlaylists)
            {
                var entity = existingPlaylists.FirstOrDefault(e => e.PlaylistId == playlist.Id);
                if (entity == null)
                {
                    entity = new PlaylistEntity
                    {
                        PlaylistId = playlist.Id,
                        Name = playlist.Name,
                        CreatedTime = playlist.CreatedTime,
                        ModifiedTime = playlist.ModifiedTime,
                        CoverArtPath = playlist.CoverArtPath
                    };
                    db.Playlists.Add(entity);
                }
                else
                {
                    entity.Name = playlist.Name;
                    entity.ModifiedTime = playlist.ModifiedTime;
                    entity.CoverArtPath = playlist.CoverArtPath;
                }

                var existingPlaylistSongs = existingSongs.Where(ps => ps.PlaylistId == playlist.Id).ToList();
                db.PlaylistSongs.RemoveRange(existingPlaylistSongs);

                for (var i = 0; i < playlist.SongFilePaths.Count; i++)
                {
                    db.PlaylistSongs.Add(new PlaylistSongEntity
                    {
                        PlaylistId = playlist.Id,
                        FilePath = playlist.SongFilePaths[i],
                        SortOrder = i
                    });
                }
            }

            db.SaveChanges();
        });
    }

    public async Task LoadPlaylistsAsync()
    {
        var (playlists, favoritePaths) = await Task.Run(() =>
        {
            using var db = new AppDbContext();
            var playlistEntities = db.Playlists.ToList();
            var playlistSongEntities = db.PlaylistSongs.ToList();
            var favPaths = db.Favorites.Select(f => f.FilePath).ToList();

            var result = playlistEntities.Select(pe => new UserPlaylist
            {
                Id = pe.PlaylistId,
                Name = pe.Name,
                CreatedTime = pe.CreatedTime,
                ModifiedTime = pe.ModifiedTime,
                CoverArtPath = pe.CoverArtPath,
                SongFilePaths = playlistSongEntities
                    .Where(ps => ps.PlaylistId == pe.PlaylistId)
                    .OrderBy(ps => ps.SortOrder)
                    .Select(ps => ps.FilePath)
                    .ToList()
            }).ToList();

            return (result, favPaths);
        });

        _userPlaylists.Clear();
        foreach (var playlist in playlists)
        {
            DeriveCoverArtFromFirstSong(playlist);
            _userPlaylists.Add(playlist);
        }

        EnsureDefaultPlaylistExists();

        foreach (var song in _libraryService.Songs)
        {
            song.IsFavorite = favoritePaths.Contains(song.FilePath);
        }

        PlaylistsChanged?.Invoke(this, EventArgs.Empty);
        FavoritesChanged?.Invoke(this, EventArgs.Empty);
    }

    private void EnsureDefaultPlaylistExists()
    {
        const string favoritesId = "favorites";
        if (!_userPlaylists.Any(p => p.Id == favoritesId))
        {
            var favoritesPlaylist = new UserPlaylist
            {
                Id = favoritesId,
                Name = "我喜欢的"
            };
            _userPlaylists.Insert(0, favoritesPlaylist);
        }
    }

    private void DeriveCoverArtFromFirstSong(UserPlaylist playlist)
    {
        if (string.IsNullOrEmpty(playlist.CoverArtPath) && playlist.SongFilePaths.Count > 0)
        {
            var firstSongPath = playlist.SongFilePaths[0];
            var firstSong = _libraryService.Songs.FirstOrDefault(s => s.FilePath == firstSongPath);
            if (firstSong != null)
            {
                playlist.CoverArtPath = firstSong.AlbumArtPath;
            }
        }
    }

    private async Task CleanupInvalidReferencesAsync()
    {
        var validPaths = _libraryService.Songs.Select(s => s.FilePath).ToHashSet();

        foreach (var playlist in _userPlaylists)
        {
            var invalidPaths = playlist.SongFilePaths.Where(p => !validPaths.Contains(p)).ToList();
            foreach (var path in invalidPaths)
            {
                playlist.SongFilePaths.Remove(path);
            }
        }

        await Task.Run(() =>
        {
            using var db = new AppDbContext();
            var allFavorites = db.Favorites.ToList();
            var invalidFavorites = allFavorites.Where(f => !validPaths.Contains(f.FilePath)).ToList();
            if (invalidFavorites.Count > 0)
            {
                db.Favorites.RemoveRange(invalidFavorites);
                db.SaveChanges();
            }
        });
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

        var json = JsonSerializer.Serialize(playlistData, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        await System.IO.File.WriteAllTextAsync(filePath, json);
    }

    public async Task ImportPlaylistAsync(string filePath)
    {
        if (!System.IO.File.Exists(filePath)) return;

        var json = await System.IO.File.ReadAllTextAsync(filePath);
        var playlistData = JsonSerializer.Deserialize<JsonElement>(json);

        if (playlistData.ValueKind == JsonValueKind.Undefined) return;
        if (!playlistData.TryGetProperty("Name", out var nameProp)) return;

        var name = nameProp.GetString() ?? string.Empty;
        var playlist = await CreatePlaylistAsync(name);

        if (playlistData.TryGetProperty("ModifiedTime", out var modifiedTimeProp))
        {
            if (modifiedTimeProp.TryGetDateTime(out var modifiedTime))
                playlist.ModifiedTime = modifiedTime;
        }

        if (playlistData.TryGetProperty("SongFilePaths", out var songsProp) &&
            songsProp.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in songsProp.EnumerateArray())
            {
                var path = item.GetString();
                if (path == null) continue;
                var song = _libraryService.Songs.FirstOrDefault(s => s.FilePath == path);
                if (song != null)
                {
                    await AddSongToPlaylistAsync(playlist.Id, song);
                }
            }
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _libraryService.Songs.CollectionChanged -= OnSongsCollectionChanged;
    }

    private async void OnSongsCollectionChanged(object? sender,
        System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        await SyncFavoritesToSongsAsync();
        await CleanupInvalidReferencesAsync();
    }
}
