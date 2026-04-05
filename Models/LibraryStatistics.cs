using System;

namespace LocalMusicPlayer.Models;

/// <summary>
/// 音乐库统计结果
/// </summary>
public class LibraryStatistics
{
    public int TotalSongs { get; set; }
    public int TotalAlbums { get; set; }
    public int TotalArtists { get; set; }
    public TimeSpan TotalDuration { get; set; }

    /// <summary>
    /// 格式化的总时长显示
    /// </summary>
    public string FormattedDuration
    {
        get
        {
            var hours = (int)TotalDuration.TotalHours;
            var minutes = TotalDuration.Minutes;
            return hours > 0 ? $"{hours}h {minutes}m" : $"{minutes}m";
        }
    }
}