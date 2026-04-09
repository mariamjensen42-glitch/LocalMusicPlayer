using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using Avalonia.Threading;

namespace LocalMusicPlayer.Services;

public class FileWatcherService : IFileWatcherService
{
    private readonly ConcurrentDictionary<string, FileSystemWatcher> _watchers = new();
    private readonly IFileScannerService _fileScannerService;
    private readonly string[] _supportedExtensions;

    public bool IsWatching => _watchers.Count > 0;

    public event EventHandler<string>? FileCreated;
    public event EventHandler<string>? FileDeleted;
    public event EventHandler<(string OldPath, string NewPath)>? FileRenamed;

    public FileWatcherService(IFileScannerService fileScannerService)
    {
        _fileScannerService = fileScannerService;
        _supportedExtensions = _fileScannerService.SupportedExtensions
            .Select(e => e.ToLowerInvariant())
            .ToArray();
    }

    public void StartWatching(string folderPath)
    {
        if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
            return;

        if (_watchers.ContainsKey(folderPath))
            return;

        var watcher = new FileSystemWatcher(folderPath)
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite,
            IncludeSubdirectories = true,
            EnableRaisingEvents = true
        };

        watcher.Created += OnFileCreated;
        watcher.Deleted += OnFileDeleted;
        watcher.Renamed += OnFileRenamed;
        watcher.Error += OnWatcherError;

        _watchers.TryAdd(folderPath, watcher);
    }

    public void StopWatching(string folderPath)
    {
        if (_watchers.TryRemove(folderPath, out var watcher))
        {
            watcher.EnableRaisingEvents = false;
            watcher.Created -= OnFileCreated;
            watcher.Deleted -= OnFileDeleted;
            watcher.Renamed -= OnFileRenamed;
            watcher.Error -= OnWatcherError;
            watcher.Dispose();
        }
    }

    public void StopAll()
    {
        foreach (var path in _watchers.Keys.ToList())
        {
            StopWatching(path);
        }
    }

    public void Dispose()
    {
        StopAll();
    }

    private bool IsSupportedFile(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return _supportedExtensions.Contains(extension);
    }

    private void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        if (!IsSupportedFile(e.FullPath))
            return;

        Dispatcher.UIThread.Post(() =>
        {
            FileCreated?.Invoke(this, e.FullPath);
        });
    }

    private void OnFileDeleted(object sender, FileSystemEventArgs e)
    {
        if (!IsSupportedFile(e.FullPath))
            return;

        Dispatcher.UIThread.Post(() =>
        {
            FileDeleted?.Invoke(this, e.FullPath);
        });
    }

    private void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            FileRenamed?.Invoke(this, (e.OldFullPath, e.FullPath));
        });
    }

    private void OnWatcherError(object sender, ErrorEventArgs e)
    {
        if (sender is FileSystemWatcher watcher)
        {
            StopWatching(watcher.Path);
        }
    }
}
