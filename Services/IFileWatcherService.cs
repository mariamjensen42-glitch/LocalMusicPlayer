using System;

namespace LocalMusicPlayer.Services;

public interface IFileWatcherService : IDisposable
{
    void StartWatching(string folderPath);

    void StopWatching(string folderPath);

    void StopAll();

    bool IsWatching { get; }

    event EventHandler<string>? FileCreated;

    event EventHandler<string>? FileDeleted;

    event EventHandler<(string OldPath, string NewPath)>? FileRenamed;
}
