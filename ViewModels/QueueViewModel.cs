using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reactive;
using System.Reactive.Linq;
using LocalMusicPlayer.Models;
using LocalMusicPlayer.Services;
using ReactiveUI;

namespace LocalMusicPlayer.ViewModels;

public class QueueViewModel : ViewModelBase
{
    private readonly IPlaylistService _playlistService;
    private readonly IMusicPlayerService _musicPlayerService;
    private bool _isPanelOpen;
    private int _queueCount;
    private Song? _currentSong;

    public bool IsPanelOpen
    {
        get => _isPanelOpen;
        set => this.RaiseAndSetIfChanged(ref _isPanelOpen, value);
    }

    public int QueueCount
    {
        get => _queueCount;
        set => this.RaiseAndSetIfChanged(ref _queueCount, value);
    }

    public Song? CurrentSong
    {
        get => _currentSong;
        set => this.RaiseAndSetIfChanged(ref _currentSong, value);
    }

    public ObservableCollection<Song> QueueSongs =>
        _playlistService.CurrentPlaylist?.Songs ?? new ObservableCollection<Song>();

    public ReactiveCommand<Unit, Unit> ClosePanelCommand { get; }
    public ReactiveCommand<int, Unit> RemoveSongCommand { get; }
    public ReactiveCommand<Unit, Unit> ClearQueueCommand { get; }
    public ReactiveCommand<Song, Unit> PlaySongCommand { get; }
    public ReactiveCommand<int, Unit> MoveSongCommand { get; }

    public QueueViewModel(IPlaylistService playlistService, IMusicPlayerService musicPlayerService)
    {
        _playlistService = playlistService;
        _musicPlayerService = musicPlayerService;

        // Initialize current song
        CurrentSong = _playlistService.CurrentSong;
        UpdateQueueCount();

        // Commands
        ClosePanelCommand = ReactiveCommand.Create(() => { IsPanelOpen = false; });

        RemoveSongCommand = ReactiveCommand.Create<int>(index =>
        {
            if (_playlistService.CurrentPlaylist != null)
            {
                _playlistService.RemoveSongFromPlaylist(_playlistService.CurrentPlaylist, index);
            }
        });

        ClearQueueCommand = ReactiveCommand.Create(() => { _playlistService.ClearPlaylist(); });

        PlaySongCommand = ReactiveCommand.Create<Song>(song =>
        {
            var index = QueueSongs.IndexOf(song);
            if (index >= 0)
            {
                // Set current index and play
                for (int i = 0; i < index; i++)
                {
                    _playlistService.PlayNext();
                }

                CurrentSong = song;
                _musicPlayerService.Play(song);
            }
        });

        MoveSongCommand = ReactiveCommand.Create<int>(newIndex =>
        {
            // This will be called by DragDropSortBehavior with the new index
            // The old index is tracked in the behavior
        });

        // Subscribe to playlist service events
        _playlistService.CurrentSongChanged += (_, song) => { CurrentSong = song; };

        // Subscribe to collection changes
        if (_playlistService.CurrentPlaylist != null)
        {
            _playlistService.CurrentPlaylist.Songs.CollectionChanged += OnQueueSongsChanged;
        }
    }

    private void OnQueueSongsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateQueueCount();
    }

    private void UpdateQueueCount()
    {
        QueueCount = _playlistService.CurrentPlaylist?.Songs.Count ?? 0;
    }

    public void MoveSongInQueue(int oldIndex, int newIndex)
    {
        _playlistService.MoveSong(oldIndex, newIndex);
    }
}