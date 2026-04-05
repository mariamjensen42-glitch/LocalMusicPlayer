using System;

namespace LocalMusicPlayer.Models;

/// <summary>
/// 歌曲播放统计数据，用于持久化存储
/// </summary>
public class SongStatistics
{
    /// <summary>
    /// 歌曲文件路径，作为唯一标识
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// 播放次数
    /// </summary>
    public int PlayCount { get; set; }

    /// <summary>
    /// 最后播放时间
    /// </summary>
    public DateTime? LastPlayedTime { get; set; }

    /// <summary>
    /// 累计播放时长（毫秒）
    /// </summary>
    public long TotalPlayedDurationMs { get; set; }
}