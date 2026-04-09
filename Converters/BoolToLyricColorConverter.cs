using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace LocalMusicPlayer.Converters;

public class BoolToLyricColorConverter : IValueConverter
{
    private static readonly SolidColorBrush ActiveBrush = new(Colors.White);
    private static readonly SolidColorBrush InactiveBrush = new(Color.Parse("#FF71717A"));

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is true)
            return ActiveBrush;
        return InactiveBrush;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
