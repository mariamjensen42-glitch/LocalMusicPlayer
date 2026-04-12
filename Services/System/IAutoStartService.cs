using System.Threading.Tasks;

namespace LocalMusicPlayer.Services;

[System.Runtime.Versioning.SupportedOSPlatform("windows")]
public interface IAutoStartService
{
    Task SetAutoStartAsync(bool enabled);
    bool IsAutoStartEnabled();
}
