using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace LocalMusicPlayer.Converters;

public class BoolToColorConverter : IValueConverter
{
    private static readonly SolidColorBrush AccentBrush = new SolidColorBrush(Color.Parse("#A855F7"));
    private static readonly SolidColorBrush MutedBrush = new SolidColorBrush(Color.Parse("#808080"));

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isTrue && parameter is string param)
        {
            if (param == "accent")
            {
                return isTrue ? AccentBrush : MutedBrush;
            }

            var colors = param.Split('|');
            if (colors.Length == 2)
            {
                var colorStr = isTrue ? colors[0] : colors[1];
                if (Color.TryParse(colorStr, out var color))
                {
                    return new SolidColorBrush(color);
                }
            }
        }

        return new SolidColorBrush(Colors.Transparent);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
