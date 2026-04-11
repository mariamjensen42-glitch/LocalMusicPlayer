using System;
using System.Collections.Generic;
using LocalMusicPlayer.Models;

namespace LocalMusicPlayer.Services;

public interface IPlayHistoryService
{
    void AddToHistory(Song song);
    IReadOnlyList<PlayHistoryEntry> GetHistory();
    void ClearHistory();
    event EventHandler? HistoryChanged;
}
