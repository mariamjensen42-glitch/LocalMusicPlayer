using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalMusicPlayer.Models;
using LocalMusicPlayer.Services;

namespace LocalMusicPlayer.ViewModels;

public partial class ArtistDetailViewModel : ViewModelBase
{
    private readonly IMusicPlayerService _musicPlayerService;
    private readonly IPlaylistService _playlistService;
    private readonly IStatisticsService _statisticsService;

    public string ArtistName { get; }
    public string? CoverArtPath { get; }
    public int SongCount => Songs.Count;
    public TimeSpan TotalDuration => Songs.Aggregate(TimeSpan.Zero, (acc, s) => acc + s.Duration);
    public string TotalDurationText => $"{(int)TotalDuration.TotalHours}h {TotalDuration.Minutes}m";

    public ObservableCollection<Song> Songs { get; } = new();

    public Action? OnNavigateBack { get; set; }

    public ArtistDetailViewModel(
        ArtistGroup artistGroup,
        IMusicPlayerService musicPlayerService,
        IPlaylistService playlistService,
        IStatisticsService statisticsService)
    {
        ArtistName = artistGroup.ArtistName;
        CoverArtPath = artistGroup.CoverArtPath;
        _musicPlayerService = musicPlayerService;
        _playlistService = playlistService;
        _statisticsService = statisticsService;

        foreach (var song in artistGroup.Songs)
        {
            Songs.Add(song);
        }
    }

    [RelayCommand]
    private void NavigateBack()
    {
        OnNavigateBack?.Invoke();
    }

    [RelayCommand]
    private void PlaySong(string path)
    {
        var song = Songs.FirstOrDefault(s => s.FilePath == path);
        if (song != null)
        {
            _statisticsService.RecordPlayStart(song);
            _musicPlayerService.Play(song);
        }
    }

    [RelayCommand]
    private void PlayAll()
    {
        if (Songs.Count == 0) return;

        var playlist = _playlistService.CreatePlaylist($"歌手-{ArtistName}");
        _playlistService.SetCurrentPlaylist(playlist);

        foreach (var song in Songs)
        {
            _playlistService.AddSongToPlaylist(playlist, song);
        }

        _playlistService.PlayNext();
        if (_playlistService.CurrentSong != null)
        {
            _statisticsService.RecordPlayStart(_playlistService.CurrentSong);
            _musicPlayerService.Play(_playlistService.CurrentSong);
        }
    }

    [RelayCommand]
    private void ShufflePlay()
    {
        if (Songs.Count == 0) return;

        var playlist = _playlistService.CreatePlaylist($"歌手-{ArtistName}-随机");
        _playlistService.SetCurrentPlaylist(playlist);

        var shuffledSongs = Songs.OrderBy(_ => Random.Shared.Next()).ToList();
        foreach (var song in shuffledSongs)
        {
            _playlistService.AddSongToPlaylist(playlist, song);
        }

        _playlistService.PlayNext();
        if (_playlistService.CurrentSong != null)
        {
            _statisticsService.RecordPlayStart(_playlistService.CurrentSong);
            _musicPlayerService.Play(_playlistService.CurrentSong);
        }
    }
}