using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using LocalMusicPlayer.Models;

namespace LocalMusicPlayer.Converters;

public class SongIsPlayingConverter : IMultiValueConverter
{
    private static readonly SolidColorBrush ActiveBgBrush = new SolidColorBrush(Color.Parse("#20A855F7"));
    private static readonly SolidColorBrush ActiveBorderBrush = new SolidColorBrush(Color.Parse("#60A855F7"));
    private static readonly SolidColorBrush NormalBgBrush = new SolidColorBrush(Color.Parse("#FF111111"));
    private static readonly SolidColorBrush NormalBorderBrush = new SolidColorBrush(Color.Parse("#FF1A1A1A"));

    public object Convert(System.Collections.Generic.IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        // 支持两种模式：
        // 1. 2个值：listSong, currentSong - 用于背景和边框
        // 2. 3个值：listSongFilePath, currentSongFilePath, isPlaying - 用于按钮可见性

        bool isCurrentSongPlaying = false;
        string? param = parameter?.ToString();

        if (values.Count >= 3)
        {
            // 新模式：FilePath, FilePath, IsPlaying
            string? listSongPath = values[0]?.ToString();
            string? currentSongPath = values[1]?.ToString();
            bool isPlaying = values[2] is bool playing && playing;
            isCurrentSongPlaying = listSongPath == currentSongPath && isPlaying;
        }
        else if (values.Count >= 2 && values[0] is Song listSong && values[1] is Song currentSong)
        {
            // 旧模式：Song, Song
            isCurrentSongPlaying = listSong.FilePath == currentSong.FilePath;
        }

        // 如果请求 bool 类型（用于 IsVisible），返回 bool
        if (targetType == typeof(bool))
        {
            return param == "invert" ? !isCurrentSongPlaying : isCurrentSongPlaying;
        }

        // 根据parameter返回背景色或边框色
        if (param == "Border")
        {
            return isCurrentSongPlaying ? ActiveBorderBrush : NormalBorderBrush;
        }
        return isCurrentSongPlaying ? ActiveBgBrush : NormalBgBrush;
    }
}
