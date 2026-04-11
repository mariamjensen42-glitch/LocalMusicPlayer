using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace LocalMusicPlayer.Converters;

public class StringToBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string hexColor && !string.IsNullOrEmpty(hexColor))
        {
            try
            {
                var color = Color.Parse(hexColor);
                return new SolidColorBrush(color);
            }
            catch
            {
                return new SolidColorBrush(Color.Parse("#1E1E2E"));
            }
        }

        return new SolidColorBrush(Color.Parse("#1E1E2E"));
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
