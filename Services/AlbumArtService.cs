using System;
using System.Collections.Concurrent;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using TagLib;

namespace LocalMusicPlayer.Services;

public class AlbumArtService : IAlbumArtService
{
    private readonly string _cacheDirectory;
    private readonly ConcurrentDictionary<string, Bitmap> _memoryCache = new();
    private readonly string _defaultArtPath;

    public string CacheDirectory => _cacheDirectory;
    public string DefaultArtPath => _defaultArtPath;

    public AlbumArtService()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _cacheDirectory = Path.Combine(appDataPath, "LocalMusicPlayer", "covers");
        _defaultArtPath = "avares://LocalMusicPlayer/Assets/default-album-art.png";

        EnsureCacheDirectoryExists();
    }

    private void EnsureCacheDirectoryExists()
    {
        if (!Directory.Exists(_cacheDirectory))
        {
            Directory.CreateDirectory(_cacheDirectory);
        }
    }

    private string GetCacheFileName(string filePath)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(filePath));
        var hash = BitConverter.ToString(hashBytes).Replace("-", "").Substring(0, 16);
        return Path.Combine(_cacheDirectory, $"{hash}.jpg");
    }

    public async Task<string?> ExtractAlbumArtAsync(string filePath)
    {
        try
        {
            var cachePath = GetCacheFileName(filePath);

            // 如果缓存已存在，直接返回
            if (System.IO.File.Exists(cachePath))
            {
                return cachePath;
            }

            // 从音频文件提取封面
            using var file = TagLib.File.Create(filePath);
            if (file.Tag.Pictures.Length > 0)
            {
                var picture = file.Tag.Pictures[0];
                var imageData = picture.Data.Data;

                if (imageData != null && imageData.Length > 0)
                {
                    await System.IO.File.WriteAllBytesAsync(cachePath, imageData);
                    return cachePath;
                }
            }

            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<Bitmap?> GetAlbumArtBitmapAsync(string? cachePath)
    {
        if (string.IsNullOrEmpty(cachePath))
        {
            return null;
        }

        // 检查内存缓存
        if (_memoryCache.TryGetValue(cachePath, out var cachedBitmap))
        {
            return cachedBitmap;
        }

        try
        {
            // 从磁盘加载
            if (System.IO.File.Exists(cachePath))
            {
                await using var stream = System.IO.File.OpenRead(cachePath);
                var bitmap = new Bitmap(stream);
                _memoryCache[cachePath] = bitmap;
                return bitmap;
            }
        }
        catch (Exception)
        {
            // 加载失败，返回 null
        }

        return null;
    }

    public async Task ClearCacheAsync()
    {
        _memoryCache.Clear();

        try
        {
            if (Directory.Exists(_cacheDirectory))
            {
                var files = Directory.GetFiles(_cacheDirectory);
                foreach (var file in files)
                {
                    System.IO.File.Delete(file);
                }
            }
        }
        catch (Exception)
        {
            // 忽略清除错误
        }

        await Task.CompletedTask;
    }
}