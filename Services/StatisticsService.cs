using System;
using System.Collections.Generic;
using System.Linq;
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
    private long _totalPlayTimeMs;
    private DateTime? _firstScanDate;

    private const double PlayThresholdPercentage = 0.3; // 播放超过30%才算有效播放

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

        // 订阅播放事件
        _musicPlayerService.PlaybackStateChanged += OnPlaybackStateChanged;
        _musicPlayerService.PositionChanged += OnPositionChanged;
        _musicPlayerService.PlaybackEnded += OnPlaybackEnded;

        // 初始化时加载统计数据
        LoadStatisticsAsync().ConfigureAwait(false);
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

        // 判断播放比例是否达到阈值
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

            // 同步更新 Song 模型
            _currentSong.PlayCount = stats.PlayCount;
            _currentSong.LastPlayedTime = stats.LastPlayedTime;

            // 更新总播放时长
            _totalPlayTimeMs += (long)_accumulatedDuration.TotalMilliseconds;

            // 触发事件
            StatisticsChanged?.Invoke(this, EventArgs.Empty);

            // 保存到配置
            SaveStatisticsAsync().ConfigureAwait(false);
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
        _configService.CurrentSettings.SongStatistics = _songStatistics;
        _configService.CurrentSettings.TotalPlayTimeMs = _totalPlayTimeMs;
        _configService.CurrentSettings.FirstScanDate = _firstScanDate;
        await _configService.SaveSettingsAsync();
    }

    public async Task LoadStatisticsAsync()
    {
        await _configService.LoadSettingsAsync();

        _songStatistics = _configService.CurrentSettings.SongStatistics ?? new Dictionary<string, SongStatistics>();
        _totalPlayTimeMs = _configService.CurrentSettings.TotalPlayTimeMs;
        _firstScanDate = _configService.CurrentSettings.FirstScanDate;

        // 如果是首次加载，记录首次扫描日期
        if (_firstScanDate == null && _musicLibraryService.Songs.Count > 0)
        {
            _firstScanDate = DateTime.Now;
            await SaveStatisticsAsync();
        }

        // 同步统计数据到 Song 模型
        SyncStatisticsToSongs();
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
            // 播放开始，记录当前歌曲
            var currentSong = GetCurrentPlayingSong();
            if (currentSong != null && currentSong != _currentSong)
            {
                // 切换了歌曲，先记录上一首的播放
                RecordPlayEnd();
                RecordPlayStart(currentSong);
            }
            _isPlaying = true;
        }
        else if (state == PlayState.Paused || state == PlayState.Stopped)
        {
            // 暂停或停止时，累计当前播放时长
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
        // 更新累计播放时长（用于实时追踪）
        if (_isPlaying && _currentSong != null)
        {
            _songDuration = _currentSong.Duration;
        }
    }

    private void OnPlaybackEnded(object? sender, EventArgs e)
    {
        // 播放结束，记录播放完成
        if (_isPlaying && _currentSong != null)
        {
            _accumulatedDuration += _musicPlayerService.Duration - _playStartTime;
        }
        RecordPlayEnd();
    }

    private Song? GetCurrentPlayingSong()
    {
        // 从音乐库中查找当前播放的歌曲
        // 这个方法需要外部提供当前歌曲信息，暂时返回 null
        // 实际使用中，MainWindowViewModel 会调用 RecordPlayStart 传入当前歌曲
        return _currentSong;
    }
}