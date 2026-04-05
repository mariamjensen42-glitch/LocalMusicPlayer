using System;
using LibVLCSharp.Shared;
using LocalMusicPlayer.Models;

namespace LocalMusicPlayer.Services;

public class MusicPlayerService : IMusicPlayerService, IDisposable
{
    private readonly LibVLC _libVlc;
    private MediaPlayer? _mediaPlayer;
    private Song? _currentSong;
    private int _volume = 100;
    private bool _isMuted;
    private int _previousVolume;
    private bool _disposed;

    public event EventHandler? PlaybackEnded;
    public event EventHandler<PlayState>? PlaybackStateChanged;
    public event EventHandler<TimeSpan>? PositionChanged;

    public TimeSpan Position => _mediaPlayer?.Time > 0 ? TimeSpan.FromMilliseconds(_mediaPlayer.Time) : TimeSpan.Zero;

    public TimeSpan Duration =>
        _mediaPlayer?.Length > 0 ? TimeSpan.FromMilliseconds(_mediaPlayer.Length) : TimeSpan.Zero;

    public bool IsPlaying => _mediaPlayer?.IsPlaying ?? false;
    public int Volume => _volume;
    public bool IsMuted => _isMuted;

    public MusicPlayerService()
    {
        Core.Initialize();
        _libVlc = new LibVLC("--quiet", "--no-video");
        _mediaPlayer = new MediaPlayer(_libVlc);

        _mediaPlayer.EndReached += OnEndReached;
        _mediaPlayer.TimeChanged += (_, args) => PositionChanged?.Invoke(this, TimeSpan.FromMilliseconds(args.Time));
        _mediaPlayer.Playing += (_, _) => PlaybackStateChanged?.Invoke(this, PlayState.Playing);
        _mediaPlayer.Paused += (_, _) => PlaybackStateChanged?.Invoke(this, PlayState.Paused);
        _mediaPlayer.Stopped += (_, _) => PlaybackStateChanged?.Invoke(this, PlayState.Stopped);
    }

    public void Play(Song song)
    {
        if (_disposed || _mediaPlayer == null) return;

        _currentSong = song;
        using var media = new Media(_libVlc, new Uri(song.FilePath));
        _mediaPlayer.Play(media);
        _mediaPlayer.Volume = _volume;
    }

    public void Pause()
    {
        _mediaPlayer?.Pause();
    }

    public void Resume()
    {
        if (_mediaPlayer != null && !_mediaPlayer.IsPlaying)
        {
            _mediaPlayer.Play();
        }
    }

    public void Stop()
    {
        _mediaPlayer?.Stop();
    }

    public void Next()
    {
    }

    public void Previous()
    {
    }

    public void Seek(TimeSpan position)
    {
        if (_mediaPlayer != null)
        {
            _mediaPlayer.Time = (long)position.TotalMilliseconds;
        }
    }

    public void SetVolume(int level)
    {
        _volume = Math.Clamp(level, 0, 100);
        if (_mediaPlayer != null && !_isMuted)
        {
            _mediaPlayer.Volume = _volume;
        }
    }

    public void Mute()
    {
        if (_isMuted)
        {
            _isMuted = false;
            _volume = _previousVolume;
            if (_mediaPlayer != null)
            {
                _mediaPlayer.Volume = _volume;
            }
        }
        else
        {
            _previousVolume = _volume;
            _isMuted = true;
            if (_mediaPlayer != null)
            {
                _mediaPlayer.Volume = 0;
            }
        }
    }

    private void OnEndReached(object? sender, EventArgs e)
    {
        PlaybackEnded?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _mediaPlayer?.Stop();
        _mediaPlayer?.Dispose();
        _libVlc.Dispose();
    }
}