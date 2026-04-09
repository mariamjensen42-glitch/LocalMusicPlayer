using System;

namespace LocalMusicPlayer.ViewModels;

public interface IPlaybackProgress
{
    TimeSpan Position { get; }
    TimeSpan Duration { get; }
    double PositionSeconds { get; }
    double DurationSeconds { get; }
}