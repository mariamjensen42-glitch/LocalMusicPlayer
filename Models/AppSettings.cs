using System;
using System.Collections.Generic;

namespace LocalMusicPlayer.Models;

public class AppSettings
{
    public List<string> ScanFolders { get; set; } = new();
    public bool IncludeSubfolders { get; set; } = true;
    public int Volume { get; set; } = 80;
    public bool IsMuted { get; set; }
    public DateTime? LastScanTime { get; set; }
    public string Theme { get; set; } = "Dark";
    public string PlaybackMode { get; set; } = "Normal";
    public float PlaybackRate { get; set; } = 1.0f;
    public bool CrossfadeEnabled { get; set; }
    public int CrossfadeDurationMs { get; set; } = 3000;
    public double LyricFontSize { get; set; } = 28;
    public double LyricLineSpacing { get; set; } = 20;
    public bool ShowTranslation { get; set; } = true;
    public bool ReplayGainEnabled { get; set; } = true;

    /// <summary>
    /// 歌曲统计数据，Key 为文件路径
    /// </summary>
    public Dictionary<string, SongStatistics> SongStatistics { get; set; } = new();

    /// <summary>
    /// 总播放时长（毫秒）
    /// </summary>
    public long TotalPlayTimeMs { get; set; }

    /// <summary>
    /// 首次扫描日期，用于计算使用天数
    /// </summary>
    public DateTime? FirstScanDate { get; set; }

    /// <summary>
    /// 用户自定义播放列表
    /// </summary>
    public List<UserPlaylist> UserPlaylists { get; set; } = new();

    /// <summary>
    /// 收藏歌曲文件路径列表
    /// </summary>
    public List<string> FavoriteFilePaths { get; set; } = new();

    /// <summary>
    /// 播放历史记录
    /// </summary>
    public List<PlayHistoryRecord> PlayHistory { get; set; } = new();
}