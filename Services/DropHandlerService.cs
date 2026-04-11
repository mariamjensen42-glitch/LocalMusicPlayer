using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace LocalMusicPlayer.Services;

public class DropHandlerService : IDropHandlerService
{
    private readonly IFileScannerService _fileScannerService;
    private readonly IMusicLibraryService _musicLibraryService;

    public DropHandlerService(IFileScannerService fileScannerService, IMusicLibraryService musicLibraryService)
    {
        _fileScannerService = fileScannerService;
        _musicLibraryService = musicLibraryService;
    }

    public async Task HandleDroppedFilesAsync(IEnumerable<string> filePaths)
    {
        foreach (var path in filePaths)
        {
            if (string.IsNullOrEmpty(path))
                continue;

            if (File.Exists(path) && _fileScannerService.SupportedExtensions.Contains(
                    Path.GetExtension(path).ToLowerInvariant()))
            {
                await AddSingleFileAsync(path);
            }
            else if (Directory.Exists(path))
            {
                await AddFolderAsync(path);
            }
        }
    }

    private async Task AddSingleFileAsync(string filePath)
    {
        try
        {
            var directory = Path.GetDirectoryName(filePath);
            if (directory == null) return;

            var songs = await _fileScannerService.ScanDirectoryAsync(directory, false);
            var song = songs.FirstOrDefault(s => s.FilePath == filePath);
            if (song != null && !_musicLibraryService.Songs.Any(s => s.FilePath == filePath))
            {
                _musicLibraryService.AddSong(song);
            }
        }
        catch
        {
        }
    }

    private async Task AddFolderAsync(string folderPath)
    {
        try
        {
            var songs = await _fileScannerService.ScanDirectoryAsync(folderPath, true);
            foreach (var song in songs)
            {
                if (!_musicLibraryService.Songs.Any(s => s.FilePath == song.FilePath))
                {
                    _musicLibraryService.AddSong(song);
                }
            }
        }
        catch
        {
        }
    }
}
