using System;

namespace LocalMusicPlayer.Models;

public class Song
{
    public string Title { get; init; } = string.Empty;
    public string Artist { get; init; } = string.Empty;
    public string Album { get; init; } = string.Empty;
    public string FilePath { get; init; } = string.Empty;
    public TimeSpan Duration { get; init; }
}
