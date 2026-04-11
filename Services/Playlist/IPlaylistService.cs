using System;
using System.Collections.Generic;
using LocalMusicPlayer.Models;

namespace LocalMusicPlayer.Services;

public interface IPlaylistService
{
    Playlist CreatePlaylist(string name);
    void SetCurrentPlaylist(Playlist playlist);
    void AddSongToPlaylist(Playlist playlist, Song song);
    void RemoveSongFromPlaylist(Playlist playlist, int songIndex);
    void ClearPlaylist();
    void MoveSong(int oldIndex, int newIndex);
    bool PlayNext();
    bool PlayPrevious();
    void PlaySong(Song song);
    void PlayAtIndex(int index);
    PlaybackMode PlaybackMode { get; set; }
    Playlist? CurrentPlaylist { get; }
    int CurrentIndex { get; }
    Song? CurrentSong { get; }
    event EventHandler<Song?>? CurrentSongChanged;
    event EventHandler<PlaybackMode>? PlaybackModeChanged;
    event EventHandler? CurrentIndexUpdated;
}
