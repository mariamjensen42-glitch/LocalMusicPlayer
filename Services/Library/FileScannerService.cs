using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TagLib;
using LocalMusicPlayer.Models;

namespace LocalMusicPlayer.Services;

public class FileScannerService : IFileScannerService
{
    private readonly IAlbumArtService _albumArtService;

    public IReadOnlyList<string> SupportedExtensions { get; } =
    [
        // 常用格式
        ".mp3", ".flac", ".wav", ".aac", ".ogg", ".m4a",
        // DSD 格式
        ".dsf", ".dff",
        // 无损格式
        ".ape", ".aiff", ".aif",
        // 其他常用格式
        ".wma", ".opus", ".webm", ".ac3", ".dts"
    ];

    public FileScannerService(IAlbumArtService albumArtService)
    {
        _albumArtService = albumArtService;
    }

    public Task<List<Song>> ScanDirectoryAsync(string path, bool includeSubfolders = true)
    {
        return ScanDirectoryAsync(path, null, CancellationToken.None, includeSubfolders);
    }

    public async Task<List<Song>> ScanDirectoryAsync(string path, IProgress<int>? progress,
        CancellationToken cancellationToken)
    {
        return await ScanDirectoryAsync(path, progress, cancellationToken, true);
    }

    private async Task<List<Song>> ScanDirectoryAsync(string path, IProgress<int>? progress,
        CancellationToken cancellationToken, bool includeSubfolders)
    {
        var songs = new List<Song>();

        if (!Directory.Exists(path))
        {
            return songs;
        }

        var searchOption = includeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        var files = Directory.EnumerateFiles(path, "*.*", searchOption)
            .Where(f => SupportedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
            .ToList();

        var total = files.Count;
        var processed = 0;

        await Task.Run(async () =>
        {
            foreach (var file in files)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    var song = await ReadSongMetadataAsync(file);
                    songs.Add(song);
                }
                catch
                {
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    songs.Add(new Song
                    {
                        Title = fileName,
                        FilePath = file
                    });
                }

                processed++;
                progress?.Report((int)((double)processed / total * 100));
            }
        }, cancellationToken);

        return songs;
    }

    private async Task<Song> ReadSongMetadataAsync(string filePath)
    {
        try
        {
            using var file = TagLib.File.Create(filePath);
            var albumArtPath = await _albumArtService.ExtractAlbumArtAsync(filePath);
            var fileInfo = new FileInfo(filePath);

            return new Song
            {
                Title = string.IsNullOrWhiteSpace(file.Tag.Title)
                    ? Path.GetFileNameWithoutExtension(filePath)
                    : file.Tag.Title,
                Artist = file.Tag.FirstPerformer ?? string.Empty,
                Album = file.Tag.Album ?? string.Empty,
                FilePath = filePath,
                Duration = file.Properties.Duration,
                AlbumArtPath = albumArtPath,
                FileSizeBytes = fileInfo.Length
            };
        }
        catch
        {
            var fileInfo = new FileInfo(filePath);
            return new Song
            {
                Title = Path.GetFileNameWithoutExtension(filePath),
                FilePath = filePath,
                FileSizeBytes = fileInfo.Exists ? fileInfo.Length : 0
            };
        }
    }
}
