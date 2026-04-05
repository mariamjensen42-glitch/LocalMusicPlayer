using System;
using LocalMusicPlayer.Models;

namespace LocalMusicPlayer.Services;

public interface IMusicPlayerService
{
    void Play(Song song);
    void Pause();
    void Resume();
    void Stop();
    TimeSpan Position { get; }
    TimeSpan Duration { get; }
    bool IsPlaying { get; }
}
