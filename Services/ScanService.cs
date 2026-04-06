using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LocalMusicPlayer.Models;

namespace LocalMusicPlayer.Services;

public class ScanService : IScanService
{
    private readonly IFileScannerService _fileScannerService;
    private readonly IMusicLibraryService _musicLibraryService;
    private readonly IConfigurationService _configService;

    public ScanService(
        IFileScannerService fileScannerService,
        IMusicLibraryService musicLibraryService,
        IConfigurationService configService)
    {
        _fileScannerService = fileScannerService;
        _musicLibraryService = musicLibraryService;
        _configService = configService;
    }

    public async Task ScanAsync(string folderPath, bool includeSubfolders)
    {
        _musicLibraryService.Clear();
        var songs = await _fileScannerService.ScanDirectoryAsync(folderPath, includeSubfolders);
        _musicLibraryService.AddSongs(songs);
    }

    public async Task ScanAllFoldersAsync()
    {
        _musicLibraryService.Clear();

        var folders = _configService.GetScanFolders();
        var includeSubfolders = _configService.CurrentSettings.IncludeSubfolders;

        var allSongs = new List<Song>();

        foreach (var folder in folders)
        {
            if (Directory.Exists(folder))
            {
                var songs = await _fileScannerService.ScanDirectoryAsync(folder, includeSubfolders);
                allSongs.AddRange(songs);
            }
        }

        _musicLibraryService.AddSongs(allSongs);
    }

    public async Task RescanLibraryAsync()
    {
        var folders = _configService.GetScanFolders();
        var includeSubfolders = _configService.CurrentSettings.IncludeSubfolders;

        // 检查现有歌曲文件是否存在
        var existingPaths = _musicLibraryService.Songs
            .Where(s => File.Exists(s.FilePath))
            .Select(s => s.FilePath)
            .ToHashSet();

        // 移除不存在的歌曲
        var songsToRemove = _musicLibraryService.Songs
            .Where(s => !File.Exists(s.FilePath))
            .ToList();

        foreach (var song in songsToRemove)
        {
            _musicLibraryService.Songs.Remove(song);
        }

        // 扫描文件夹查找新歌曲
        var allNewSongs = new List<Song>();

        foreach (var folder in folders)
        {
            if (Directory.Exists(folder))
            {
                var songs = await _fileScannerService.ScanDirectoryAsync(folder, includeSubfolders);
                // 只添加新歌曲
                var newSongs = songs.Where(s => !existingPaths.Contains(s.FilePath));
                allNewSongs.AddRange(newSongs);
            }
        }

        // 添加新歌曲到库
        if (allNewSongs.Count > 0)
        {
            _musicLibraryService.AddSongs(allNewSongs);
        }
    }
}
