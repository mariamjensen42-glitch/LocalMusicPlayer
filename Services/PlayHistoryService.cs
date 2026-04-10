using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LocalMusicPlayer.Models;

namespace LocalMusicPlayer.Services;

public class PlayHistoryService : IPlayHistoryService
{
    private const int MaxHistoryCount = 200;
    private readonly List<PlayHistoryEntry> _history = new();
    private readonly IConfigurationService _configService;
    private readonly IMusicLibraryService _libraryService;

    public event EventHandler? HistoryChanged;

    public PlayHistoryService(IConfigurationService configService, IMusicLibraryService libraryService)
    {
        _configService = configService;
        _libraryService = libraryService;
        LoadHistory();
    }

    public void AddToHistory(Song song)
    {
        var entry = new PlayHistoryEntry(song, DateTime.Now);

        _history.Insert(0, entry);

        if (_history.Count > MaxHistoryCount)
        {
            _history.RemoveAt(_history.Count - 1);
        }

        HistoryChanged?.Invoke(this, EventArgs.Empty);
        SaveHistoryAsync().ConfigureAwait(false);
    }

    public IReadOnlyList<PlayHistoryEntry> GetHistory()
    {
        return _history.ToList().AsReadOnly();
    }

    public void ClearHistory()
    {
        _history.Clear();
        HistoryChanged?.Invoke(this, EventArgs.Empty);
        SaveHistoryAsync().ConfigureAwait(false);
    }

    private void LoadHistory()
    {
        var records = _configService.CurrentSettings.PlayHistory;
        if (records == null || records.Count == 0)
            return;

        _history.Clear();
        foreach (var record in records.OrderByDescending(r => r.PlayedAt))
        {
            var song = _libraryService.Songs.FirstOrDefault(s => s.FilePath == record.FilePath);
            if (song != null)
            {
                _history.Add(new PlayHistoryEntry(song, record.PlayedAt));
            }
        }

        if (_history.Count > MaxHistoryCount)
        {
            _history.RemoveRange(MaxHistoryCount, _history.Count - MaxHistoryCount);
        }
    }

    private async Task SaveHistoryAsync()
    {
        var records = _history.Select(h => new PlayHistoryRecord
        {
            FilePath = h.Song.FilePath,
            PlayedAt = h.PlayedAt
        }).ToList();

        _configService.CurrentSettings.PlayHistory = records;
        await _configService.SaveSettingsAsync();
    }
}
