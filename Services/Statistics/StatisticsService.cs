using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LocalMusicPlayer.Models;

namespace LocalMusicPlayer.Services;

public class StatisticsService : IStatisticsService
{
    private readonly IMusicPlayerService _musicPlayerService;
    private readonly IMusicLibraryService _musicLibraryService;
    private readonly IConfigurationService _configService;

    private Song? _currentSong;
    private TimeSpan _playStartTime;
    private TimeSpan _accumulatedDuration;
    private TimeSpan _songDuration;
    private bool _isPlaying;

    private Dictionary<string, SongStatistics> _songStatistics = new();
    private List<ListeningHistoryEntry> _listeningHistory = new();
    private long _totalPlayTimeMs;
    private DateTime? _firstScanDate;

    private readonly string _statisticsFilePath;
    private const double PlayThresholdPercentage = 0.3;

    public int TotalPlayCount => _songStatistics.Values.Sum(s => s.PlayCount);
    public TimeSpan TotalPlayTime => TimeSpan.FromMilliseconds(_totalPlayTimeMs);
    public int UsageDays => _firstScanDate != null ? (int)(DateTime.Now - _firstScanDate.Value).TotalDays + 1 : 1;

    public event EventHandler? StatisticsChanged;

    public StatisticsService(
        IMusicPlayerService musicPlayerService,
        IMusicLibraryService musicLibraryService,
        IConfigurationService configService)
    {
        _musicPlayerService = musicPlayerService;
        _musicLibraryService = musicLibraryService;
        _configService = configService;

        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(appDataPath, "LocalMusicPlayer");
        Directory.CreateDirectory(appFolder);
        _statisticsFilePath = Path.Combine(appFolder, "statistics.json");

        _musicPlayerService.PlaybackStateChanged += OnPlaybackStateChanged;
        _musicPlayerService.PositionChanged += OnPositionChanged;
        _musicPlayerService.PlaybackEnded += OnPlaybackEnded;

        _ = LoadStatisticsAsync();
    }

    public void Initialize()
    {
        _ = LoadStatisticsAsync();
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

            if (!_songStatistics.ContainsKey(filePath))
            {
                _songStatistics[filePath] = new SongStatistics { FilePath = filePath };
            }

            var stats = _songStatistics[filePath];
            stats.PlayCount++;
            stats.LastPlayedTime = DateTime.Now;
            stats.TotalPlayedDurationMs += (long)_accumulatedDuration.TotalMilliseconds;

            _currentSong.PlayCount = stats.PlayCount;
            _currentSong.LastPlayedTime = stats.LastPlayedTime;

            _totalPlayTimeMs += (long)_accumulatedDuration.TotalMilliseconds;

            _listeningHistory.Add(new ListeningHistoryEntry
            {
                FilePath = filePath,
                PlayedAt = DateTime.Now,
                PlayedDurationMs = (long)_accumulatedDuration.TotalMilliseconds,
                CompletionPercentage = playPercentage
            });

            StatisticsChanged?.Invoke(this, EventArgs.Empty);
            _ = SaveStatisticsAsync();
        }

        _isPlaying = false;
        _currentSong = null;
    }

    public IReadOnlyList<SongPlayRecord> GetTopPlayedSongs(int count)
    {
        var result = new List<SongPlayRecord>();
        var topSongs = _songStatistics
            .Where(s => s.Value.PlayCount > 0)
            .OrderByDescending(s => s.Value.PlayCount)
            .ThenByDescending(s => s.Value.LastPlayedTime)
            .Take(count)
            .ToList();

        for (int i = 0; i < topSongs.Count; i++)
        {
            var s = topSongs[i];
            var song = _musicLibraryService.Songs.FirstOrDefault(libSong => libSong.FilePath == s.Key);
            result.Add(new SongPlayRecord
            {
                Rank = i + 1,
                Song = song ?? new Song { Title = "Unknown", FilePath = s.Key },
                PlayCount = s.Value.PlayCount,
                LastPlayedTime = s.Value.LastPlayedTime
            });
        }

        return result;
    }

    public IReadOnlyList<SongPlayRecord> GetRecentlyPlayedSongs(int count)
    {
        var result = new List<SongPlayRecord>();
        var recentSongs = _songStatistics
            .Where(s => s.Value.LastPlayedTime != null)
            .OrderByDescending(s => s.Value.LastPlayedTime)
            .Take(count)
            .ToList();

        for (int i = 0; i < recentSongs.Count; i++)
        {
            var s = recentSongs[i];
            var song = _musicLibraryService.Songs.FirstOrDefault(libSong => libSong.FilePath == s.Key);
            result.Add(new SongPlayRecord
            {
                Rank = i + 1,
                Song = song ?? new Song { Title = "Unknown", FilePath = s.Key },
                PlayCount = s.Value.PlayCount,
                LastPlayedTime = s.Value.LastPlayedTime
            });
        }

        return result;
    }

    public async Task SaveStatisticsAsync()
    {
        var data = new StatisticsData
        {
            SongStatistics = _songStatistics,
            ListeningHistory = _listeningHistory,
            TotalPlayTimeMs = _totalPlayTimeMs,
            FirstScanDate = _firstScanDate
        };

        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_statisticsFilePath, json);
    }

    public async Task LoadStatisticsAsync()
    {
        if (File.Exists(_statisticsFilePath))
        {
            var json = await File.ReadAllTextAsync(_statisticsFilePath);
            var data = JsonSerializer.Deserialize<StatisticsData>(json);
            if (data != null)
            {
                _songStatistics = data.SongStatistics ?? new Dictionary<string, SongStatistics>();
                _listeningHistory = data.ListeningHistory ?? new List<ListeningHistoryEntry>();
                _totalPlayTimeMs = data.TotalPlayTimeMs;
                _firstScanDate = data.FirstScanDate;
            }
        }
        else
        {
            await _configService.LoadSettingsAsync();
            _songStatistics = _configService.CurrentSettings.SongStatistics ?? new Dictionary<string, SongStatistics>();
            _totalPlayTimeMs = _configService.CurrentSettings.TotalPlayTimeMs;
            _firstScanDate = _configService.CurrentSettings.FirstScanDate;
        }

        if (_firstScanDate == null && _musicLibraryService.Songs.Count > 0)
        {
            _firstScanDate = DateTime.Now;
            await SaveStatisticsAsync();
        }

        SyncStatisticsToSongs();
    }

    public async Task TrackPlayAsync(Song song, TimeSpan playedDuration)
    {
        if (song == null)
            return;

        var playPercentage = song.Duration.TotalSeconds > 0
            ? playedDuration.TotalSeconds / song.Duration.TotalSeconds
            : 0;

        var filePath = song.FilePath;

        if (!_songStatistics.ContainsKey(filePath))
        {
            _songStatistics[filePath] = new SongStatistics { FilePath = filePath };
        }

        var stats = _songStatistics[filePath];
        stats.PlayCount++;
        stats.LastPlayedTime = DateTime.Now;
        stats.TotalPlayedDurationMs += (long)playedDuration.TotalMilliseconds;

        song.PlayCount = stats.PlayCount;
        song.LastPlayedTime = stats.LastPlayedTime;

        _totalPlayTimeMs += (long)playedDuration.TotalMilliseconds;

        _listeningHistory.Add(new ListeningHistoryEntry
        {
            FilePath = filePath,
            PlayedAt = DateTime.Now,
            PlayedDurationMs = (long)playedDuration.TotalMilliseconds,
            CompletionPercentage = playPercentage
        });

        StatisticsChanged?.Invoke(this, EventArgs.Empty);
        await SaveStatisticsAsync();
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
                FirstListenDate = _firstScanDate,
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
        var query = _listeningHistory.AsEnumerable();

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

        foreach (var song in _musicLibraryService.Songs)
        {
            if (!_songStatistics.TryGetValue(song.FilePath, out var stats) || stats.PlayCount == 0)
                continue;

            if (!artistStats.TryGetValue(song.Artist, out var artistStat))
            {
                artistStat = new ArtistStatistics
                {
                    ArtistName = song.Artist
                };
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

        foreach (var song in _musicLibraryService.Songs)
        {
            if (!_songStatistics.TryGetValue(song.FilePath, out var stats) || stats.PlayCount == 0)
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

        foreach (var song in _musicLibraryService.Songs)
        {
            var genre = string.IsNullOrEmpty(song.Genre) ? "Unknown" : song.Genre;

            if (!genreStats.TryGetValue(genre, out var stat))
            {
                stat = (0, 0, 0);
            }

            stat.SongCount++;

            if (_songStatistics.TryGetValue(song.FilePath, out var songStat))
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

    private void SyncStatisticsToSongs()
    {
        foreach (var song in _musicLibraryService.Songs)
        {
            if (_songStatistics.TryGetValue(song.FilePath, out var stats))
            {
                song.PlayCount = stats.PlayCount;
                song.LastPlayedTime = stats.LastPlayedTime;
            }
        }
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
