using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace LocalMusicPlayer.Converters;

public class FavouriteIconConverter : IValueConverter
{
    private const string FavouriteIcon = "\ue87d";
    private const string NotFavouriteIcon = "\ue87c";

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isFavourite)
            return isFavourite ? FavouriteIcon : NotFavouriteIcon;
        return NotFavouriteIcon;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string icon)
            return string.Equals(icon, FavouriteIcon, StringComparison.Ordinal);
        return false;
    }
}
