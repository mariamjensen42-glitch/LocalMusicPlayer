using Avalonia.Controls;

namespace LocalMusicPlayer.Services;

public interface IWindowProvider
{
    Window? CurrentWindow { get; set; }
}
