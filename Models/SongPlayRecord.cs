using System;

namespace LocalMusicPlayer.Models;

/// <summary>
/// 排行榜记录，用于显示最常听歌曲
/// </summary>
public class SongPlayRecord
{
    /// <summary>
    /// 排名
    /// </summary>
    public int Rank { get; set; }

    /// <summary>
    /// 歌曲信息
    /// </summary>
    public Song Song { get; set; } = null!;

    /// <summary>
    /// 播放次数
    /// </summary>
    public int PlayCount { get; set; }

    /// <summary>
    /// 最后播放时间
    /// </summary>
    public DateTime? LastPlayedTime { get; set; }

    /// <summary>
    /// 格式化的最后播放时间显示
    /// </summary>
    public string FormattedLastPlayed
    {
        get
        {
            if (LastPlayedTime == null)
                return "Never";

            var diff = DateTime.Now - LastPlayedTime.Value;

            if (diff.TotalMinutes < 1)
                return "Just now";
            if (diff.TotalHours < 1)
                return $"{(int)diff.TotalMinutes}m ago";
            if (diff.TotalDays < 1)
                return $"{(int)diff.TotalHours}h ago";
            if (diff.TotalDays < 7)
                return $"{(int)diff.TotalDays}d ago";

            return LastPlayedTime.Value.ToString("MM/dd");
        }
    }
}