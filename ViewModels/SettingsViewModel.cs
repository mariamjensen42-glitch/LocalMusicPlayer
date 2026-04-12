using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalMusicPlayer.Services;

namespace LocalMusicPlayer.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly IDialogService _dialogService;
    private readonly IScanService _scanService;
    private readonly IMusicLibraryService _musicLibraryService;
    private readonly IConfigurationService _configService;
    private readonly IPlaybackStateService _playbackStateService;
    private readonly IAutoStartService _autoStartService;

    public ObservableCollection<string> ScanFolders { get; } = new();

    [ObservableProperty] private bool _includeSubfolders = true;

    [ObservableProperty] private bool _downloadAlbumArtwork = true;

    [ObservableProperty] private bool _autoDetectMetadata;

    [ObservableProperty] private int _songCount;

    [ObservableProperty] private int _albumCount;

    [ObservableProperty] private string _totalSize = "0 GB";

    [ObservableProperty] private string _lastScanTime = "Never";

    [ObservableProperty] private string _audioQuality = "Standard";

    [ObservableProperty] private string _themeMode = "Dark";

    [ObservableProperty] private bool _isScanning;

    [ObservableProperty] private bool _crossfadeEnabled;

    [ObservableProperty] private int _crossfadeDurationSeconds = 3;

    [ObservableProperty] private float _playbackRate = 1.0f;

    [ObservableProperty] private bool _replayGainEnabled = true;

    [ObservableProperty] private bool _minimizeToTray = true;

    [ObservableProperty] private bool _showSongChangeNotification = true;

    [ObservableProperty] private bool _autoStartOnBoot;

    [ObservableProperty] private bool _resumeLastPlayback = true;

    [RelayCommand]
    private async Task AddFolder()
    {
        var paths = await _dialogService.ShowFolderPickerAsync();
        if (paths != null)
        {
            foreach (var path in paths)
            {
                if (!string.IsNullOrEmpty(path) && !ScanFolders.Contains(path))
                {
                    ScanFolders.Add(path);
                    await _configService.AddScanFolderAsync(path);
                }
            }
        }
    }

    [RelayCommand]
    private async Task RemoveFolder(string path)
    {
        ScanFolders.Remove(path);
        await _configService.RemoveScanFolderAsync(path);
    }

    [RelayCommand]
    private async Task ScanAll()
    {
        if (ScanFolders.Count == 0) return;

        IsScanning = true;

        await _scanService.ScanAllFoldersAsync();

        IsScanning = false;
        LastScanTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        SongCount = _musicLibraryService.Songs.Count;
        AlbumCount = _musicLibraryService.Songs.Select(s => s.Album).Distinct().Count();

        _configService.CurrentSettings.IncludeSubfolders = IncludeSubfolders;
        _configService.CurrentSettings.LastScanTime = DateTime.Now;
        await _configService.SaveSettingsAsync();
    }

    [RelayCommand]
    private void SelectAudioQuality(string quality)
    {
        AudioQuality = quality;
    }

    [RelayCommand]
    private async Task SelectTheme(string theme)
    {
        ThemeMode = theme;
        _configService.CurrentSettings.Theme = theme;
        await _configService.SaveSettingsAsync();
    }

    public SettingsViewModel(
        IDialogService dialogService,
        IScanService scanService,
        IMusicLibraryService musicLibraryService,
        IConfigurationService configService,
        IPlaybackStateService playbackStateService,
        IAutoStartService autoStartService)
    {
        _dialogService = dialogService;
        _scanService = scanService;
        _musicLibraryService = musicLibraryService;
        _configService = configService;
        _playbackStateService = playbackStateService;
        _autoStartService = autoStartService;

        LoadSettings();
    }

    private void LoadSettings()
    {
        var settings = _configService.CurrentSettings;

        ScanFolders.Clear();
        foreach (var folder in settings.ScanFolders)
        {
            ScanFolders.Add(folder);
        }

        IncludeSubfolders = settings.IncludeSubfolders;
        AudioQuality = settings.AudioQuality;
        DownloadAlbumArtwork = settings.DownloadAlbumArtwork;
        AutoDetectMetadata = settings.AutoDetectMetadata;
        ThemeMode = settings.Theme;
        CrossfadeEnabled = settings.CrossfadeEnabled;
        CrossfadeDurationSeconds = settings.CrossfadeDurationMs / 1000;
        PlaybackRate = settings.PlaybackRate;
        ReplayGainEnabled = settings.ReplayGainEnabled;
        MinimizeToTray = settings.MinimizeToTray;
        ShowSongChangeNotification = settings.ShowSongChangeNotification;
        AutoStartOnBoot = _autoStartService.IsAutoStartEnabled();
        ResumeLastPlayback = settings.ResumeLastPlayback;
        SongCount = _musicLibraryService.Songs.Count;
        AlbumCount = _musicLibraryService.Songs.Select(s => s.Album).Distinct().Count();

        if (settings.LastScanTime.HasValue)
        {
            LastScanTime = settings.LastScanTime.Value.ToString("yyyy-MM-dd HH:mm");
        }
    }

    private async Task SaveSettingsAsync()
    {
        _configService.CurrentSettings.IncludeSubfolders = IncludeSubfolders;
        await _configService.SaveSettingsAsync();
    }

    partial void OnAudioQualityChanged(string value)
    {
        _configService.CurrentSettings.AudioQuality = value;
        _ = _configService.SaveSettingsAsync().ContinueWith(_ => { }, TaskContinuationOptions.OnlyOnFaulted);
    }

    partial void OnThemeModeChanged(string value)
    {
        _configService.CurrentSettings.Theme = value;
        _ = _configService.SaveSettingsAsync().ContinueWith(_ => { }, TaskContinuationOptions.OnlyOnFaulted);
    }

    partial void OnDownloadAlbumArtworkChanged(bool value)
    {
        _configService.CurrentSettings.DownloadAlbumArtwork = value;
        _ = _configService.SaveSettingsAsync().ContinueWith(_ => { }, TaskContinuationOptions.OnlyOnFaulted);
    }

    partial void OnAutoDetectMetadataChanged(bool value)
    {
        _configService.CurrentSettings.AutoDetectMetadata = value;
        _ = _configService.SaveSettingsAsync().ContinueWith(_ => { }, TaskContinuationOptions.OnlyOnFaulted);
    }

    partial void OnIncludeSubfoldersChanged(bool value)
    {
        _ = SaveSettingsAsync().ContinueWith(_ => { }, TaskContinuationOptions.OnlyOnFaulted);
    }

    partial void OnCrossfadeEnabledChanged(bool value)
    {
        _playbackStateService.IsCrossfadeEnabled = value;
        _configService.CurrentSettings.CrossfadeEnabled = value;
        _ = _configService.SaveSettingsAsync().ContinueWith(_ => { }, TaskContinuationOptions.OnlyOnFaulted);
    }

    partial void OnCrossfadeDurationSecondsChanged(int value)
    {
        _playbackStateService.CrossfadeDuration = TimeSpan.FromSeconds(value);
        _configService.CurrentSettings.CrossfadeDurationMs = value * 1000;
        _ = _configService.SaveSettingsAsync().ContinueWith(_ => { }, TaskContinuationOptions.OnlyOnFaulted);
    }

    partial void OnPlaybackRateChanged(float value)
    {
        _playbackStateService.SetPlaybackRate(value);
        _configService.CurrentSettings.PlaybackRate = value;
        _ = _configService.SaveSettingsAsync().ContinueWith(_ => { }, TaskContinuationOptions.OnlyOnFaulted);
    }

    partial void OnReplayGainEnabledChanged(bool value)
    {
        _playbackStateService.IsReplayGainEnabled = value;
        _configService.CurrentSettings.ReplayGainEnabled = value;
        _ = _configService.SaveSettingsAsync().ContinueWith(_ => { }, TaskContinuationOptions.OnlyOnFaulted);
    }

    partial void OnMinimizeToTrayChanged(bool value)
    {
        _configService.CurrentSettings.MinimizeToTray = value;
        _ = _configService.SaveSettingsAsync().ContinueWith(_ => { }, TaskContinuationOptions.OnlyOnFaulted);
    }

    partial void OnShowSongChangeNotificationChanged(bool value)
    {
        _configService.CurrentSettings.ShowSongChangeNotification = value;
        _ = _configService.SaveSettingsAsync().ContinueWith(_ => { }, TaskContinuationOptions.OnlyOnFaulted);
    }

    partial void OnAutoStartOnBootChanged(bool value)
    {
        _ = HandleAutoStartOnBootChangedAsync(value);
    }

    private async Task HandleAutoStartOnBootChangedAsync(bool value)
    {
        await _autoStartService.SetAutoStartAsync(value);
        _configService.CurrentSettings.AutoStartOnBoot = value;
        await _configService.SaveSettingsAsync();
    }

    partial void OnResumeLastPlaybackChanged(bool value)
    {
        _configService.CurrentSettings.ResumeLastPlayback = value;
        _ = _configService.SaveSettingsAsync().ContinueWith(_ => { }, TaskContinuationOptions.OnlyOnFaulted);
    }
}
