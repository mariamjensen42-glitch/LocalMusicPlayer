using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalMusicPlayer.Models;
using LocalMusicPlayer.Services;
using Microsoft.Extensions.Logging;

namespace LocalMusicPlayer.ViewModels;

public partial class MusicBrowseViewModel : ViewModelBase
{
    private readonly IMusicLibraryService _musicLibraryService;
    private readonly IPlaylistService _playlistService;
    private readonly IPlaybackStateService _playbackStateService;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    private readonly ILogger<MusicBrowseViewModel> _logger;

    [ObservableProperty] private string _selectedTab = "song";

    [ObservableProperty] private string _searchText = string.Empty;

    [ObservableProperty] private object? _currentContent;

    public MusicBrowseViewModel(
        IMusicLibraryService musicLibraryService,
        IPlaylistService playlistService,
        IPlaybackStateService playbackStateService,
        INavigationService navigationService,
        IDialogService dialogService,
        ILogger<MusicBrowseViewModel> logger)
    {
        _musicLibraryService = musicLibraryService;
        _playlistService = playlistService;
        _playbackStateService = playbackStateService;
        _navigationService = navigationService;
        _dialogService = dialogService;
        _logger = logger;

        SubscribeEvent(
            () => _playbackStateService.PlaybackStateChanged += OnPlaybackStateChanged,
            () => _playbackStateService.PlaybackStateChanged -= OnPlaybackStateChanged);

        SubscribeEvent(
            () => _playbackStateService.CurrentSongChanged += OnCurrentSongChanged,
            () => _playbackStateService.CurrentSongChanged -= OnCurrentSongChanged);
    }

    partial void OnSelectedTabChanged(string value)
    {
        _ = OnSelectionChangedAsync();
    }

    private async Task OnSelectionChangedAsync()
    {
        _logger.LogInformation("Tab changed to: {SelectedTab}", SelectedTab);
        switch (SelectedTab)
        {
            case "song":
                _navigationService.NavigateTo<SongListViewModel>();
                break;
            case "album":
                _navigationService.NavigateTo<AlbumsPageViewModel>();
                break;
            case "artist":
                _navigationService.NavigateTo<ArtistsPageViewModel>();
                break;
            case "folder":
                _navigationService.NavigateTo<FolderBrowseViewModel>();
                break;
            case "favourite":
                _logger.LogWarning("Favourite page not implemented yet");
                break;
            case "playlist":
                _navigationService.NavigateTo<PlaylistManagementViewModel>();
                break;
            default:
                _logger.LogWarning("Unknown tab: {SelectedTab}", SelectedTab);
                break;
        }
        await Task.CompletedTask;
    }

    [RelayCommand]
    private void Play()
    {
        if (_playbackStateService.IsPlaying)
            _playbackStateService.Pause();
        else
            _playbackStateService.Resume();
        _logger.LogDebug("Play/Pause toggled");
    }

    [RelayCommand]
    private void Pause()
    {
        _playbackStateService.Pause();
        _logger.LogDebug("Pause");
    }

    [RelayCommand]
    private void Next()
    {
        _playbackStateService.PlayNext();
        _logger.LogDebug("Next track");
    }

    [RelayCommand]
    private void Previous()
    {
        _playbackStateService.PlayPrevious();
        _logger.LogDebug("Previous track");
    }

    [RelayCommand]
    private void Stop()
    {
        _playbackStateService.Stop();
        _logger.LogDebug("Stop playback");
    }

    [RelayCommand]
    private void FastForward()
    {
        AdjustPlaybackPosition(5);
    }

    [RelayCommand]
    private void FastBackward()
    {
        AdjustPlaybackPosition(-5);
    }

    private void AdjustPlaybackPosition(int seconds)
    {
        var newPosition = _playbackStateService.Position + TimeSpan.FromSeconds(seconds);
        if (newPosition < TimeSpan.Zero)
            newPosition = TimeSpan.Zero;
        if (newPosition > _playbackStateService.Duration && _playbackStateService.Duration > TimeSpan.Zero)
            newPosition = _playbackStateService.Duration;
        _playbackStateService.Seek(newPosition);
        _logger.LogDebug("Adjust position by {Seconds}s", seconds);
    }

    [RelayCommand]
    private void VolumeUp()
    {
        var newVolume = Math.Min(100, _playbackStateService.Volume + 5);
        _playbackStateService.SetVolume(newVolume);
        _logger.LogDebug("Volume up to {Volume}", newVolume);
    }

    [RelayCommand]
    private void VolumeDown()
    {
        var newVolume = Math.Max(0, _playbackStateService.Volume - 5);
        _playbackStateService.SetVolume(newVolume);
        _logger.LogDebug("Volume down to {Volume}", newVolume);
    }

    [RelayCommand]
    private void MuteToggle()
    {
        if (_playbackStateService.IsMuted)
            _playbackStateService.SetVolume(50);
        else
            _playbackStateService.Mute();
        _logger.LogDebug("Mute toggled");
    }

    [RelayCommand]
    private void PlayModeChanged()
    {
        var currentMode = _playbackStateService.PlaybackMode;
        _playbackStateService.PlaybackMode = currentMode switch
        {
            PlaybackMode.SingleLoop => PlaybackMode.Loop,
            PlaybackMode.Loop => PlaybackMode.Shuffle,
            PlaybackMode.Shuffle => PlaybackMode.Normal,
            _ => PlaybackMode.SingleLoop
        };
        _logger.LogInformation("Play mode changed to {Mode}", _playbackStateService.PlaybackMode);
    }

    private void OnPlaybackStateChanged(object? sender, PlayState state)
    {
        OnPropertyChanged(nameof(IsPlaying));
    }

    private void OnCurrentSongChanged(object? sender, Song? song)
    {
        OnPropertyChanged(nameof(CurrentSong));
    }

    public bool IsPlaying => _playbackStateService.IsPlaying;

    public Song? CurrentSong => _playbackStateService.CurrentSong;

    protected override void DisposeCore()
    {
        base.DisposeCore();
    }
}
