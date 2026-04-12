using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace LocalMusicPlayer.Converters;

public class CountToBoolConverter : IValueConverter
{
    public static readonly CountToBoolConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int count)
            return false;

        if (parameter is string paramValue)
        {
            if (paramValue == "invert")
                return count == 0;
            if (int.TryParse(paramValue, out var targetCount))
                return count == targetCount;
            return count > 0;
        }

        if (parameter is int targetCount2)
            return count == targetCount2;

        return count > 0;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
