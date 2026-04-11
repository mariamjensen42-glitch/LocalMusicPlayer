using Avalonia.Controls;

namespace LocalMusicPlayer.Services;

public class WindowProvider : IWindowProvider
{
    public Window? CurrentWindow { get; set; }
}
