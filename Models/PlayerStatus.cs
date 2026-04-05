using System;

namespace LocalMusicPlayer.Models;

public class PlayerStatus
{
    public TimeSpan Position { get; init; }
    public TimeSpan Duration { get; init; }
    public bool IsPlaying { get; init; }
    public int Volume { get; init; }
    public bool IsMuted { get; init; }
    public PlayState State { get; init; }
    public Song? CurrentSong { get; init; }
}
