using System.Collections.Generic;
using System.Threading.Tasks;
using LocalMusicPlayer.Models;

namespace LocalMusicPlayer.Services;

public interface IConfigurationService
{
    AppSettings CurrentSettings { get; }
    Task LoadSettingsAsync();
    Task SaveSettingsAsync();
    List<string> GetScanFolders();
    Task AddScanFolderAsync(string path);
    Task RemoveScanFolderAsync(string path);
    Task SavePlaybackStateAsync(string? lastSongFilePath, List<string> queueFilePaths, double lastPlaybackPosition);
}