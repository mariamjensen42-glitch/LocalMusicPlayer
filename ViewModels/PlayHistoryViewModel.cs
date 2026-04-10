using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalMusicPlayer.Models;
using LocalMusicPlayer.Services;

namespace LocalMusicPlayer.ViewModels;

public partial class PlayHistoryViewModel : ViewModelBase
{
    private readonly IPlayHistoryService _playHistoryService;
    private readonly IPlaybackStateService _playbackStateService;
    private readonly IPlaylistService _playlistService;
    private readonly IStatisticsService _statisticsService;

    [ObservableProperty] private ObservableCollection<PlayHistoryEntry> _historyEntries = new();

    public Action? OnNavigateBack { get; set; }

    public PlayHistoryViewModel(
        IPlayHistoryService playHistoryService,
        IPlaybackStateService playbackStateService,
        IPlaylistService playlistService,
        IStatisticsService statisticsService)
    {
        _playHistoryService = playHistoryService;
        _playbackStateService = playbackStateService;
        _playlistService = playlistService;
        _statisticsService = statisticsService;

        LoadHistory();
        _playHistoryService.HistoryChanged += (_, _) => LoadHistory();
    }

    private void LoadHistory()
    {
        HistoryEntries.Clear();
        foreach (var entry in _playHistoryService.GetHistory())
        {
            HistoryEntries.Add(entry);
        }
    }

    [RelayCommand]
    private void PlaySong(Song song)
    {
        _playlistService.ClearPlaylist();
        _playlistService.AddSongToPlaylist(_playlistService.CurrentPlaylist!, song);
        _playlistService.SetCurrentPlaylist(_playlistService.CurrentPlaylist!);
        _playlistService.PlaySong(song);
        _statisticsService.RecordPlayStart(song);
        _playbackStateService.Play(song);
    }

    [RelayCommand]
    private void ClearHistory()
    {
        _playHistoryService.ClearHistory();
    }

    [RelayCommand]
    private void NavigateBack()
    {
        OnNavigateBack?.Invoke();
    }
}
