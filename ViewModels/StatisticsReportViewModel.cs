using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalMusicPlayer.Models;
using LocalMusicPlayer.Services;

namespace LocalMusicPlayer.ViewModels;

public partial class StatisticsReportViewModel : ViewModelBase
{
    private readonly IStatisticsService _statisticsService;

    [ObservableProperty] private OverviewStatistics _overview = new();

    [ObservableProperty] private int _totalSongs;

    [ObservableProperty] private TimeSpan _totalDuration;

    [ObservableProperty] private int _totalPlayCount;

    [ObservableProperty] private string _formattedTotalDuration = "0h 0m";

    [ObservableProperty] private string _formattedTotalPlayTime = "0h 0m";

    [ObservableProperty] private DateTime _startDate = DateTime.Now.AddDays(-30);

    [ObservableProperty] private DateTime _endDate = DateTime.Now;

    [ObservableProperty] private bool _isLoading;

    public ObservableCollection<ArtistStatistics> TopArtists { get; } = new();
    public ObservableCollection<AlbumStatistics> TopAlbums { get; } = new();
    public ObservableCollection<SongPlayRecord> TopSongs { get; } = new();
    public ObservableCollection<GenreDistribution> GenreDistribution { get; } = new();
    public ObservableCollection<ListeningHistoryItem> ListeningHistory { get; } = new();

    public StatisticsReportViewModel(IStatisticsService statisticsService)
    {
        _statisticsService = statisticsService;

        _statisticsService.StatisticsChanged += OnStatisticsChanged;

        _ = LoadDataAsync().ContinueWith(_ => { }, TaskContinuationOptions.OnlyOnFaulted);
    }

    private void OnStatisticsChanged(object? sender, EventArgs e)
    {
        _ = LoadDataAsync().ContinueWith(_ => { }, TaskContinuationOptions.OnlyOnFaulted);
    }

    protected override void DisposeCore()
    {
        _statisticsService.StatisticsChanged -= OnStatisticsChanged;
        base.DisposeCore();
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsLoading = true;

        try
        {
            var report = await _statisticsService.GetStatisticsReportAsync();
            Overview = report.Overview;
            TotalSongs = report.Overview.TotalSongs;
            TotalDuration = report.Overview.TotalPlayTime;
            TotalPlayCount = report.Overview.TotalPlayCount;

            FormattedTotalDuration = FormatDuration(TotalDuration);
            FormattedTotalPlayTime = FormatDuration(report.Overview.TotalPlayTime);

            TopArtists.Clear();
            foreach (var artist in report.TopArtists)
            {
                TopArtists.Add(artist);
            }

            TopAlbums.Clear();
            foreach (var album in report.TopAlbums)
            {
                TopAlbums.Add(album);
            }

            TopSongs.Clear();
            foreach (var song in report.TopSongs)
            {
                TopSongs.Add(song);
            }

            GenreDistribution.Clear();
            foreach (var genre in report.GenreDistribution)
            {
                GenreDistribution.Add(genre);
            }

            await LoadListeningHistoryAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task LoadListeningHistoryAsync()
    {
        var history = await _statisticsService.GetListeningHistoryAsync(StartDate, EndDate);

        ListeningHistory.Clear();
        foreach (var item in history)
        {
            ListeningHistory.Add(item);
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadDataAsync();
    }

    [RelayCommand]
    private void SetTimeRange(string range)
    {
        EndDate = DateTime.Now;

        StartDate = range switch
        {
            "7d" => DateTime.Now.AddDays(-7),
            "30d" => DateTime.Now.AddDays(-30),
            "90d" => DateTime.Now.AddDays(-90),
            "1y" => DateTime.Now.AddYears(-1),
            _ => DateTime.Now.AddDays(-30)
        };

        _ = LoadListeningHistoryAsync().ContinueWith(_ => { }, TaskContinuationOptions.OnlyOnFaulted);
    }

    partial void OnStartDateChanged(DateTime value)
    {
        _ = LoadListeningHistoryAsync().ContinueWith(_ => { }, TaskContinuationOptions.OnlyOnFaulted);
    }

    partial void OnEndDateChanged(DateTime value)
    {
        _ = LoadListeningHistoryAsync().ContinueWith(_ => { }, TaskContinuationOptions.OnlyOnFaulted);
    }

    private static string FormatDuration(TimeSpan duration)
    {
        var hours = (int)duration.TotalHours;
        var minutes = duration.Minutes;
        return hours > 0 ? $"{hours}h {minutes}m" : $"{minutes}m";
    }
}
