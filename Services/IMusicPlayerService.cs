using System;
using LocalMusicPlayer.Models;

namespace LocalMusicPlayer.Services;

public interface IMusicPlayerService
{
    void Play(Song song);
    void Pause();
    void Resume();
    void Stop();
    void Next();
    void Previous();
    void Seek(TimeSpan position);
    void SetVolume(int level);
    void Mute();

    TimeSpan Position { get; }
    TimeSpan Duration { get; }
    int Volume { get; }
    bool IsPlaying { get; }
    bool IsMuted { get; }

    event EventHandler? PlaybackEnded;
    event EventHandler<PlayState>? PlaybackStateChanged;
    event EventHandler<TimeSpan>? PositionChanged;
}