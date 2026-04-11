using System.Collections.Generic;
using System.Threading.Tasks;

namespace LocalMusicPlayer.Services;

public interface IScanService
{
    Task ScanAsync(string folderPath, bool includeSubfolders);
    Task ScanAllFoldersAsync();
    Task RescanLibraryAsync();
    void StartWatching();
    void StopWatching();
    bool IsWatching { get; }
}
