using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LocalMusicPlayer.Models;

namespace LocalMusicPlayer.Services;

public interface IFileScannerService
{
    Task<List<Song>> ScanDirectoryAsync(string path, bool includeSubfolders = true);

    Task<List<Song>> ScanDirectoryAsync(string path, IProgress<int>? progress, CancellationToken cancellationToken);

    IReadOnlyList<string> SupportedExtensions { get; }
}
