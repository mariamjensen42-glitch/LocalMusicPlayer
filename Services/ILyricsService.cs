using System;
using System.Collections.Generic;
using LocalMusicPlayer.Models;
using ReactiveUI;

namespace LocalMusicPlayer.Services;

public interface ILyricsService
{
    /// <summary>
    /// 从音频文件中读取歌词
    /// </summary>
    /// <param name="filePath">音频文件路径</param>
    /// <returns>歌词列表，如果没有歌词则返回空列表</returns>
    List<LyricLine> GetLyrics(string filePath);

    /// <summary>
    /// 根据当前播放位置获取当前歌词索引
    /// </summary>
    /// <param name="lyrics">歌词列表</param>
    /// <param name="position">当前播放位置</param>
    /// <returns>当前歌词索引，如果没有匹配的歌词返回 -1</returns>
    int GetCurrentLyricIndex(List<LyricLine> lyrics, TimeSpan position);
}

public class LyricLine : ReactiveObject
{
    public TimeSpan Timestamp { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? Translation { get; set; }

    private bool _isActive;

    public bool IsActive
    {
        get => _isActive;
        set => this.RaiseAndSetIfChanged(ref _isActive, value);
    }
}
