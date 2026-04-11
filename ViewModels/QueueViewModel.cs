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
    private void RemoveSongByReference(Song song)
    {
        var index = QueueSongs.IndexOf(song);
        if (index >= 0 && _playlistService.CurrentPlaylist != null)
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
            _playlistService.PlayAtIndex(index);
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

        _playlistService.CurrentSongChanged += OnCurrentSongChanged;
        _navigationService.QueuePanelChanged += OnQueuePanelChanged;

        if (_playlistService.CurrentPlaylist != null)
        {
            _playlistService.CurrentPlaylist.Songs.CollectionChanged += OnSongsCollectionChanged;
        }
    }

    private void OnCurrentSongChanged(object? sender, Song? song)
    {
        OnPropertyChanged(nameof(CurrentSong));
    }

    private void OnQueuePanelChanged(object? sender, bool isOpen)
    {
        OnPropertyChanged(nameof(IsPanelOpen));
    }

    private void OnSongsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(QueueCount));
        OnPropertyChanged(nameof(QueueSongs));
    }

    protected override void DisposeCore()
    {
        _playlistService.CurrentSongChanged -= OnCurrentSongChanged;
        _navigationService.QueuePanelChanged -= OnQueuePanelChanged;
        if (_playlistService.CurrentPlaylist != null)
        {
            _playlistService.CurrentPlaylist.Songs.CollectionChanged -= OnSongsCollectionChanged;
        }
        base.DisposeCore();
    }

    public void MoveSongInQueue(int oldIndex, int newIndex)
    {
        _playlistService.MoveSong(oldIndex, newIndex);
    }
}
