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
        if (values == null || values.Count < 2 || values[0] == null || values[1] == null)
            return false;

        return values[0].Equals(values[1]);
    }
}
