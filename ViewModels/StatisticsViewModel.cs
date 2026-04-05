using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using LocalMusicPlayer.Models;
using LocalMusicPlayer.Services;

namespace LocalMusicPlayer.ViewModels;

public class StatisticsViewModel : ViewModelBase
{
    private readonly IStatisticsService _statisticsService;
    private readonly IMusicLibraryService _musicLibraryService;

    private LibraryStatistics _libraryStatistics = new();
    private int _totalPlayCount;
    private TimeSpan _totalPlayTime;
    private int _usageDays;
    private double _averagePlaysPerDay;
    private string _formattedTotalPlayTime = "0h 0m";

    public LibraryStatistics LibraryStatistics
    {
        get => _libraryStatistics;
        set => this.RaiseAndSetIfChanged(ref _libraryStatistics, value);
    }

    public int TotalPlayCount
    {
        get => _totalPlayCount;
        set => this.RaiseAndSetIfChanged(ref _totalPlayCount, value);
    }

    public TimeSpan TotalPlayTime
    {
        get => _totalPlayTime;
        set
        {
            this.RaiseAndSetIfChanged(ref _totalPlayTime, value);
            UpdateFormattedPlayTime();
        }
    }

    public int UsageDays
    {
        get => _usageDays;
        set => this.RaiseAndSetIfChanged(ref _usageDays, value);
    }

    public double AveragePlaysPerDay
    {
        get => _averagePlaysPerDay;
        set => this.RaiseAndSetIfChanged(ref _averagePlaysPerDay, value);
    }

    public string FormattedTotalPlayTime
    {
        get => _formattedTotalPlayTime;
        set => this.RaiseAndSetIfChanged(ref _formattedTotalPlayTime, value);
    }

    public ObservableCollection<SongPlayRecord> TopPlayedSongs { get; } = new();
    public ObservableCollection<SongPlayRecord> RecentlyPlayedSongs { get; } = new();

    private bool _hasPlaybackData;

    public bool HasPlaybackData
    {
        get => _hasPlaybackData;
        set => this.RaiseAndSetIfChanged(ref _hasPlaybackData, value);
    }

    public ReactiveCommand<Unit, Unit> RefreshStatisticsCommand { get; }

    public StatisticsViewModel(
        IStatisticsService statisticsService,
        IMusicLibraryService musicLibraryService)
    {
        _statisticsService = statisticsService;
        _musicLibraryService = musicLibraryService;

        RefreshStatisticsCommand = ReactiveCommand.Create(RefreshStatistics);

        // 订阅统计变更事件
        _statisticsService.StatisticsChanged += (_, _) => RefreshStatistics();

        // 订阅音乐库变更事件
        _musicLibraryService.Songs.CollectionChanged += (_, _) => RefreshStatistics();

        // 初始化加载统计数据
        RefreshStatistics();
    }

    private void RefreshStatistics()
    {
        // 更新音乐库统计
        LibraryStatistics = _statisticsService.GetLibraryStatistics();

        // 更新播放统计
        TotalPlayCount = _statisticsService.TotalPlayCount;
        TotalPlayTime = _statisticsService.TotalPlayTime;
        UsageDays = _statisticsService.UsageDays;
        AveragePlaysPerDay = UsageDays > 0 ? TotalPlayCount / (double)UsageDays : 0;

        // 更新排行榜
        TopPlayedSongs.Clear();
        var topSongs = _statisticsService.GetTopPlayedSongs(10);
        foreach (var record in topSongs)
        {
            TopPlayedSongs.Add(record);
        }

        // 更新最近播放
        RecentlyPlayedSongs.Clear();
        var recentSongs = _statisticsService.GetRecentlyPlayedSongs(10);
        foreach (var record in recentSongs)
        {
            RecentlyPlayedSongs.Add(record);
        }

        // 更新是否有播放数据
        HasPlaybackData = TopPlayedSongs.Count > 0;
    }

    private void UpdateFormattedPlayTime()
    {
        var hours = (int)_totalPlayTime.TotalHours;
        var minutes = _totalPlayTime.Minutes;
        FormattedTotalPlayTime = hours > 0 ? $"{hours}h {minutes}m" : $"{minutes}m";
    }
}