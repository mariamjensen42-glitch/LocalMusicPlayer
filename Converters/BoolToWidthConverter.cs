using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace LocalMusicPlayer.Converters;

public class BoolToWidthConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isOpen)
        {
            var width = parameter != null ? double.Parse(parameter.ToString()!) : 320;
            return isOpen ? width : 0;
        }

        return 0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double width)
        {
            var targetWidth = parameter != null ? double.Parse(parameter.ToString()!) : 320;
            return width > 0;
        }

        return false;
    }
}