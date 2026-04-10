using System;

namespace LocalMusicPlayer.Models;

public class ListeningHistoryItem
{
    public DateTime PlayedAt { get; set; }
    public Song Song { get; set; } = null!;
    public TimeSpan PlayedDuration { get; set; }
    public bool IsCompleted { get; set; }
    public double CompletionPercentage { get; set; }
}
