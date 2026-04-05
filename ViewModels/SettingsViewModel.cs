using System;
using System.Linq;
using System.Reactive;
using Avalonia.Platform.Storage;
using ReactiveUI;
using LocalMusicPlayer.Services;

namespace LocalMusicPlayer.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly IWindowProvider _windowProvider;
    private readonly IScanService _scanService;
    private readonly IMusicLibraryService _musicLibraryService;
    private readonly IConfigurationService _configService;

    private string _musicFolderPath = string.Empty;

    public string MusicFolderPath
    {
        get => _musicFolderPath;
        set => this.RaiseAndSetIfChanged(ref _musicFolderPath, value);
    }

    private bool _includeSubfolders = true;

    public bool IncludeSubfolders
    {
        get => _includeSubfolders;
        set => this.RaiseAndSetIfChanged(ref _includeSubfolders, value);
    }

    private bool _downloadAlbumArtwork = true;

    public bool DownloadAlbumArtwork
    {
        get => _downloadAlbumArtwork;
        set => this.RaiseAndSetIfChanged(ref _downloadAlbumArtwork, value);
    }

    private bool _autoDetectMetadata;

    public bool AutoDetectMetadata
    {
        get => _autoDetectMetadata;
        set => this.RaiseAndSetIfChanged(ref _autoDetectMetadata, value);
    }

    private int _songCount;

    public int SongCount
    {
        get => _songCount;
        set => this.RaiseAndSetIfChanged(ref _songCount, value);
    }

    private int _albumCount;

    public int AlbumCount
    {
        get => _albumCount;
        set => this.RaiseAndSetIfChanged(ref _albumCount, value);
    }

    private string _totalSize = "0 GB";

    public string TotalSize
    {
        get => _totalSize;
        set => this.RaiseAndSetIfChanged(ref _totalSize, value);
    }

    private string _lastScanTime = "Never";

    public string LastScanTime
    {
        get => _lastScanTime;
        set => this.RaiseAndSetIfChanged(ref _lastScanTime, value);
    }

    private string _audioQuality = "Standard";

    public string AudioQuality
    {
        get => _audioQuality;
        set => this.RaiseAndSetIfChanged(ref _audioQuality, value);
    }

    private string _themeMode = "Dark";

    public string ThemeMode
    {
        get => _themeMode;
        set => this.RaiseAndSetIfChanged(ref _themeMode, value);
    }

    private bool _isScanning;

    public bool IsScanning
    {
        get => _isScanning;
        set => this.RaiseAndSetIfChanged(ref _isScanning, value);
    }

    public ReactiveCommand<Unit, Unit> BrowseFolderCommand { get; }
    public ReactiveCommand<Unit, Unit> ScanNowCommand { get; }
    public ReactiveCommand<string, Unit> SelectAudioQualityCommand { get; }
    public ReactiveCommand<string, Unit> SelectThemeCommand { get; }

    public SettingsViewModel(
        IWindowProvider windowProvider,
        IScanService scanService,
        IMusicLibraryService musicLibraryService,
        IConfigurationService configService)
    {
        _windowProvider = windowProvider;
        _scanService = scanService;
        _musicLibraryService = musicLibraryService;
        _configService = configService;

        // 从配置加载初始值
        var settings = _configService.CurrentSettings;
        MusicFolderPath = settings.ScanFolders.FirstOrDefault() ?? string.Empty;
        IncludeSubfolders = settings.IncludeSubfolders;
        ThemeMode = settings.Theme;
        if (settings.LastScanTime.HasValue)
        {
            LastScanTime = settings.LastScanTime.Value.ToString("yyyy-MM-dd HH:mm");
        }

        BrowseFolderCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var window = _windowProvider.CurrentWindow;
            if (window == null) return;

            var folders = await window.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select Music Folder",
                AllowMultiple = false
            });

            if (folders.Count > 0)
            {
                MusicFolderPath = folders[0].Path.LocalPath;
            }
        });

        ScanNowCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            if (string.IsNullOrWhiteSpace(MusicFolderPath)) return;

            IsScanning = true;

            await _scanService.ScanAsync(MusicFolderPath, IncludeSubfolders);

            IsScanning = false;
            LastScanTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            SongCount = _musicLibraryService.Songs.Count;
            AlbumCount = _musicLibraryService.Songs.Select(s => s.Album).Distinct().Count();

            // 保存配置
            await _configService.AddScanFolderAsync(MusicFolderPath);
            _configService.CurrentSettings.IncludeSubfolders = IncludeSubfolders;
            _configService.CurrentSettings.LastScanTime = DateTime.Now;
            await _configService.SaveSettingsAsync();
        });

        SelectAudioQualityCommand = ReactiveCommand.Create<string>(quality => { AudioQuality = quality; });

        SelectThemeCommand = ReactiveCommand.Create<string>(async theme =>
        {
            ThemeMode = theme;
            _configService.CurrentSettings.Theme = theme;
            await _configService.SaveSettingsAsync();
        });
    }
}
