using System.Threading.Tasks;
using LocalMusicPlayer.Models;

namespace LocalMusicPlayer.Services;

public class ScanService : IScanService
{
    private readonly IFileScannerService _fileScannerService;
    private readonly IMusicLibraryService _musicLibraryService;

    public ScanService(IFileScannerService fileScannerService, IMusicLibraryService musicLibraryService)
    {
        _fileScannerService = fileScannerService;
        _musicLibraryService = musicLibraryService;
    }

    public async Task ScanAsync(string folderPath, bool includeSubfolders)
    {
        _musicLibraryService.Clear();
        var songs = await _fileScannerService.ScanDirectoryAsync(folderPath, includeSubfolders);
        _musicLibraryService.AddSongs(songs);
    }
}
