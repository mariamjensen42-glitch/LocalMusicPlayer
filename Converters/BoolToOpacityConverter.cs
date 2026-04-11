using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace LocalMusicPlayer.Converters;

public class BoolToOpacityConverter : IValueConverter
{
    private const double DefaultTrueOpacity = 1.0;
    private const double DefaultFalseOpacity = 0.3;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var trueOpacity = DefaultTrueOpacity;
        var falseOpacity = DefaultFalseOpacity;

        if (parameter is string param)
        {
            var parts = param.Split('|');
            if (parts.Length == 2)
            {
                if (double.TryParse(parts[0], out var t)) trueOpacity = t;
                if (double.TryParse(parts[1], out var f)) falseOpacity = f;
            }
            else if (parts.Length == 1)
            {
                if (double.TryParse(parts[0], out var t)) trueOpacity = t;
            }
        }

        if (value is bool b)
            return b ? trueOpacity : falseOpacity;
        return falseOpacity;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
