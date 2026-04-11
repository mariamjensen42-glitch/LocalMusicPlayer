using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace LocalMusicPlayer.Converters;

public class CountToBoolConverter : IValueConverter
{
    public static readonly CountToBoolConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool result = false;

        if (value is int count && parameter is string paramValue)
        {
            if (paramValue == "invert")
                result = count == 0;
            else
            {
                var targetCount = int.TryParse(paramValue, out var tc) ? tc : 0;
                result = count == targetCount;
            }
        }
        else if (value is int count2 && parameter is int targetCount2)
        {
            result = count2 == targetCount2;
        }

        return result;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
