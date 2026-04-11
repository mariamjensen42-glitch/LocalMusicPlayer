using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using LocalMusicPlayer.Models;

namespace LocalMusicPlayer.Services;

public interface ILyricsService
{
    List<LyricLine> GetLyrics(string filePath);
    int GetCurrentLyricIndex(List<LyricLine> lyrics, TimeSpan position);
}

public partial class LyricLine : ObservableObject
{
    public TimeSpan Timestamp { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? Translation { get; set; }

    [ObservableProperty]
    private bool _isActive;
}
