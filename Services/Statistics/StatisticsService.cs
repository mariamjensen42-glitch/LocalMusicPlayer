using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LocalMusicPlayer.Data;
using LocalMusicPlayer.Models;
using Microsoft.EntityFrameworkCore;

namespace LocalMusicPlayer.Services;

public class StatisticsService : IStatisticsService
{
    private readonly IMusicPlayerService _musicPlayerService;
    private readonly IMusicLibraryService _musicLibraryService;
    private readonly IConfigurationService _configService;
    private readonly AppDbContext _db;

    private Song? _currentSong;
    private TimeSpan _playStartTime;
    private TimeSpan _accumulatedDuration;
    private TimeSpan _songDuration;
    private bool _isPlaying;

    private const string MetaKey = "global";
    private const double PlayThresholdPercentage = 0.3;

    public int TotalPlayCount => _db.SongStatistics.Sum(s => s.PlayCount);
    public TimeSpan TotalPlayTime => TimeSpan.FromMilliseconds(GetMeta()?.TotalPlayTimeMs ?? 0);
    public int UsageDays
    {
        get
        {
            var first = GetMeta()?.FirstScanDate;
            return first != null ? (int)(DateTime.Now - first.Value).TotalDays + 1 : 1;
        }
    }

    public event EventHandler? StatisticsChanged;

    public StatisticsService(
        IMusicPlayerService musicPlayerService,
        IMusicLibraryService musicLibraryService,
        IConfigurationService configService,
        AppDbContext db)
    {
        _musicPlayerService = musicPlayerService;
        _musicLibraryService = musicLibraryService;
        _configService = configService;
        _db = db;

        _musicPlayerService.PlaybackStateChanged += OnPlaybackStateChanged;
        _musicPlayerService.PositionChanged += OnPositionChanged;
        _musicPlayerService.PlaybackEnded += OnPlaybackEnded;

        _ = InitializeAsync();
    }

    public void Initialize()
    {
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        await MigrateFromJsonIfNeededAsync();
        await SyncStatisticsToSongsAsync();
    }

    private async Task MigrateFromJsonIfNeededAsync()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(appDataPath, "LocalMusicPlayer");
        var statisticsFilePath = Path.Combine(appFolder, "statistics.json");

        if (!File.Exists(statisticsFilePath))
            return;

        // Check if already migrated
        if (await _db.SongStatistics.AnyAsync())
            return;

        try
        {
            var json = await File.ReadAllTextAsync(statisticsFilePath);
            var data = JsonSerializer.Deserialize<StatisticsData>(json);
            if (data == null) return;

            if (data.SongStatistics != null)
            {
                foreach (var kvp in data.SongStatistics)
                {
                    _db.SongStatistics.Add(new SongStatisticsEntity
                    {
                        FilePath = kvp.Key,
                        PlayCount = kvp.Value.PlayCount,
                        LastPlayedTime = kvp.Value.LastPlayedTime,
                        TotalPlayedDurationMs = kvp.Value.TotalPlayedDurationMs
                    });
                }
            }

            if (data.ListeningHistory != null)
            {
                foreach (var entry in data.ListeningHistory)
                {
                    _db.ListeningHistory.Add(new ListeningHistoryRecordEntity
                    {
                        FilePath = entry.FilePath,
                        PlayedAt = entry.PlayedAt,
                        PlayedDurationMs = entry.PlayedDurationMs,
                        CompletionPercentage = entry.CompletionPercentage
                    });
                }
            }

            _db.StatisticsMeta.Add(new StatisticsMetaEntity
            {
                Key = MetaKey,
                TotalPlayTimeMs = data.TotalPlayTimeMs,
                FirstScanDate = data.FirstScanDate
            });

            await _db.SaveChangesAsync();
        }
        catch
        {
            // If migration fails, start fresh
        }
    }

    private StatisticsMetaEntity? GetMeta()
    {
        return _db.StatisticsMeta.Local.FirstOrDefault(m => m.Key == MetaKey)
            ?? _db.StatisticsMeta.FirstOrDefault(m => m.Key == MetaKey);
    }

    public LibraryStatistics GetLibraryStatistics()
    {
        var songs = _musicLibraryService.Songs;
        var totalDuration = songs.Aggregate(TimeSpan.Zero, (acc, s) => acc + s.Duration);

        return new LibraryStatistics
        {
            TotalSongs = songs.Count,
            TotalAlbums = songs.Select(s => s.Album).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
            TotalArtists = songs.Select(s => s.Artist).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
            TotalDuration = totalDuration
        };
    }

    public void RecordPlayStart(Song song)
    {
        _currentSong = song;
        _playStartTime = _musicPlayerService.Position;
        _accumulatedDuration = TimeSpan.Zero;
        _songDuration = song.Duration;
        _isPlaying = true;
    }

    public void RecordPlayEnd()
    {
        if (_currentSong == null || !_isPlaying)
            return;

        var playPercentage = _songDuration.TotalSeconds > 0
            ? _accumulatedDuration.TotalSeconds / _songDuration.TotalSeconds
            : 0;

        if (playPercentage >= PlayThresholdPercentage)
        {
            var filePath = _currentSong.FilePath;

            var stats = _db.SongStatistics.FirstOrDefault(s => s.FilePath == filePath);
            if (stats == null)
            {
                stats = new SongStatisticsEntity { FilePath = filePath };
                _db.SongStatistics.Add(stats);
            }

            stats.PlayCount++;
            stats.LastPlayedTime = DateTime.Now;
            stats.TotalPlayedDurationMs += (long)_accumulatedDuration.TotalMilliseconds;

            _currentSong.PlayCount = stats.PlayCount;
            _currentSong.LastPlayedTime = stats.LastPlayedTime;

            _db.ListeningHistory.Add(new ListeningHistoryRecordEntity
            {
                FilePath = filePath,
                PlayedAt = DateTime.Now,
                PlayedDurationMs = (long)_accumulatedDuration.TotalMilliseconds,
                CompletionPercentage = playPercentage
            });

            var meta = GetMeta();
            if (meta == null)
            {
                meta = new StatisticsMetaEntity { Key = MetaKey };
                _db.StatisticsMeta.Add(meta);
            }
            meta.TotalPlayTimeMs += (long)_accumulatedDuration.TotalMilliseconds;
            if (meta.FirstScanDate == null && _musicLibraryService.Songs.Count > 0)
                meta.FirstScanDate = DateTime.Now;

            _db.SaveChanges();
            StatisticsChanged?.Invoke(this, EventArgs.Empty);
        }

        _isPlaying = false;
        _currentSong = null;
    }

    public IReadOnlyList<SongPlayRecord> GetTopPlayedSongs(int count)
    {
        var result = new List<SongPlayRecord>();
        var topSongs = _db.SongStatistics
            .Where(s => s.PlayCount > 0)
            .OrderByDescending(s => s.PlayCount)
            .ThenByDescending(s => s.LastPlayedTime)
            .Take(count)
            .ToList();

        for (int i = 0; i < topSongs.Count; i++)
        {
            var s = topSongs[i];
            var song = _musicLibraryService.Songs.FirstOrDefault(libSong => libSong.FilePath == s.FilePath);
            result.Add(new SongPlayRecord
            {
                Rank = i + 1,
                Song = song ?? new Song { Title = "Unknown", FilePath = s.FilePath },
                PlayCount = s.PlayCount,
                LastPlayedTime = s.LastPlayedTime
            });
        }

        return result;
    }

    public IReadOnlyList<SongPlayRecord> GetRecentlyPlayedSongs(int count)
    {
        var result = new List<SongPlayRecord>();
        var recentSongs = _db.SongStatistics
            .Where(s => s.LastPlayedTime != null)
            .OrderByDescending(s => s.LastPlayedTime)
            .Take(count)
            .ToList();

        for (int i = 0; i < recentSongs.Count; i++)
        {
            var s = recentSongs[i];
            var song = _musicLibraryService.Songs.FirstOrDefault(libSong => libSong.FilePath == s.FilePath);
            result.Add(new SongPlayRecord
            {
                Rank = i + 1,
                Song = song ?? new Song { Title = "Unknown", FilePath = s.FilePath },
                PlayCount = s.PlayCount,
                LastPlayedTime = s.LastPlayedTime
            });
        }

        return result;
    }

    public Task SaveStatisticsAsync()
    {
        return Task.CompletedTask;
    }

    public async Task LoadStatisticsAsync()
    {
        await InitializeAsync();
    }

    public async Task TrackPlayAsync(Song song, TimeSpan playedDuration)
    {
        if (song == null)
            return;

        var playPercentage = song.Duration.TotalSeconds > 0
            ? playedDuration.TotalSeconds / song.Duration.TotalSeconds
            : 0;

        var filePath = song.FilePath;

        var stats = _db.SongStatistics.FirstOrDefault(s => s.FilePath == filePath);
        if (stats == null)
        {
            stats = new SongStatisticsEntity { FilePath = filePath };
            _db.SongStatistics.Add(stats);
        }

        stats.PlayCount++;
        stats.LastPlayedTime = DateTime.Now;
        stats.TotalPlayedDurationMs += (long)playedDuration.TotalMilliseconds;

        song.PlayCount = stats.PlayCount;
        song.LastPlayedTime = stats.LastPlayedTime;

        var meta = GetMeta();
        if (meta == null)
        {
            meta = new StatisticsMetaEntity { Key = MetaKey };
            _db.StatisticsMeta.Add(meta);
        }
        meta.TotalPlayTimeMs += (long)playedDuration.TotalMilliseconds;

        _db.ListeningHistory.Add(new ListeningHistoryRecordEntity
        {
            FilePath = filePath,
            PlayedAt = DateTime.Now,
            PlayedDurationMs = (long)playedDuration.TotalMilliseconds,
            CompletionPercentage = playPercentage
        });

        await _db.SaveChangesAsync();
        StatisticsChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task<StatisticsReport> GetStatisticsReportAsync()
    {
        var report = new StatisticsReport
        {
            GeneratedAt = DateTime.Now,
            Overview = new OverviewStatistics
            {
                TotalSongs = _musicLibraryService.Songs.Count,
                TotalPlayCount = TotalPlayCount,
                TotalPlayTime = TotalPlayTime,
                UniqueArtists = _musicLibraryService.Songs.Select(s => s.Artist).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
                UniqueAlbums = _musicLibraryService.Songs.Select(s => s.Album).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
                FirstListenDate = GetMeta()?.FirstScanDate,
                ListeningDays = UsageDays,
                AverageDailyPlayTime = UsageDays > 0 ? TotalPlayTime.TotalMinutes / UsageDays : 0
            },
            TopSongs = GetTopPlayedSongs(10).ToList(),
            TopArtists = (await GetTopArtistsAsync(10)).ToList(),
            TopAlbums = (await GetTopAlbumsAsync(10)).ToList(),
            GenreDistribution = (await GetGenreDistributionAsync()).ToList()
        };

        return report;
    }

    public Task<IReadOnlyList<ListeningHistoryItem>> GetListeningHistoryAsync(DateTime? startDate, DateTime? endDate)
    {
        var query = _db.ListeningHistory.AsEnumerable();

        if (startDate.HasValue)
            query = query.Where(h => h.PlayedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(h => h.PlayedAt <= endDate.Value);

        var result = query
            .OrderByDescending(h => h.PlayedAt)
            .Select(h =>
            {
                var song = _musicLibraryService.Songs.FirstOrDefault(s => s.FilePath == h.FilePath);
                return new ListeningHistoryItem
                {
                    PlayedAt = h.PlayedAt,
                    Song = song ?? new Song { Title = "Unknown", FilePath = h.FilePath },
                    PlayedDuration = TimeSpan.FromMilliseconds(h.PlayedDurationMs),
                    IsCompleted = h.CompletionPercentage >= 0.9,
                    CompletionPercentage = h.CompletionPercentage
                };
            })
            .ToList();

        return Task.FromResult<IReadOnlyList<ListeningHistoryItem>>(result);
    }

    public Task<IReadOnlyList<ArtistStatistics>> GetTopArtistsAsync(int count)
    {
        var artistStats = new Dictionary<string, ArtistStatistics>(StringComparer.OrdinalIgnoreCase);

        var allStats = _db.SongStatistics.ToList();

        foreach (var song in _musicLibraryService.Songs)
        {
            var stats = allStats.FirstOrDefault(s => s.FilePath == song.FilePath);
            if (stats == null || stats.PlayCount == 0)
                continue;

            if (!artistStats.TryGetValue(song.Artist, out var artistStat))
            {
                artistStat = new ArtistStatistics { ArtistName = song.Artist };
                artistStats[song.Artist] = artistStat;
            }

            artistStat.PlayCount += stats.PlayCount;
            artistStat.TotalPlayTime += TimeSpan.FromMilliseconds(stats.TotalPlayedDurationMs);
            artistStat.SongCount++;

            if (stats.LastPlayedTime > artistStat.LastPlayedTime)
                artistStat.LastPlayedTime = stats.LastPlayedTime;
        }

        var result = artistStats.Values
            .OrderByDescending(a => a.PlayCount)
            .ThenByDescending(a => a.TotalPlayTime)
            .Take(count)
            .Select((a, i) => { a.Rank = i + 1; return a; })
            .ToList();

        return Task.FromResult<IReadOnlyList<ArtistStatistics>>(result);
    }

    public Task<IReadOnlyList<AlbumStatistics>> GetTopAlbumsAsync(int count)
    {
        var albumStats = new Dictionary<string, AlbumStatistics>(StringComparer.OrdinalIgnoreCase);
        var allStats = _db.SongStatistics.ToList();

        foreach (var song in _musicLibraryService.Songs)
        {
            var stats = allStats.FirstOrDefault(s => s.FilePath == song.FilePath);
            if (stats == null || stats.PlayCount == 0)
                continue;

            var albumKey = $"{song.Album}|{song.Artist}";

            if (!albumStats.TryGetValue(albumKey, out var albumStat))
            {
                albumStat = new AlbumStatistics
                {
                    AlbumName = song.Album,
                    ArtistName = song.Artist
                };
                albumStats[albumKey] = albumStat;
            }

            albumStat.PlayCount += stats.PlayCount;
            albumStat.TotalPlayTime += TimeSpan.FromMilliseconds(stats.TotalPlayedDurationMs);
            albumStat.SongCount++;

            if (stats.LastPlayedTime > albumStat.LastPlayedTime)
                albumStat.LastPlayedTime = stats.LastPlayedTime;
        }

        var result = albumStats.Values
            .OrderByDescending(a => a.PlayCount)
            .ThenByDescending(a => a.TotalPlayTime)
            .Take(count)
            .Select((a, i) => { a.Rank = i + 1; return a; })
            .ToList();

        return Task.FromResult<IReadOnlyList<AlbumStatistics>>(result);
    }

    public Task<IReadOnlyList<SongPlayRecord>> GetTopSongsAsync(int count)
    {
        return Task.FromResult(GetTopPlayedSongs(count));
    }

    public Task<IReadOnlyList<GenreDistribution>> GetGenreDistributionAsync()
    {
        var genreStats = new Dictionary<string, (int SongCount, int PlayCount, long TotalPlayTimeMs)>();
        var allStats = _db.SongStatistics.ToList();

        foreach (var song in _musicLibraryService.Songs)
        {
            var genre = string.IsNullOrEmpty(song.Genre) ? "Unknown" : song.Genre;

            if (!genreStats.TryGetValue(genre, out var stat))
                stat = (0, 0, 0);

            stat.SongCount++;

            var songStat = allStats.FirstOrDefault(s => s.FilePath == song.FilePath);
            if (songStat != null)
            {
                stat.PlayCount += songStat.PlayCount;
                stat.TotalPlayTimeMs += songStat.TotalPlayedDurationMs;
            }

            genreStats[genre] = stat;
        }

        var totalSongs = _musicLibraryService.Songs.Count;

        var result = genreStats
            .Select(g => new GenreDistribution
            {
                Genre = g.Key,
                SongCount = g.Value.SongCount,
                PlayCount = g.Value.PlayCount,
                TotalPlayTime = TimeSpan.FromMilliseconds(g.Value.TotalPlayTimeMs),
                Percentage = totalSongs > 0 ? (double)g.Value.SongCount / totalSongs * 100 : 0
            })
            .OrderByDescending(g => g.SongCount)
            .ToList();

        return Task.FromResult<IReadOnlyList<GenreDistribution>>(result);
    }

    private async Task SyncStatisticsToSongsAsync()
    {
        var allStats = _db.SongStatistics.ToList();
        foreach (var song in _musicLibraryService.Songs)
        {
            var stats = allStats.FirstOrDefault(s => s.FilePath == song.FilePath);
            if (stats != null)
            {
                song.PlayCount = stats.PlayCount;
                song.LastPlayedTime = stats.LastPlayedTime;
            }
        }
        await Task.CompletedTask;
    }

    private void OnPlaybackStateChanged(object? sender, PlayState state)
    {
        if (state == PlayState.Playing)
        {
            var currentSong = GetCurrentPlayingSong();
            if (currentSong != null && currentSong != _currentSong)
            {
                RecordPlayEnd();
                RecordPlayStart(currentSong);
            }
            _isPlaying = true;
        }
        else if (state == PlayState.Paused || state == PlayState.Stopped)
        {
            if (_isPlaying && _currentSong != null)
            {
                _accumulatedDuration += _musicPlayerService.Position - _playStartTime;
                _playStartTime = _musicPlayerService.Position;
            }
            _isPlaying = false;
        }
    }

    private void OnPositionChanged(object? sender, TimeSpan position)
    {
        if (_isPlaying && _currentSong != null)
        {
            _songDuration = _currentSong.Duration;
        }
    }

    private void OnPlaybackEnded(object? sender, EventArgs e)
    {
        if (_isPlaying && _currentSong != null)
        {
            _accumulatedDuration += _musicPlayerService.Duration - _playStartTime;
        }
        RecordPlayEnd();
    }

    private Song? GetCurrentPlayingSong()
    {
        return _currentSong;
    }
}

public class StatisticsData
{
    public Dictionary<string, SongStatistics> SongStatistics { get; set; } = new();
    public List<ListeningHistoryEntry> ListeningHistory { get; set; } = new();
    public long TotalPlayTimeMs { get; set; }
    public DateTime? FirstScanDate { get; set; }
}

public class ListeningHistoryEntry
{
    public string FilePath { get; set; } = string.Empty;
    public DateTime PlayedAt { get; set; }
    public long PlayedDurationMs { get; set; }
    public double CompletionPercentage { get; set; }
}
