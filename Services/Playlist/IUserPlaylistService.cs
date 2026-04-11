using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LocalMusicPlayer.Models;

namespace LocalMusicPlayer.Services;

public interface IUserPlaylistService
{
    ObservableCollection<UserPlaylist> UserPlaylists { get; }

    Task<UserPlaylist> CreatePlaylistAsync(string name);
    Task DeletePlaylistAsync(string playlistId);
    Task RenamePlaylistAsync(string playlistId, string newName);

    Task AddSongToPlaylistAsync(string playlistId, Song song);
    Task RemoveSongFromPlaylistAsync(string playlistId, string filePath);
    List<Song> GetPlaylistSongs(string playlistId);
    Task MoveSongInPlaylistAsync(string playlistId, int oldIndex, int newIndex);

    Task AddToFavoritesAsync(Song song);
    Task RemoveFromFavoritesAsync(Song song);
    Task<bool> IsFavoriteAsync(string filePath);
    Task<List<Song>> GetFavoriteSongsAsync();

    Task SavePlaylistsAsync();
    Task LoadPlaylistsAsync();
    Task ExportPlaylistAsync(string playlistId, string filePath);
    Task ImportPlaylistAsync(string filePath);

    event EventHandler? PlaylistsChanged;
    event EventHandler? FavoritesChanged;
}
