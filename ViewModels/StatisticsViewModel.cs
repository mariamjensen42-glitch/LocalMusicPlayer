using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalMusicPlayer.Models;
using LocalMusicPlayer.Services;

namespace LocalMusicPlayer.ViewModels;

public partial class StatisticsViewModel : ViewModelBase
{
    private readonly IStatisticsService _statisticsService;
    private readonly IMusicLibraryService _musicLibraryService;

    [ObservableProperty] private LibraryStatistics _libraryStatistics = new();

    [ObservableProperty] private int _totalPlayCount;

    [ObservableProperty] private TimeSpan _totalPlayTime;

    [ObservableProperty] private int _usageDays;

    [ObservableProperty] private double _averagePlaysPerDay;

    [ObservableProperty] private string _formattedTotalPlayTime = "0h 0m";

    public ObservableCollection<SongPlayRecord> TopPlayedSongs { get; } = new();
    public ObservableCollection<SongPlayRecord> RecentlyPlayedSongs { get; } = new();

    [ObservableProperty] private bool _hasPlaybackData;

    [RelayCommand]
    private void RefreshStatistics()
    {
        LibraryStatistics = _statisticsService.GetLibraryStatistics();
        TotalPlayCount = _statisticsService.TotalPlayCount;
        TotalPlayTime = _statisticsService.TotalPlayTime;
        UsageDays = _statisticsService.UsageDays;
        AveragePlaysPerDay = UsageDays > 0 ? TotalPlayCount / (double)UsageDays : 0;

        TopPlayedSongs.Clear();
        var topSongs = _statisticsService.GetTopPlayedSongs(10);
        foreach (var record in topSongs)
        {
            TopPlayedSongs.Add(record);
        }

        RecentlyPlayedSongs.Clear();
        var recentSongs = _statisticsService.GetRecentlyPlayedSongs(10);
        foreach (var record in recentSongs)
        {
            RecentlyPlayedSongs.Add(record);
        }

        HasPlaybackData = TopPlayedSongs.Count > 0;
    }

    public StatisticsViewModel(
        IStatisticsService statisticsService,
        IMusicLibraryService musicLibraryService)
    {
        _statisticsService = statisticsService;
        _musicLibraryService = musicLibraryService;

        _statisticsService.StatisticsChanged += OnStatisticsChanged;
        _musicLibraryService.Songs.CollectionChanged += OnSongsCollectionChanged;

        RefreshStatistics();
    }

    private void OnStatisticsChanged(object? sender, EventArgs e)
    {
        RefreshStatistics();
    }

    private void OnSongsCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        RefreshStatistics();
    }

    protected override void DisposeCore()
    {
        _statisticsService.StatisticsChanged -= OnStatisticsChanged;
        _musicLibraryService.Songs.CollectionChanged -= OnSongsCollectionChanged;
        base.DisposeCore();
    }

    partial void OnTotalPlayTimeChanged(TimeSpan value)
    {
        var hours = (int)value.TotalHours;
        var minutes = value.Minutes;
        FormattedTotalPlayTime = hours > 0 ? $"{hours}h {minutes}m" : $"{minutes}m";
    }
}
