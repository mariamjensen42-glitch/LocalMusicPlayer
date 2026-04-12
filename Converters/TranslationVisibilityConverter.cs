using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace LocalMusicPlayer.Converters;

public class TranslationVisibilityConverter : IMultiValueConverter
{
    public static readonly TranslationVisibilityConverter Instance = new();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 2)
            return false;

        var translation = values[0] as string;
        var showTranslation = values[1] is bool show && show;

        return showTranslation && !string.IsNullOrEmpty(translation);
    }
}
