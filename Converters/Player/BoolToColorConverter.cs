using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace LocalMusicPlayer.Converters;

public class BoolToColorConverter : IValueConverter
{
    private const string AccentColor = "#A855F7";
    private const string MutedColor = "#808080";

    private static readonly SolidColorBrush AccentBrush = new(Color.Parse(AccentColor));
    private static readonly SolidColorBrush MutedBrush = new(Color.Parse(MutedColor));
    private static readonly SolidColorBrush TransparentBrush = new(Colors.Transparent);

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

        return TransparentBrush;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
