using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalMusicPlayer.Models;
using LocalMusicPlayer.Services;

namespace LocalMusicPlayer.ViewModels;

public partial class PlayerPageViewModel : ViewModelBase, IPlaybackProgress
{
    private readonly IPlaybackStateService _playbackStateService;
    private readonly INavigationService _navigationService;
    private readonly ILyricsService _lyricsService;
    private readonly IMusicLibraryService _musicLibraryService;
    private readonly DispatcherTimer _timer;

    public Action? OnNavigateBack { get; set; }
    public Action<int>? OnScrollToLyric { get; set; }

    [ObservableProperty] private ObservableCollection<LyricLine> _lyrics = new();

    [ObservableProperty] private int _currentLyricIndex = -1;

    [ObservableProperty] private bool _hasLyrics;

    public Song? CurrentSong => _playbackStateService.CurrentSong;
    public bool IsPlaying => _playbackStateService.IsPlaying;
    public TimeSpan Position => _playbackStateService.Position;
    public TimeSpan Duration => _playbackStateService.Duration;

    public double PositionSeconds
    {
        get => _playbackStateService.PositionSeconds;
        set => _playbackStateService.PositionSeconds = value;
    }

    public double DurationSeconds => _playbackStateService.DurationSeconds;
    public int Volume => _playbackStateService.Volume;
    public bool IsMuted => _playbackStateService.IsMuted;
    public bool IsShuffle => _playbackStateService.PlaybackMode == PlaybackMode.Shuffle;
    public bool IsRepeat => _playbackStateService.PlaybackMode == PlaybackMode.Loop;

    public PlayerPageViewModel(
        IPlaybackStateService playbackStateService,
        INavigationService navigationService,
        ILyricsService lyricsService,
        IMusicLibraryService musicLibraryService)
    {
        _playbackStateService = playbackStateService;
        _navigationService = navigationService;
        _lyricsService = lyricsService;
        _musicLibraryService = musicLibraryService;

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(250)
        };
        _timer.Tick += Timer_Tick;

        _playbackStateService.CurrentSongChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(CurrentSong));
            LoadLyrics();
        };

        _playbackStateService.PlaybackStateChanged += (_, _) => { OnPropertyChanged(nameof(IsPlaying)); };

        _playbackStateService.PlaybackModeChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(IsShuffle));
            OnPropertyChanged(nameof(IsRepeat));
        };

        _timer.Start();
        LoadLyrics();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(Position));
        OnPropertyChanged(nameof(Duration));
        OnPropertyChanged(nameof(PositionSeconds));
        OnPropertyChanged(nameof(DurationSeconds));
        UpdateCurrentLyricIndex();
    }

    [RelayCommand]
    private void Play()
    {
        _playbackStateService.Resume();
    }

    [RelayCommand]
    private void Pause()
    {
        _playbackStateService.Pause();
    }

    [RelayCommand]
    private void Next()
    {
        _playbackStateService.PlayNext();
    }

    [RelayCommand]
    private void Previous()
    {
        _playbackStateService.PlayPrevious();
    }

    [RelayCommand]
    private void Shuffle()
    {
        _playbackStateService.PlaybackMode = _playbackStateService.PlaybackMode == PlaybackMode.Shuffle
            ? PlaybackMode.Normal
            : PlaybackMode.Shuffle;
        OnPropertyChanged(nameof(IsShuffle));
        OnPropertyChanged(nameof(IsRepeat));
    }

    [RelayCommand]
    private void Repeat()
    {
        _playbackStateService.PlaybackMode = _playbackStateService.PlaybackMode == PlaybackMode.Loop
            ? PlaybackMode.Normal
            : PlaybackMode.Loop;
        OnPropertyChanged(nameof(IsShuffle));
        OnPropertyChanged(nameof(IsRepeat));
    }

    [RelayCommand]
    private void ToggleFavorite()
    {
        if (CurrentSong != null)
        {
            CurrentSong.IsFavorite = !CurrentSong.IsFavorite;
        }
    }

    [RelayCommand]
    private void NavigateBack()
    {
        OnNavigateBack?.Invoke();
    }

    [RelayCommand]
    private void ToggleMute()
    {
        _playbackStateService.Mute();
    }

    [RelayCommand]
    private void ToggleQueuePanel()
    {
        _navigationService.ToggleQueuePanel();
    }

    private void LoadLyrics()
    {
        Lyrics.Clear();
        CurrentLyricIndex = -1;
        HasLyrics = false;

        if (CurrentSong != null && !string.IsNullOrEmpty(CurrentSong.FilePath))
        {
            var lyrics = _lyricsService.GetLyrics(CurrentSong.FilePath);
            foreach (var lyric in lyrics)
            {
                Lyrics.Add(lyric);
            }

            HasLyrics = Lyrics.Count > 0;
        }
    }

    private void UpdateCurrentLyricIndex()
    {
        if (Lyrics.Count > 0)
        {
            var newIndex = _lyricsService.GetCurrentLyricIndex(Lyrics.ToList(), Position);

            if (CurrentLyricIndex >= 0 && CurrentLyricIndex < Lyrics.Count)
            {
                Lyrics[CurrentLyricIndex].IsActive = false;
            }

            if (newIndex >= 0 && newIndex < Lyrics.Count)
            {
                Lyrics[newIndex].IsActive = true;
            }

            CurrentLyricIndex = newIndex;
            OnScrollToLyric?.Invoke(newIndex);
        }
    }
}
