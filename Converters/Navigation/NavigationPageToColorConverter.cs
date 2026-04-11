using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using LocalMusicPlayer.ViewModels;

namespace LocalMusicPlayer.Converters;

public class NavigationPageToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is NavigationPage currentPage && parameter is NavigationPage targetPage)
        {
            return currentPage == targetPage
                ? new SolidColorBrush(Color.Parse("#A855F7"))
                : new SolidColorBrush(Color.Parse("#9CA3AF"));
        }

        return new SolidColorBrush(Color.Parse("#9CA3AF"));
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
