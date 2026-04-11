using System.Threading.Tasks;
using LocalMusicPlayer.Models;

namespace LocalMusicPlayer.Services;

public interface ICoverManagerService
{
    Task<AlbumCover?> ExtractCoverAsync(string filePath);

    Task<AlbumCover?> GetCachedCoverAsync(string album, string artist);

    Task<AlbumCover?> SetCoverFromFileAsync(string filePath, string imagePath);

    Task<bool> EmbedCoverAsync(string filePath, byte[] imageData);

    Task ClearCacheAsync();
}
