using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalMusicPlayer.Models;
using LocalMusicPlayer.Services;

namespace LocalMusicPlayer.ViewModels;

public partial class QueueViewModel : ViewModelBase
{
    private readonly IPlaylistService _playlistService;
    private readonly IPlaybackStateService _playbackStateService;
    private readonly INavigationService _navigationService;

    public bool IsPanelOpen => _navigationService.IsQueuePanelOpen;

    public int QueueCount => _playlistService.CurrentPlaylist?.Songs.Count ?? 0;

    public Song? CurrentSong => _playbackStateService.CurrentSong;

    public ObservableCollection<Song> QueueSongs =>
        _playlistService.CurrentPlaylist?.Songs ?? new ObservableCollection<Song>();

    [RelayCommand]
    private void ClosePanel()
    {
        _navigationService.ToggleQueuePanel();
    }

    [RelayCommand]
    private void RemoveSong(int index)
    {
        if (_playlistService.CurrentPlaylist != null)
        {
            _playlistService.RemoveSongFromPlaylist(_playlistService.CurrentPlaylist, index);
        }
    }

    [RelayCommand]
    private void ClearQueue()
    {
        _playlistService.ClearPlaylist();
    }

    [RelayCommand]
    private void PlaySong(Song song)
    {
        var index = QueueSongs.IndexOf(song);
        if (index >= 0)
        {
            for (int i = 0; i < index; i++)
            {
                _playlistService.PlayNext();
            }
            _playbackStateService.Play(song);
        }
    }

    public QueueViewModel(
        IPlaylistService playlistService,
        IPlaybackStateService playbackStateService,
        INavigationService navigationService)
    {
        _playlistService = playlistService;
        _playbackStateService = playbackStateService;
        _navigationService = navigationService;

        _playlistService.CurrentSongChanged += (_, _) => OnPropertyChanged(nameof(CurrentSong));

        if (_playlistService.CurrentPlaylist != null)
        {
            _playlistService.CurrentPlaylist.Songs.CollectionChanged += (_, _) =>
            {
                OnPropertyChanged(nameof(QueueCount));
                OnPropertyChanged(nameof(QueueSongs));
            };
        }

        _navigationService.QueuePanelChanged += (_, _) => OnPropertyChanged(nameof(IsPanelOpen));
    }

    public void MoveSongInQueue(int oldIndex, int newIndex)
    {
        _playlistService.MoveSong(oldIndex, newIndex);
    }
}
