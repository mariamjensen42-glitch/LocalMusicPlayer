using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace LocalMusicPlayer.Converters;

/// <summary>
/// 将 AlternationIndex 转换为 1-based 索引（排名）
/// </summary>
public class IndexConverter : IValueConverter
{
    public static readonly IndexConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int index)
        {
            return (index + 1).ToString();
        }
        return "1";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}