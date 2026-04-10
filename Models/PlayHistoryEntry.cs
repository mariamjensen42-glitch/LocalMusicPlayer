using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LocalMusicPlayer.Models;

public partial class PlayHistoryEntry : ObservableObject
{
    [ObservableProperty] private Song _song;

    [ObservableProperty] private DateTime _playedAt;

    public string FormattedPlayedAt => PlayedAt switch
    {
        var dt when (DateTime.Now - dt).TotalMinutes < 1 => "刚刚",
        var dt when (DateTime.Now - dt).TotalHours < 1 => $"{(int)(DateTime.Now - dt).TotalMinutes} 分钟前",
        var dt when (DateTime.Now - dt).TotalDays < 1 => $"{(int)(DateTime.Now - dt).TotalHours} 小时前",
        var dt when (DateTime.Now - dt).TotalDays < 7 => $"{(int)(DateTime.Now - dt).TotalDays} 天前",
        _ => PlayedAt.ToString("yyyy-MM-dd")
    };

    public PlayHistoryEntry(Song song, DateTime playedAt)
    {
        _song = song;
        _playedAt = playedAt;
    }
}
