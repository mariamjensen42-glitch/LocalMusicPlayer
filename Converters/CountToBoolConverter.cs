using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace LocalMusicPlayer.Converters;

public class CountToBoolConverter : IValueConverter
{
    public static readonly CountToBoolConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int count && parameter is string paramValue)
        {
            var targetCount = int.TryParse(paramValue, out var tc) ? tc : 0;
            return count == targetCount;
        }

        if (value is int count2 && parameter is int targetCount2)
        {
            return count2 == targetCount2;
        }

        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
