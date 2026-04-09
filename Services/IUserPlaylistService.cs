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

    UserPlaylist CreatePlaylist(string name);
    void DeletePlaylist(string playlistId);
    void RenamePlaylist(string playlistId, string newName);

    void AddSongToPlaylist(string playlistId, Song song);
    void RemoveSongFromPlaylist(string playlistId, string filePath);
    List<Song> GetPlaylistSongs(string playlistId);
    void MoveSongInPlaylist(string playlistId, int oldIndex, int newIndex);

    void AddToFavorites(Song song);
    void RemoveFromFavorites(Song song);
    bool IsFavorite(string filePath);
    List<Song> GetFavoriteSongs();

    Task SavePlaylistsAsync();
    Task LoadPlaylistsAsync();
    Task ExportPlaylistAsync(string playlistId, string filePath);
    Task ImportPlaylistAsync(string filePath);

    event EventHandler? PlaylistsChanged;
    event EventHandler? FavoritesChanged;
}
