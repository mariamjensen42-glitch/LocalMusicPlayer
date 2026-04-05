using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace LocalMusicPlayer.Converters;

/// <summary>
/// 将集合数量转换为布尔值，用于控制无数据提示的显示
/// 当数量等于指定参数值时返回 true
/// </summary>
public class CountToBoolConverter : IValueConverter
{
    public static readonly CountToBoolConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int count && parameter is string paramValue)
        {
            var targetCount = int.Parse(paramValue);
            return count == targetCount;
        }

        if (value is int count2 && parameter is int targetCount2)
        {
            return count2 == targetCount2;
        }

        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}