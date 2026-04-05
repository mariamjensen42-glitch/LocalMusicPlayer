using System.Threading.Tasks;

namespace LocalMusicPlayer.Services;

public interface IScanService
{
    Task ScanAsync(string folderPath, bool includeSubfolders);
}
