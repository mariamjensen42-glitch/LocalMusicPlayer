using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace LocalMusicPlayer.Converters;

public class BoolToFontWeightConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is true)
            return FontWeight.Bold;
        return FontWeight.SemiBold;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
