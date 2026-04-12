using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia;
using LocalMusicPlayer.Models;
using TagLib;
using TagLibFile = TagLib.File;
using SystemIOFile = System.IO.File;

namespace LocalMusicPlayer.Services;

internal class CoverManagerService : ICoverManagerService
{
    private readonly string _cacheDirectory;
    private readonly int _maxCoverSize = 800;

    public CoverManagerService()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _cacheDirectory = Path.Combine(appDataPath, "LocalMusicPlayer", "covercache");
        EnsureCacheDirectoryExists();
    }

    private void EnsureCacheDirectoryExists()
    {
        if (!Directory.Exists(_cacheDirectory))
        {
            Directory.CreateDirectory(_cacheDirectory);
        }
    }

    private string GetCacheFilePath(string album, string artist)
    {
        var key = $"{artist}_{album}";
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(key));
        var hash = BitConverter.ToString(hashBytes).Replace("-", "").Substring(0, 16);
        return Path.Combine(_cacheDirectory, $"{hash}.jpg");
    }

    public async Task<AlbumCover?> ExtractCoverAsync(string filePath)
    {
        try
        {
            if (!SystemIOFile.Exists(filePath))
            {
                return null;
            }

            using var file = TagLibFile.Create(filePath);
            if (file.Tag.Pictures.Length == 0)
            {
                return null;
            }

            var picture = file.Tag.Pictures[0];
            var imageData = picture.Data.Data;

            if (imageData == null || imageData.Length == 0)
            {
                return null;
            }

            var album = file.Tag.Album ?? "Unknown Album";
            var artist = file.Tag.FirstPerformer ?? file.Tag.FirstAlbumArtist ?? "Unknown Artist";
            var cachePath = GetCacheFilePath(album, artist);

            var processedData = await ProcessImageAsync(imageData);
            await SystemIOFile.WriteAllBytesAsync(cachePath, processedData);

            var (width, height) = await GetImageDimensionsAsync(processedData);

            return new AlbumCover
            {
                Album = album,
                Artist = artist,
                CoverPath = cachePath,
                CachedAt = DateTime.Now,
                FileSizeBytes = processedData.Length,
                Width = width,
                Height = height,
                MimeType = "image/jpeg"
            };
        }
        catch (CorruptFileException)
        {
            return null;
        }
        catch (UnsupportedFormatException)
        {
            return null;
        }
        catch (IOException)
        {
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<AlbumCover?> GetCachedCoverAsync(string album, string artist)
    {
        try
        {
            var cachePath = GetCacheFilePath(album, artist);

            if (!SystemIOFile.Exists(cachePath))
            {
                return null;
            }

            var fileInfo = new FileInfo(cachePath);
            var (width, height) = await GetImageDimensionsAsync(await SystemIOFile.ReadAllBytesAsync(cachePath));

            return new AlbumCover
            {
                Album = album,
                Artist = artist,
                CoverPath = cachePath,
                CachedAt = fileInfo.LastWriteTime,
                FileSizeBytes = fileInfo.Length,
                Width = width,
                Height = height,
                MimeType = "image/jpeg"
            };
        }
        catch (IOException)
        {
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<AlbumCover?> SetCoverFromFileAsync(string filePath, string imagePath)
    {
        try
        {
            if (!SystemIOFile.Exists(filePath) || !SystemIOFile.Exists(imagePath))
            {
                return null;
            }

            var imageData = await SystemIOFile.ReadAllBytesAsync(imagePath);
            var processedData = await ProcessImageAsync(imageData);

            using var file = TagLibFile.Create(filePath);
            var album = file.Tag.Album ?? "Unknown Album";
            var artist = file.Tag.FirstPerformer ?? file.Tag.FirstAlbumArtist ?? "Unknown Artist";

            var cachePath = GetCacheFilePath(album, artist);
            await SystemIOFile.WriteAllBytesAsync(cachePath, processedData);

            var (width, height) = await GetImageDimensionsAsync(processedData);

            return new AlbumCover
            {
                Album = album,
                Artist = artist,
                CoverPath = cachePath,
                CachedAt = DateTime.Now,
                FileSizeBytes = processedData.Length,
                Width = width,
                Height = height,
                MimeType = "image/jpeg"
            };
        }
        catch (CorruptFileException)
        {
            return null;
        }
        catch (UnsupportedFormatException)
        {
            return null;
        }
        catch (IOException)
        {
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<bool> EmbedCoverAsync(string filePath, byte[] imageData)
    {
        try
        {
            if (!SystemIOFile.Exists(filePath) || imageData == null || imageData.Length == 0)
            {
                return false;
            }

            var processedData = await ProcessImageAsync(imageData);

            using var file = TagLibFile.Create(filePath);
            var picture = new Picture(new ByteVector(processedData, processedData.Length))
            {
                Type = PictureType.FrontCover,
                MimeType = "image/jpeg",
                Description = "Cover"
            };

            file.Tag.Pictures = new[] { picture };
            file.Save();

            return true;
        }
        catch (CorruptFileException)
        {
            return false;
        }
        catch (UnsupportedFormatException)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task ClearCacheAsync()
    {
        try
        {
            if (!Directory.Exists(_cacheDirectory))
            {
                return;
            }

            var files = Directory.GetFiles(_cacheDirectory, "*.jpg");
            foreach (var file in files)
            {
                try
                {
                    SystemIOFile.Delete(file);
                }
                catch (IOException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
            }

            await Task.CompletedTask;
        }
        catch (DirectoryNotFoundException)
        {
        }
        catch (Exception)
        {
        }
    }

    private async Task<byte[]> ProcessImageAsync(byte[] imageData)
    {
        return await Task.Run(() =>
        {
            try
            {
                using var inputStream = new MemoryStream(imageData);
                using var bitmap = new Bitmap(inputStream);

                var originalWidth = bitmap.PixelSize.Width;
                var originalHeight = bitmap.PixelSize.Height;

                if (originalWidth <= _maxCoverSize && originalHeight <= _maxCoverSize)
                {
                    return imageData;
                }

                double scale;
                if (originalWidth > originalHeight)
                {
                    scale = (double)_maxCoverSize / originalWidth;
                }
                else
                {
                    scale = (double)_maxCoverSize / originalHeight;
                }

                var newWidth = (int)(originalWidth * scale);
                var newHeight = (int)(originalHeight * scale);

                using var resizedBitmap = bitmap.CreateScaledBitmap(new PixelSize(newWidth, newHeight));
                using var outputStream = new MemoryStream();
                resizedBitmap.Save(outputStream);

                return outputStream.ToArray();
            }
            catch (Exception)
            {
                return imageData;
            }
        });
    }

    private async Task<(int width, int height)> GetImageDimensionsAsync(byte[] imageData)
    {
        return await Task.Run(() =>
        {
            try
            {
                using var stream = new MemoryStream(imageData);
                using var bitmap = new Bitmap(stream);
                return (bitmap.PixelSize.Width, bitmap.PixelSize.Height);
            }
            catch (Exception)
            {
                return (0, 0);
            }
        });
    }
}
