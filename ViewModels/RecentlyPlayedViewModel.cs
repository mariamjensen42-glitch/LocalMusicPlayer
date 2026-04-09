using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalMusicPlayer.Models;
using LocalMusicPlayer.Services;

namespace LocalMusicPlayer.ViewModels;

public partial class RecentlyPlayedViewModel : ViewModelBase
{
    private readonly IMusicLibraryService _musicLibraryService;
    private readonly IStatisticsService _statisticsService;
    private readonly IPlaylistService _playlistService;
    private readonly IPlaybackStateService _playbackStateService;

    [ObservableProperty]
    private ObservableCollection<SongPlayRecord> _recentlyPlayedSongs = new();

    public delegate void NavigateBackDelegate();
    public NavigateBackDelegate? OnNavigateBack { get; set; }

    public RecentlyPlayedViewModel(
        IMusicLibraryService musicLibraryService,
        IStatisticsService statisticsService,
        IPlaylistService playlistService,
        IPlaybackStateService playbackStateService)
    {
        _musicLibraryService = musicLibraryService;
        _statisticsService = statisticsService;
        _playlistService = playlistService;
        _playbackStateService = playbackStateService;

        LoadRecentlyPlayedSongs();
    }

    private void LoadRecentlyPlayedSongs()
    {
        var recentSongs = _statisticsService.GetRecentlyPlayedSongs(50);
        RecentlyPlayedSongs.Clear();
        foreach (var song in recentSongs)
        {
            RecentlyPlayedSongs.Add(song);
        }
    }

    [RelayCommand]
    private void PlaySong(Song song)
    {
        var playlist = _playlistService.CreatePlaylist("最近播放");
        _playlistService.ClearPlaylist();
        _playlistService.AddSongToPlaylist(playlist, song);
        _playlistService.SetCurrentPlaylist(playlist);
        _playlistService.PlaySong(song);
        _playbackStateService.Play(song);
    }

    [RelayCommand]
    private void NavigateBack()
    {
        OnNavigateBack?.Invoke();
    }
}
