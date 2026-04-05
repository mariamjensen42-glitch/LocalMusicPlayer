using System.Collections.Generic;
using System.Threading.Tasks;
using LocalMusicPlayer.Models;

namespace LocalMusicPlayer.Services;

public interface IFileScannerService
{
    Task<List<Song>> ScanDirectoryAsync(string path);
}
