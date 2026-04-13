using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalMusicPlayer.Models;
using LocalMusicPlayer.Services;

namespace LocalMusicPlayer.ViewModels;

public partial class SmartPlaylistSongsViewModel : ViewModelBase
{
    private readonly ISmartPlaylistService _smartPlaylistService;
    private readonly IPlaybackStateService _playbackStateService;
    private readonly IPlaylistService _playlistService;
    private readonly IStatisticsService _statisticsService;
    private readonly IPlayHistoryService _playHistoryService;
    private readonly IUserPlaylistService _userPlaylistService;

    public SmartPlaylist SmartPlaylist { get; }
    public string PageTitle => SmartPlaylist.Name;

    public ObservableCollection<Song> Songs { get; } = new();

    public SmartPlaylistSongsViewModel(
        SmartPlaylist smartPlaylist,
        ISmartPlaylistService smartPlaylistService,
        IPlaybackStateService playbackStateService,
        IPlaylistService playlistService,
        IStatisticsService statisticsService,
        IPlayHistoryService playHistoryService,
        IUserPlaylistService userPlaylistService)
    {
        SmartPlaylist = smartPlaylist;
        _smartPlaylistService = smartPlaylistService;
        _playbackStateService = playbackStateService;
        _playlistService = playlistService;
        _statisticsService = statisticsService;
        _playHistoryService = playHistoryService;
        _userPlaylistService = userPlaylistService;
        _ = LoadSongsAsync();
    }

    private async Task LoadSongsAsync()
    {
        var songs = await _smartPlaylistService.GetSongsForSmartPlaylistAsync(SmartPlaylist);
        Songs.Clear();
        foreach (var song in songs)
            Songs.Add(song);
    }

    [RelayCommand]
    private void PlayAll()
    {
        if (Songs.Count == 0) return;
        var playlist = _playlistService.CreatePlaylist(SmartPlaylist.Name);
        _playlistService.ClearPlaylist();
        foreach (var song in Songs)
            _playlistService.AddSongToPlaylist(playlist, song);
        if (_playlistService.PlayNext() is Song song)
        {
            _statisticsService.RecordPlayStart(song);
            _playHistoryService.AddToHistory(song);
            _playbackStateService.Play(song);
        }
    }

    [RelayCommand]
    private void ShuffleAll()
    {
        if (Songs.Count == 0) return;
        var shuffled = new System.Collections.Generic.List<Song>(Songs);
        var rng = new System.Random();
        int n = shuffled.Count;
        while (n > 1) { n--; int k = rng.Next(n + 1); (shuffled[k], shuffled[n]) = (shuffled[n], shuffled[k]); }
        var playlist = _playlistService.CreatePlaylist(SmartPlaylist.Name);
        _playlistService.ClearPlaylist();
        foreach (var song in shuffled)
            _playlistService.AddSongToPlaylist(playlist, song);
        _playbackStateService.PlaybackMode = PlaybackMode.Shuffle;
        if (_playlistService.PlayNext() is Song song)
        {
            _statisticsService.RecordPlayStart(song);
            _playHistoryService.AddToHistory(song);
            _playbackStateService.Play(song);
        }
    }
}
