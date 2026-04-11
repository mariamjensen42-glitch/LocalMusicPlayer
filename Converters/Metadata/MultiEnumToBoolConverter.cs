using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace LocalMusicPlayer.Converters;

public class MultiEnumToBoolConverter : IMultiValueConverter
{
    public static readonly MultiEnumToBoolConverter Instance = new();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values == null || values.Count < 2)
            return false;

        for (var i = 0; i < values.Count - 1; i += 2)
        {
            var currentValue = values[i];
            var targetValue = values[i + 1];

            if (currentValue == null || targetValue == null)
                return false;

            if (!Equals(currentValue, targetValue))
                return false;
        }

        return true;
    }
}
