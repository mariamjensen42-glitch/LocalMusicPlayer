using System;
using LocalMusicPlayer.Models;

namespace LocalMusicPlayer.Services;

public interface IPlayHistoryService
{
    void AddToHistory(Song song);
    void ClearHistory();
    event EventHandler? HistoryChanged;
}
