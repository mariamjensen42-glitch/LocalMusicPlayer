using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using LocalMusicPlayer.Models;

namespace LocalMusicPlayer.Converters;

public class SongIsPlayingConverter : IMultiValueConverter
{
    private const string ActiveBgColor = "#20A855F7";
    private const string ActiveBorderColor = "#60A855F7";
    private const string NormalBgColor = "#FF111111";
    private const string NormalBorderColor = "#FF1A1A1A";
    private const string ActiveFgColor = "#FFA855F7";
    private const string NormalFgColor = "#FFFFFFFF";

    private static readonly SolidColorBrush ActiveBgBrush = new(Color.Parse(ActiveBgColor));
    private static readonly SolidColorBrush ActiveBorderBrush = new(Color.Parse(ActiveBorderColor));
    private static readonly SolidColorBrush NormalBgBrush = new(Color.Parse(NormalBgColor));
    private static readonly SolidColorBrush NormalBorderBrush = new(Color.Parse(NormalBorderColor));
    private static readonly SolidColorBrush ActiveFgBrush = new(Color.Parse(ActiveFgColor));
    private static readonly SolidColorBrush NormalFgBrush = new(Color.Parse(NormalFgColor));

    public object Convert(System.Collections.Generic.IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        bool isCurrentSongPlaying = false;
        string? param = parameter?.ToString();

        if (values.Count >= 3)
        {
            string? listSongPath = values[0]?.ToString();
            string? currentSongPath = values[1]?.ToString();
            bool isPlaying = values[2] is bool playing && playing;
            isCurrentSongPlaying = listSongPath == currentSongPath && isPlaying;
        }
        else if (values.Count >= 2 && values[0] is Song listSong && values[1] is Song currentSong)
        {
            isCurrentSongPlaying = listSong.FilePath == currentSong.FilePath;
        }

        if (targetType == typeof(bool))
        {
            return param == "invert" ? !isCurrentSongPlaying : isCurrentSongPlaying;
        }

        if (param == "Border")
        {
            return isCurrentSongPlaying ? ActiveBorderBrush : NormalBorderBrush;
        }
        return isCurrentSongPlaying ? ActiveFgBrush : NormalFgBrush;
    }
}
