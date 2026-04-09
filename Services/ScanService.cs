using System;
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
    private readonly IFileWatcherService _fileWatcherService;

    public bool IsWatching => _fileWatcherService.IsWatching;

    public ScanService(
        IFileScannerService fileScannerService,
        IMusicLibraryService musicLibraryService,
        IConfigurationService configService,
        IFileWatcherService fileWatcherService)
    {
        _fileScannerService = fileScannerService;
        _musicLibraryService = musicLibraryService;
        _configService = configService;
        _fileWatcherService = fileWatcherService;

        _fileWatcherService.FileCreated += OnFileCreated;
        _fileWatcherService.FileDeleted += OnFileDeleted;
        _fileWatcherService.FileRenamed += OnFileRenamed;
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

        var existingPaths = _musicLibraryService.Songs
            .Where(s => File.Exists(s.FilePath))
            .Select(s => s.FilePath)
            .ToHashSet();

        var songsToRemove = _musicLibraryService.Songs
            .Where(s => !File.Exists(s.FilePath))
            .ToList();

        foreach (var song in songsToRemove)
        {
            _musicLibraryService.Songs.Remove(song);
        }

        var allNewSongs = new List<Song>();

        foreach (var folder in folders)
        {
            if (Directory.Exists(folder))
            {
                var songs = await _fileScannerService.ScanDirectoryAsync(folder, includeSubfolders);
                var newSongs = songs.Where(s => !existingPaths.Contains(s.FilePath));
                allNewSongs.AddRange(newSongs);
            }
        }

        if (allNewSongs.Count > 0)
        {
            _musicLibraryService.AddSongs(allNewSongs);
        }
    }

    public void StartWatching()
    {
        var folders = _configService.GetScanFolders();
        foreach (var folder in folders)
        {
            if (Directory.Exists(folder))
            {
                _fileWatcherService.StartWatching(folder);
            }
        }
    }

    public void StopWatching()
    {
        _fileWatcherService.StopAll();
    }

    private async void OnFileCreated(object? sender, string filePath)
    {
        if (!_fileScannerService.SupportedExtensions.Contains(Path.GetExtension(filePath).ToLowerInvariant()))
            return;

        if (_musicLibraryService.Songs.Any(s => s.FilePath == filePath))
            return;

        try
        {
            var songs = await _fileScannerService.ScanDirectoryAsync(Path.GetDirectoryName(filePath)!, false);
            var song = songs.FirstOrDefault(s => s.FilePath == filePath);
            if (song != null)
            {
                _musicLibraryService.AddSong(song);
            }
        }
        catch
        {
        }
    }

    private void OnFileDeleted(object? sender, string filePath)
    {
        var song = _musicLibraryService.Songs.FirstOrDefault(s => s.FilePath == filePath);
        if (song != null)
        {
            _musicLibraryService.Songs.Remove(song);
        }
    }

    private void OnFileRenamed(object? sender, (string OldPath, string NewPath) paths)
    {
        var song = _musicLibraryService.Songs.FirstOrDefault(s => s.FilePath == paths.OldPath);
        if (song != null)
        {
            song.FilePath = paths.NewPath;
            song.Title = Path.GetFileNameWithoutExtension(paths.NewPath);
        }
    }
}
