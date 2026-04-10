using System;

namespace LocalMusicPlayer.Models;

public class PlayHistoryRecord
{
    public string FilePath { get; set; } = string.Empty;
    public DateTime PlayedAt { get; set; }
}