using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LocalMusicPlayer.Models;

namespace LocalMusicPlayer.Services;

public interface IStatisticsService
{
    /// <summary>
    /// 总播放次数
    /// </summary>
    int TotalPlayCount { get; }

    /// <summary>
    /// 总播放时长
    /// </summary>
    TimeSpan TotalPlayTime { get; }

    /// <summary>
    /// 使用天数
    /// </summary>
    int UsageDays { get; }

    /// <summary>
    /// 获取音乐库统计
    /// </summary>
    LibraryStatistics GetLibraryStatistics();

    /// <summary>
    /// 记录播放开始
    /// </summary>
    void RecordPlayStart(Song song);

    /// <summary>
    /// 记录播放完成（由服务内部根据播放时长判定是否为有效播放）
    /// </summary>
    void RecordPlayEnd();

    /// <summary>
    /// 获取最常听歌曲排行
    /// </summary>
    IReadOnlyList<SongPlayRecord> GetTopPlayedSongs(int count);

    /// <summary>
    /// 获取最近播放歌曲
    /// </summary>
    IReadOnlyList<SongPlayRecord> GetRecentlyPlayedSongs(int count);

    /// <summary>
    /// 保存统计数据
    /// </summary>
    Task SaveStatisticsAsync();

    /// <summary>
    /// 加载统计数据
    /// </summary>
    Task LoadStatisticsAsync();

    /// <summary>
    /// 统计数据变更事件
    /// </summary>
    event EventHandler? StatisticsChanged;
}