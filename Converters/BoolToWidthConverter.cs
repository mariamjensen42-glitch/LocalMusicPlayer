using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace LocalMusicPlayer.Converters;

public class BoolToWidthConverter : IValueConverter
{
    private const double DefaultWidth = 320;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isOpen)
        {
            var width = double.TryParse(parameter?.ToString(), out var w) ? w : DefaultWidth;
            return isOpen ? width : 0;
        }

        return 0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double width)
        {
            var targetWidth = double.TryParse(parameter?.ToString(), out var w) ? w : DefaultWidth;
            return width > 0;
        }

        return false;
    }
}
