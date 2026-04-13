using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
    private readonly IOnlineLyricsService _onlineLyricsService;
    private readonly IMusicLibraryService _musicLibraryService;
    private readonly IAlbumArtService _albumArtService;
    private readonly IConfigurationService _configService;
    private readonly IUserPlaylistService _userPlaylistService;
    private readonly IDialogService _dialogService;

    public Action? OnClose { get; set; }

    public Action? OnToggleFullScreen { get; set; }

    [ObservableProperty] private ObservableCollection<LyricLine> _lyrics = new();

    [ObservableProperty] private int _currentLyricIndex = -1;

    [ObservableProperty] private bool _hasLyrics;

    [ObservableProperty] private bool _isSearchingLyrics;

    [ObservableProperty] private string _lyricSearchStatus = string.Empty;

    [ObservableProperty] private float _playbackRate = 1.0f;

    public static float[] AvailablePlaybackRates => [0.5f, 0.75f, 1.0f, 1.25f, 1.5f, 1.75f, 2.0f];

    [ObservableProperty] private string _gradientBackground = "#1E1E2E";

    [ObservableProperty] private byte[]? _imageBytes;

    [ObservableProperty] private bool _isBackgroundEnabled = true;

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

    public bool IsRepeat => _playbackStateService.PlaybackMode == PlaybackMode.Loop ||
                            _playbackStateService.PlaybackMode == PlaybackMode.SingleLoop;

    public bool IsSingleLoop => _playbackStateService.PlaybackMode == PlaybackMode.SingleLoop;

    public PlayerPageViewModel(
        IPlaybackStateService playbackStateService,
        INavigationService navigationService,
        ILyricsService lyricsService,
        IOnlineLyricsService onlineLyricsService,
        IMusicLibraryService musicLibraryService,
        IAlbumArtService albumArtService,
        IConfigurationService configService,
        IUserPlaylistService userPlaylistService,
        IDialogService dialogService)
    {
        _playbackStateService = playbackStateService;
        _navigationService = navigationService;
        _lyricsService = lyricsService;
        _onlineLyricsService = onlineLyricsService;
        _musicLibraryService = musicLibraryService;
        _albumArtService = albumArtService;
        _configService = configService;
        _userPlaylistService = userPlaylistService;
        _dialogService = dialogService;

        LoadLyricSettings();

        _playbackStateService.CurrentSongChanged += OnCurrentSongChanged;
        _playbackStateService.PlaybackStateChanged += OnPlaybackStateChanged;
        _playbackStateService.PlaybackModeChanged += OnPlaybackModeChanged;
        _playbackStateService.PositionChanged += OnPositionChanged;

        LoadLyrics();
    }

    private void OnCurrentSongChanged(object? sender, Song? song)
    {
        OnPropertyChanged(nameof(CurrentSong));
        LoadLyrics();
        _ = UpdateGradientBackgroundAsync();
    }

    private void OnPlaybackStateChanged(object? sender, PlayState state)
    {
        OnPropertyChanged(nameof(IsPlaying));
    }

    private void OnPlaybackModeChanged(object? sender, PlaybackMode mode)
    {
        _configService.CurrentSettings.PlaybackMode = mode.ToString();
        _ = _configService.SaveSettingsAsync().ContinueWith(_ => { }, TaskContinuationOptions.OnlyOnFaulted);
        OnPropertyChanged(nameof(IsShuffle));
        OnPropertyChanged(nameof(IsRepeat));
        OnPropertyChanged(nameof(IsSingleLoop));
    }

    private void OnPositionChanged(object? sender, TimeSpan position)
    {
        OnPropertyChanged(nameof(Position));
        OnPropertyChanged(nameof(Duration));
        OnPropertyChanged(nameof(PositionSeconds));
        OnPropertyChanged(nameof(DurationSeconds));
        UpdateCurrentLyricIndex();
    }

    protected override void DisposeCore()
    {
        _playbackStateService.CurrentSongChanged -= OnCurrentSongChanged;
        _playbackStateService.PlaybackStateChanged -= OnPlaybackStateChanged;
        _playbackStateService.PlaybackModeChanged -= OnPlaybackModeChanged;
        _playbackStateService.PositionChanged -= OnPositionChanged;
        base.DisposeCore();
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
    private async Task ToggleFavoriteAsync()
    {
        if (CurrentSong != null)
        {
            if (CurrentSong.IsFavorite)
                await _userPlaylistService.RemoveFromFavoritesAsync(CurrentSong);
            else
                await _userPlaylistService.AddToFavoritesAsync(CurrentSong);
        }
    }

    [RelayCommand]
    private void NavigateBack()
    {
        OnClose?.Invoke();
    }

    [RelayCommand]
    private void ToggleFullScreen()
    {
        OnToggleFullScreen?.Invoke();
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
        _ = _configService.SaveSettingsAsync().ContinueWith(_ => { }, TaskContinuationOptions.OnlyOnFaulted);
    }

    partial void OnLyricLineSpacingChanged(double value)
    {
        _configService.CurrentSettings.LyricLineSpacing = value;
        _ = _configService.SaveSettingsAsync().ContinueWith(_ => { }, TaskContinuationOptions.OnlyOnFaulted);
    }

    partial void OnShowTranslationChanged(bool value)
    {
        _configService.CurrentSettings.ShowTranslation = value;
        _ = _configService.SaveSettingsAsync().ContinueWith(_ => { }, TaskContinuationOptions.OnlyOnFaulted);

        foreach (var lyric in Lyrics)
        {
            lyric.ShowTranslation = value;
        }
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

    [RelayCommand]
    private async Task SearchOnlineLyricsAsync()
    {
        if (CurrentSong == null || IsSearchingLyrics)
            return;

        IsSearchingLyrics = true;

        try
        {
            var result = await _onlineLyricsService.SearchLyricsAsync(CurrentSong);

            var selectedResult = await _dialogService.ShowLyricSearchResultDialogAsync(CurrentSong, result);

            if (selectedResult != null && selectedResult.Lyrics.Count > 0)
            {
                Lyrics.Clear();
                foreach (var lyric in selectedResult.Lyrics)
                {
                    lyric.ShowTranslation = ShowTranslation;
                    Lyrics.Add(lyric);
                }
                HasLyrics = Lyrics.Count > 0;
                UpdateCurrentLyricIndex();
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowMessageDialogAsync("Error", $"Search failed: {ex.Message}");
        }
        finally
        {
            IsSearchingLyrics = false;
        }
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
                lyric.ShowTranslation = ShowTranslation;
                Lyrics.Add(lyric);
            }

            HasLyrics = Lyrics.Count > 0;
        }
    }

    private async Task UpdateGradientBackgroundAsync()
    {
        if (CurrentSong == null)
        {
            GradientBackground = "#1E1E2E";
            ImageBytes = null;
            return;
        }

        try
        {
            if (!string.IsNullOrEmpty(CurrentSong.AlbumArtPath) && File.Exists(CurrentSong.AlbumArtPath))
            {
                ImageBytes = await File.ReadAllBytesAsync(CurrentSong.AlbumArtPath);
            }
            else
            {
                ImageBytes = null;
            }

            var dominantColor = await _albumArtService.ExtractDominantColorAsync(CurrentSong.AlbumArtPath);

            if (dominantColor.HasValue)
            {
                var color = dominantColor.Value;
                var darkColor = $"#{(byte)(color.R * 0.15):X2}{(byte)(color.G * 0.15):X2}{(byte)(color.B * 0.15):X2}";
                GradientBackground = darkColor;
            }
            else
            {
                GradientBackground = "#1E1E2E";
            }
        }
        catch
        {
            ImageBytes = null;
            GradientBackground = "#1E1E2E";
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
        }
    }
}
