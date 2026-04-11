using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;

namespace LocalMusicPlayer.Converters;

public class AlbumArtConverter : IValueConverter
{
    public static readonly AlbumArtConverter Instance = new();

    private static readonly ConcurrentDictionary<string, WeakReference<Bitmap>> Cache = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string path && !string.IsNullOrEmpty(path))
        {
            try
            {
                if (Cache.TryGetValue(path, out var weakRef) && weakRef.TryGetTarget(out var cached))
                    return cached;

                if (File.Exists(path))
                {
                    using var stream = File.OpenRead(path);
                    var bitmap = new Bitmap(stream);
                    Cache[path] = new WeakReference<Bitmap>(bitmap);
                    return bitmap;
                }
            }
            catch (IOException)
            {
                return GetDefaultCover();
            }
            catch (UnauthorizedAccessException)
            {
                return GetDefaultCover();
            }
        }

        return GetDefaultCover();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    private static Bitmap? GetDefaultCover()
    {
        try
        {
            var uri = new Uri("avares://LocalMusicPlayer/Assets/default-album-art.png");
            using var stream = Avalonia.Platform.AssetLoader.Open(uri);
            return new Bitmap(stream);
        }
        catch (IOException)
        {
            return null;
        }
    }
}
