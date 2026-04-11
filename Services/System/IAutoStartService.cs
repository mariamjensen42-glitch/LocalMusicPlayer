using System.Threading.Tasks;

namespace LocalMusicPlayer.Services;

public interface IAutoStartService
{
    Task SetAutoStartAsync(bool enabled);
    bool IsAutoStartEnabled();
}
