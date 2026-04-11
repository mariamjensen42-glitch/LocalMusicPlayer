using System.Collections.Generic;
using System.Threading.Tasks;

namespace LocalMusicPlayer.Services;

public interface IDropHandlerService
{
    Task HandleDroppedFilesAsync(IEnumerable<string> filePaths);
}
