using System;
using LocalMusicPlayer.Models;

namespace LocalMusicPlayer.Services;

public interface IPlaybackStateService
{
    Song? CurrentSong { get; }
    bool IsPlaying { get; }
    TimeSpan Position { get; }
    TimeSpan Duration { get; }
    int Volume { get; }
    bool IsMuted { get; }
    PlaybackMode PlaybackMode { get; set; }
    double PositionSeconds { get; set; }
    double DurationSeconds { get; }

    void Play(Song song);
    void Play();
    void Pause();
    void Resume();
    void Stop();
    void Seek(TimeSpan position);
    void Seek(double seconds);
    void SetVolume(int level);
    void Mute();
    void PlayNext();
    void PlayPrevious();

    event EventHandler? PlaybackEnded;
    event EventHandler<PlayState>? PlaybackStateChanged;
    event EventHandler<TimeSpan>? PositionChanged;
    event EventHandler<Song?>? CurrentSongChanged;
    event EventHandler<PlaybackMode>? PlaybackModeChanged;
}
