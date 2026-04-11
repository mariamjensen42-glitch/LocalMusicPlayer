using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace LocalMusicPlayer.Converters;

public class EnumBooleanConverter : IValueConverter
{
    public static readonly EnumBooleanConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
            return false;

        return string.Equals(value.ToString(), parameter.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue && boolValue && parameter != null)
        {
            var paramStr = parameter.ToString();
            if (targetType.IsEnum && paramStr != null)
                return Enum.Parse(targetType, paramStr, true);
            return paramStr;
        }

        return Avalonia.Data.BindingOperations.DoNothing;
    }
}
