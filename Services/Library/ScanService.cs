using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LocalMusicPlayer.Models;
using Microsoft.Extensions.Logging;

namespace LocalMusicPlayer.Services;

public class ScanService : IScanService
{
    private readonly IFileScannerService _fileScannerService;
    private readonly IMusicLibraryService _musicLibraryService;
    private readonly IConfigurationService _configService;
    private readonly IFileWatcherService _fileWatcherService;
    private readonly ILogger<ScanService>? _logger;

    public bool IsWatching => _fileWatcherService.IsWatching;

    public ScanService(
        IFileScannerService fileScannerService,
        IMusicLibraryService musicLibraryService,
        IConfigurationService configService,
        IFileWatcherService fileWatcherService,
        ILogger<ScanService>? logger = null)
    {
        _fileScannerService = fileScannerService;
        _musicLibraryService = musicLibraryService;
        _configService = configService;
        _fileWatcherService = fileWatcherService;
        _logger = logger;

        _fileWatcherService.FileCreated += OnFileCreated;
        _fileWatcherService.FileDeleted += OnFileDeleted;
        _fileWatcherService.FileRenamed += OnFileRenamed;
    }

    public async Task ScanAsync(string folderPath, bool includeSubfolders)
    {
        _logger?.LogInformation("[Scan] Starting scan of folder: {FolderPath}, IncludeSubfolders: {IncludeSubfolders}",
            folderPath, includeSubfolders);
        _musicLibraryService.Clear();
        var songs = await _fileScannerService.ScanDirectoryAsync(folderPath, includeSubfolders);
        _musicLibraryService.AddSongs(songs);
        _logger?.LogInformation("[Scan] Scan completed. Found {SongCount} songs", songs.Count);
    }

    public async Task ScanAllFoldersAsync()
    {
        _musicLibraryService.Clear();

        var folders = _configService.GetScanFolders();
        var includeSubfolders = _configService.CurrentSettings.IncludeSubfolders;

        _logger?.LogInformation("[Scan] Starting scan of all folders: {FolderCount} folders", folders.Count);

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
        _logger?.LogInformation("[Scan] All folders scan completed. Found {SongCount} songs total", allSongs.Count);
    }

    public async Task RescanLibraryAsync()
    {
        _logger?.LogInformation("[Scan] Starting library rescan");
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

        _logger?.LogInformation(
            "[Scan] Library rescan completed. Removed {RemovedCount} songs, added {AddedCount} new songs",
            songsToRemove.Count, allNewSongs.Count);
    }

    public void StartWatching()
    {
        _logger?.LogInformation("[Scan] Starting file watching");
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
        _logger?.LogInformation("[Scan] Stopping file watching");
        _fileWatcherService.StopAll();
    }

    private async void OnFileCreated(object? sender, string filePath)
    {
        _logger?.LogDebug("[Scan] File created: {FilePath}", filePath);
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
                _logger?.LogInformation("[Scan] New song added to library: {Title}", song.Title);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[Scan] Error processing new file: {FilePath}", filePath);
        }
    }

    private void OnFileDeleted(object? sender, string filePath)
    {
        _logger?.LogDebug("[Scan] File deleted: {FilePath}", filePath);
        var song = _musicLibraryService.Songs.FirstOrDefault(s => s.FilePath == filePath);
        if (song != null)
        {
            _musicLibraryService.Songs.Remove(song);
            _logger?.LogInformation("[Scan] Song removed from library: {Title}", song.Title);
        }
    }

    private void OnFileRenamed(object? sender, (string OldPath, string NewPath) paths)
    {
        _logger?.LogDebug("[Scan] File renamed from {OldPath} to {NewPath}", paths.OldPath, paths.NewPath);
        var song = _musicLibraryService.Songs.FirstOrDefault(s => s.FilePath == paths.OldPath);
        if (song != null)
        {
            song.FilePath = paths.NewPath;
            song.Title = Path.GetFileNameWithoutExtension(paths.NewPath);
            _logger?.LogInformation("[Scan] Song updated after rename: {Title}", song.Title);
        }
    }
}
