using System;

namespace LocalMusicPlayer.Models;

public enum SmartPlaylistRule
{
    MostPlayed,      // 播放次数最多
    RecentlyPlayed,  // 最近播放
    LeastPlayed,     // 播放次数最少
    RecentlyAdded,    // 最近添加
    NeverPlayed,     // 从未播放
}

public class SmartPlaylist
{
    public string Name { get; set; } = string.Empty;
    public SmartPlaylistRule Rule { get; set; }
    public int Limit { get; set; } = 50;
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
