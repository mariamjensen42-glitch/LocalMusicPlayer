using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LocalMusicPlayer.Data;
using LocalMusicPlayer.Models;

namespace LocalMusicPlayer.Services;

public class PlayHistoryService : IPlayHistoryService
{
    private const int MaxHistoryCount = 200;
    private readonly IMusicLibraryService _libraryService;

    public event EventHandler? HistoryChanged;

    public PlayHistoryService(IMusicLibraryService libraryService)
    {
        _libraryService = libraryService;
    }

    public void AddToHistory(Song song)
    {
        _ = Task.Run(() =>
        {
            using var db = new AppDbContext();
            var existing = db.PlayHistory.FirstOrDefault(e => e.FilePath == song.FilePath);

            if (existing != null)
            {
                existing.PlayedAt = DateTime.Now;
                db.SaveChanges();
            }
            else
            {
                var entity = new PlayHistoryEntity
                {
                    FilePath = song.FilePath,
                    Title = song.Title,
                    Artist = song.Artist,
                    PlayedAt = DateTime.Now
                };

                db.PlayHistory.Add(entity);
                db.SaveChanges();

                var excess = db.PlayHistory.Count() - MaxHistoryCount;
                if (excess > 0)
                {
                    var oldest = db.PlayHistory
                        .OrderBy(e => e.PlayedAt)
                        .Take(excess)
                        .ToList();

                    db.PlayHistory.RemoveRange(oldest);
                    db.SaveChanges();
                }
            }
        });

        HistoryChanged?.Invoke(this, EventArgs.Empty);
    }

    public IReadOnlyList<PlayHistoryEntry> GetHistory()
    {
        var entities = Task.Run(() =>
        {
            using var db = new AppDbContext();
            return db.PlayHistory
                .OrderByDescending(e => e.PlayedAt)
                .ToList();
        }).GetAwaiter().GetResult();

        var result = new List<PlayHistoryEntry>();
        foreach (var entity in entities)
        {
            var song = _libraryService.Songs.FirstOrDefault(s => s.FilePath == entity.FilePath);
            if (song != null)
            {
                result.Add(new PlayHistoryEntry(song, entity.PlayedAt));
            }
        }

        return result.AsReadOnly();
    }

    public void ClearHistory()
    {
        _ = Task.Run(() =>
        {
            using var db = new AppDbContext();
            db.PlayHistory.RemoveRange(db.PlayHistory.ToList());
            db.SaveChanges();
        });

        HistoryChanged?.Invoke(this, EventArgs.Empty);
    }
}
