using System.Threading.Tasks;
using Avalonia.Media.Imaging;

namespace LocalMusicPlayer.Services;

public interface IAlbumArtService
{
    string CacheDirectory { get; }
    string DefaultArtPath { get; }
    Task<string?> ExtractAlbumArtAsync(string filePath);
    Task<Bitmap?> GetAlbumArtBitmapAsync(string? cachePath);
    Task ClearCacheAsync();
}