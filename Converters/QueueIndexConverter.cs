using System;
using System.Collections;
using System.Globalization;
using Avalonia.Data.Converters;

namespace LocalMusicPlayer.Converters;

public class QueueIndexConverter : IMultiValueConverter
{
    public object Convert(System.Collections.Generic.IList<object?> values, Type targetType, object? parameter,
        CultureInfo culture)
    {
        if (values.Count >= 2 && values[0] is IList list && values[1] != null)
        {
            var index = list.IndexOf(values[1]);
            return index >= 0 ? (index + 1).ToString() : "0";
        }

        return "0";
    }
}