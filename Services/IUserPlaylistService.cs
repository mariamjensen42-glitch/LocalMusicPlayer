using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using LocalMusicPlayer.Models;

namespace LocalMusicPlayer.Services;

public interface IUserPlaylistService
{
    ObservableCollection<UserPlaylist> UserPlaylists { get; }

    // 播放列表管理
    UserPlaylist CreatePlaylist(string name);
    void DeletePlaylist(string playlistId);
    void RenamePlaylist(string playlistId, string newName);

    // 歌曲管理
    void AddSongToPlaylist(string playlistId, Song song);
    void RemoveSongFromPlaylist(string playlistId, string filePath);
    List<Song> GetPlaylistSongs(string playlistId);

    // 收藏管理
    void AddToFavorites(Song song);
    void RemoveFromFavorites(Song song);
    bool IsFavorite(string filePath);
    List<Song> GetFavoriteSongs();

    // 持久化
    Task SavePlaylistsAsync();
    Task LoadPlaylistsAsync();

    // 事件
    event EventHandler? PlaylistsChanged;
    event EventHandler? FavoritesChanged;
}