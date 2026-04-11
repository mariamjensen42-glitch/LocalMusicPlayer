using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace LocalMusicPlayer.Converters;

public class LyricTranslationFontSizeConverter : IValueConverter
{
    private const double TranslationScaleFactor = 0.57;
    private const double DefaultFontSize = 16;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double fontSize)
            return Math.Round(fontSize * TranslationScaleFactor);
        return DefaultFontSize;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
