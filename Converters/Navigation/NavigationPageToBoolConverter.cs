using System;
using System.Globalization;
using Avalonia.Data.Converters;
using LocalMusicPlayer.ViewModels;

namespace LocalMusicPlayer.Converters;

public class NavigationPageToBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is NavigationPage currentPage && parameter is NavigationPage targetPage)
            return currentPage == targetPage;
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
