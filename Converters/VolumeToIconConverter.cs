using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace LocalMusicPlayer.Converters;

public class VolumeToIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double volume)
        {
            return volume switch
            {
                > 75 => "\ue995",
                > 50 => "\ue994",
                > 25 => "\ue993",
                > 0 => "\ue992",
                _ => "\ue74f"
            };
        }
        if (value is int intVolume)
        {
            return intVolume switch
            {
                > 75 => "\ue995",
                > 50 => "\ue994",
                > 25 => "\ue993",
                > 0 => "\ue992",
                _ => "\ue74f"
            };
        }
        return "\ue74f";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
