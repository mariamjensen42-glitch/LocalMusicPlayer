using System;
using LocalMusicPlayer.Models;

namespace LocalMusicPlayer.Services;

public interface ISystemTrayService
{
    void Initialize();
    void UpdateTrayIcon(bool isPlaying);
    void ShowNotification(string title, string message);
    void Dispose();
}
