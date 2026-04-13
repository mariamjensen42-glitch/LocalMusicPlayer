using System;
using System.Globalization;
using Avalonia.Data.Converters;
using LocalMusicPlayer.Models;

namespace LocalMusicPlayer.Converters;

public class PlayModeToIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is PlaybackMode mode)
        {
            return mode switch
            {
                PlaybackMode.SingleLoop => "\ue8ed",
                PlaybackMode.Loop => "\ue8ee",
                PlaybackMode.Shuffle => "\ue8b1",
                _ => "\ue8ee"
            };
        }
        return "\ue8ee";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
