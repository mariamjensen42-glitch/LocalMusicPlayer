using System;
using System.Globalization;
using System.IO;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;

namespace LocalMusicPlayer.Converters;

public class AlbumArtConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string path && !string.IsNullOrEmpty(path))
        {
            try
            {
                if (File.Exists(path))
                {
                    using var stream = File.OpenRead(path);
                    return new Bitmap(stream);
                }
            }
            catch
            {
                // 加载失败，返回默认封面
            }
        }

        // 返回默认封面（从资源加载）
        try
        {
            var uri = new Uri("avares://LocalMusicPlayer/Assets/default-album-art.png");
            using var stream = Avalonia.Platform.AssetLoader.Open(uri);
            return new Bitmap(stream);
        }
        catch
        {
            return null;
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}