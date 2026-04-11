using System;
using System.Globalization;
using Avalonia.Data.Converters;
using LocalMusicPlayer.ViewModels;

namespace LocalMusicPlayer.Converters;

public class NavPageVisibilityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is NavigationPage current && parameter is NavigationPage target)
        {
            return current != target;
        }
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
