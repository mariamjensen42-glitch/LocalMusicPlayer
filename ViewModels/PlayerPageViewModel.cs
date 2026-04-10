using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Media;
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
    private readonly IAlbumArtService _albumArtService;
    private readonly IConfigurationService _configService;
    private readonly DispatcherTimer _timer;

    public Action? OnNavigateBack { get; set; }
    public Action<int>? OnScrollToLyric { get; set; }

    [ObservableProperty] private ObservableCollection<LyricLine> _lyrics = new();

    [ObservableProperty] private int _currentLyricIndex = -1;

    [ObservableProperty] private bool _hasLyrics;

    [ObservableProperty] private float _playbackRate = 1.0f;

    public static float[] AvailablePlaybackRates => [0.5f, 0.75f, 1.0f, 1.25f, 1.5f, 1.75f, 2.0f];

    [ObservableProperty] private IBrush _gradientBackground = new SolidColorBrush(Color.FromRgb(30, 30, 46));

    [ObservableProperty] private double _lyricFontSize = 28;

    [ObservableProperty] private double _lyricLineSpacing = 20;

    [ObservableProperty] private bool _showTranslation = true;

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
    public bool IsRepeat => _playbackStateService.PlaybackMode == PlaybackMode.Loop || _playbackStateService.PlaybackMode == PlaybackMode.SingleLoop;
    public bool IsSingleLoop => _playbackStateService.PlaybackMode == PlaybackMode.SingleLoop;

    public PlayerPageViewModel(
        IPlaybackStateService playbackStateService,
        INavigationService navigationService,
        ILyricsService lyricsService,
        IMusicLibraryService musicLibraryService,
        IAlbumArtService albumArtService,
        IConfigurationService configService)
    {
        _playbackStateService = playbackStateService;
        _navigationService = navigationService;
        _lyricsService = lyricsService;
        _musicLibraryService = musicLibraryService;
        _albumArtService = albumArtService;
        _configService = configService;

        LoadLyricSettings();

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(250)
        };
        _timer.Tick += Timer_Tick;

        _playbackStateService.CurrentSongChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(CurrentSong));
            LoadLyrics();
            UpdateGradientBackground();
        };

        _playbackStateService.PlaybackStateChanged += (_, _) => { OnPropertyChanged(nameof(IsPlaying)); };

        _playbackStateService.PlaybackModeChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(IsShuffle));
            OnPropertyChanged(nameof(IsRepeat));
            OnPropertyChanged(nameof(IsSingleLoop));
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
        _playbackStateService.PlaybackMode = _playbackStateService.PlaybackMode switch
        {
            PlaybackMode.Normal => PlaybackMode.Loop,
            PlaybackMode.Loop => PlaybackMode.SingleLoop,
            _ => PlaybackMode.Normal
        };
        OnPropertyChanged(nameof(IsShuffle));
        OnPropertyChanged(nameof(IsRepeat));
        OnPropertyChanged(nameof(IsSingleLoop));
    }

    partial void OnPlaybackRateChanged(float value)
    {
        _playbackStateService.SetPlaybackRate(value);
    }

    [RelayCommand]
    private void ChangePlaybackRate(float rate)
    {
        PlaybackRate = rate;
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

    private void LoadLyricSettings()
    {
        var settings = _configService.CurrentSettings;
        LyricFontSize = settings.LyricFontSize;
        LyricLineSpacing = settings.LyricLineSpacing;
        ShowTranslation = settings.ShowTranslation;
    }

    partial void OnLyricFontSizeChanged(double value)
    {
        _configService.CurrentSettings.LyricFontSize = value;
        _ = _configService.SaveSettingsAsync();
    }

    partial void OnLyricLineSpacingChanged(double value)
    {
        _configService.CurrentSettings.LyricLineSpacing = value;
        _ = _configService.SaveSettingsAsync();
    }

    partial void OnShowTranslationChanged(bool value)
    {
        _configService.CurrentSettings.ShowTranslation = value;
        _ = _configService.SaveSettingsAsync();
    }

    [RelayCommand]
    private void IncreaseLyricFontSize()
    {
        LyricFontSize = Math.Min(48, LyricFontSize + 2);
    }

    [RelayCommand]
    private void DecreaseLyricFontSize()
    {
        LyricFontSize = Math.Max(14, LyricFontSize - 2);
    }

    [RelayCommand]
    private void ToggleTranslation()
    {
        ShowTranslation = !ShowTranslation;
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

    private async void UpdateGradientBackground()
    {
        if (CurrentSong == null)
        {
            GradientBackground = new SolidColorBrush(Color.FromRgb(30, 30, 46));
            return;
        }

        var dominantColor = await _albumArtService.ExtractDominantColorAsync(CurrentSong.AlbumArtPath);

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (dominantColor.HasValue)
            {
                var color = dominantColor.Value;
                var darkColor = Color.FromRgb(
                    (byte)(color.R * 0.15),
                    (byte)(color.G * 0.15),
                    (byte)(color.B * 0.15));
                var midColor = Color.FromArgb(80, color.R, color.G, color.B);

                GradientBackground = new LinearGradientBrush
                {
                    StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                    EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
                    GradientStops = new GradientStops
                    {
                        new GradientStop(midColor, 0),
                        new GradientStop(darkColor, 0.6),
                        new GradientStop(Color.FromRgb(30, 30, 46), 1)
                    }
                };
            }
            else
            {
                GradientBackground = new SolidColorBrush(Color.FromRgb(30, 30, 46));
            }
        });
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
