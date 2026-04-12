using System;
using System.Collections.Generic;
using LocalMusicPlayer.Models;
using Microsoft.Extensions.Logging;

namespace LocalMusicPlayer.Services;

public class PlaylistService : IPlaylistService
{
    private Playlist? _currentPlaylist;
    private int _currentIndex = -1;
    private PlaybackMode _playbackMode = PlaybackMode.Normal;
    private readonly Random _random = new();
    private readonly ILogger<PlaylistService>? _logger;

    public PlaylistService(ILogger<PlaylistService>? logger = null)
    {
        _logger = logger;
        _logger?.LogInformation("[Playlist] PlaylistService initialized");
    }

    public event EventHandler<Song?>? CurrentSongChanged;
    public event EventHandler<PlaybackMode>? PlaybackModeChanged;
    public event EventHandler? CurrentIndexUpdated;

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
                _logger?.LogInformation("[Playlist] Playback mode changed to {PlaybackMode}", _playbackMode);
                PlaybackModeChanged?.Invoke(this, _playbackMode);
            }
        }
    }

    public Playlist CreatePlaylist(string name)
    {
        var playlist = new Playlist
        {
            Name = name,
            Songs = new()
        };
        _currentPlaylist = playlist;
        _logger?.LogInformation("[Playlist] Playlist created: {PlaylistName}", name);
        return playlist;
    }

    public void SetCurrentPlaylist(Playlist playlist)
    {
        _currentPlaylist = playlist;
        _currentIndex = -1;
        _logger?.LogInformation("[Playlist] Current playlist set to: {PlaylistName} with {SongCount} songs",
            playlist.Name, playlist.Songs.Count);
    }

    public void AddSongToPlaylist(Playlist playlist, Song song)
    {
        playlist.Songs.Add(song);
        _logger?.LogDebug("[Playlist] Song added to playlist {PlaylistName}: {SongTitle}", playlist.Name, song.Title);
    }

    public void RemoveSongFromPlaylist(Playlist playlist, int songIndex)
    {
        if (songIndex < 0 || songIndex >= playlist.Songs.Count)
            return;

        // Adjust currentIndex when removing
        if (playlist == _currentPlaylist)
        {
            if (songIndex == _currentIndex)
            {
                // Current song is being removed
                if (playlist.Songs.Count > 1)
                {
                    // Keep currentIndex, next song will take this position
                }
                else
                {
                    _currentIndex = -1;
                    _logger?.LogInformation("[Playlist] Current song removed, playlist now empty");
                    CurrentSongChanged?.Invoke(this, null);
                }
            }
            else if (songIndex < _currentIndex)
            {
                _currentIndex--;
                CurrentIndexUpdated?.Invoke(this, EventArgs.Empty);
            }
        }

        playlist.Songs.RemoveAt(songIndex);
    }

    public void ClearPlaylist()
    {
        if (_currentPlaylist == null)
            return;

        _logger?.LogInformation("[Playlist] Playlist cleared: {PlaylistName}", _currentPlaylist.Name);
        _currentPlaylist.Songs.Clear();
        _currentIndex = -1;
        CurrentSongChanged?.Invoke(this, null);
    }

    public void MoveSong(int oldIndex, int newIndex)
    {
        if (_currentPlaylist == null)
            return;

        if (oldIndex < 0 || oldIndex >= _currentPlaylist.Songs.Count)
            return;

        if (newIndex < 0 || newIndex >= _currentPlaylist.Songs.Count)
            return;

        // Move the song in the collection
        _currentPlaylist.Songs.Move(oldIndex, newIndex);
        _logger?.LogDebug("[Playlist] Song moved from index {OldIndex} to {NewIndex}", oldIndex, newIndex);

        // Adjust currentIndex
        if (_currentIndex == oldIndex)
        {
            // Current playing song was moved
            _currentIndex = newIndex;
        }
        else if (oldIndex < _currentIndex && newIndex >= _currentIndex)
        {
            // Song moved from before currentIndex to after or equal
            _currentIndex--;
        }
        else if (oldIndex > _currentIndex && newIndex <= _currentIndex)
        {
            // Song moved from after currentIndex to before or equal
            _currentIndex++;
        }

        CurrentIndexUpdated?.Invoke(this, EventArgs.Empty);
    }

    public bool PlayNext()
    {
        if (_currentPlaylist == null || _currentPlaylist.Songs.Count == 0)
            return false;

        int nextIndex;
        switch (_playbackMode)
        {
            case PlaybackMode.SingleLoop:
                nextIndex = _currentIndex;
                break;
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
        var song = CurrentSong;
        _logger?.LogDebug("[Playlist] PlayNext: index={Index}, mode={Mode}", _currentIndex, _playbackMode);
        CurrentSongChanged?.Invoke(this, song);
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
        var song = CurrentSong;
        _logger?.LogDebug("[Playlist] PlayPrevious: index={Index}", _currentIndex);
        CurrentSongChanged?.Invoke(this, song);
        return true;
    }

    public void PlaySong(Song song)
    {
        if (_currentPlaylist == null)
            return;

        var index = _currentPlaylist.Songs.IndexOf(song);
        if (index >= 0)
        {
            _currentIndex = index;
            _logger?.LogInformation("[Playlist] Playing song from playlist: {SongTitle}", song.Title);
            CurrentSongChanged?.Invoke(this, CurrentSong);
        }
    }

    public void PlayAtIndex(int index)
    {
        if (_currentPlaylist == null || index < 0 || index >= _currentPlaylist.Songs.Count)
            return;

        _currentIndex = index;
        _logger?.LogInformation("[Playlist] Playing song at index: {Index}", index);
        CurrentSongChanged?.Invoke(this, CurrentSong);
    }
}