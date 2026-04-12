using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace LocalMusicPlayer.Converters;

public class BoolToLyricFontWeightConverter : IValueConverter
{
    private static readonly FontWeight ActiveWeight = FontWeight.Bold;
    private static readonly FontWeight InactiveWeight = FontWeight.Normal;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is true)
            return ActiveWeight;
        return InactiveWeight;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
