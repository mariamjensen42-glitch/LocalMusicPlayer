using System;
using System.Collections.Generic;
using LocalMusicPlayer.Models;

namespace LocalMusicPlayer.Services;

public class PlaylistService : IPlaylistService
{
    private Playlist? _currentPlaylist;
    private int _currentIndex = -1;
    private PlaybackMode _playbackMode = PlaybackMode.Normal;
    private readonly Random _random = new();

    public event EventHandler<Song?>? CurrentSongChanged;
    public event EventHandler<PlaybackMode>? PlaybackModeChanged;

    public Playlist? CurrentPlaylist => _currentPlaylist;
    public int CurrentIndex => _currentIndex;
    public Song? CurrentSong => _currentPlaylist?.Songs.Count > _currentIndex && _currentIndex >= 0
        ? _currentPlaylist.Songs[_currentIndex]
        : null;

    public PlaybackMode PlaybackMode
    {
        get => _playbackMode;
        set
        {
            if (_playbackMode != value)
            {
                _playbackMode = value;
                PlaybackModeChanged?.Invoke(this, _playbackMode);
            }
        }
    }

    public Playlist CreatePlaylist(string name)
    {
        var playlist = new Playlist
        {
            Name = name,
            Songs = []
        };
        _currentPlaylist = playlist;
        return playlist;
    }

    public void SetCurrentPlaylist(Playlist playlist)
    {
        _currentPlaylist = playlist;
        _currentIndex = -1;
    }

    public void AddSongToPlaylist(Playlist playlist, Song song)
    {
        var songList = new List<Song>(playlist.Songs) { song };
        playlist.Songs = songList;
    }

    public void RemoveSongFromPlaylist(Playlist playlist, int songIndex)
    {
        if (songIndex < 0 || songIndex >= playlist.Songs.Count)
            return;

        var songList = new List<Song>(playlist.Songs);
        songList.RemoveAt(songIndex);
        playlist.Songs = songList;
    }

    public bool PlayNext()
    {
        if (_currentPlaylist == null || _currentPlaylist.Songs.Count == 0)
            return false;

        int nextIndex;
        switch (_playbackMode)
        {
            case PlaybackMode.Shuffle:
                nextIndex = _random.Next(_currentPlaylist.Songs.Count);
                break;
            case PlaybackMode.Loop:
                nextIndex = _currentIndex + 1;
                if (nextIndex >= _currentPlaylist.Songs.Count)
                    nextIndex = 0;
                break;
            default:
                nextIndex = _currentIndex + 1;
                if (nextIndex >= _currentPlaylist.Songs.Count)
                    return false;
                break;
        }

        _currentIndex = nextIndex;
        CurrentSongChanged?.Invoke(this, CurrentSong);
        return true;
    }

    public bool PlayPrevious()
    {
        if (_currentPlaylist == null || _currentPlaylist.Songs.Count == 0)
            return false;

        int prevIndex;
        if (_playbackMode == PlaybackMode.Loop)
        {
            prevIndex = _currentIndex - 1;
            if (prevIndex < 0)
                prevIndex = _currentPlaylist.Songs.Count - 1;
        }
        else
        {
            prevIndex = _currentIndex - 1;
            if (prevIndex < 0)
                return false;
        }

        _currentIndex = prevIndex;
        CurrentSongChanged?.Invoke(this, CurrentSong);
        return true;
    }
}