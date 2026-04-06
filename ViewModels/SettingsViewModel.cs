using System;
using System.Collections.ObjectModel;
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

    public ObservableCollection<string> ScanFolders { get; } = new();

    private bool _includeSubfolders = true;

    public bool IncludeSubfolders
    {
        get => _includeSubfolders;
        set
        {
            this.RaiseAndSetIfChanged(ref _includeSubfolders, value);
            SaveSettingsAsync().ConfigureAwait(false);
        }
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

    public ReactiveCommand<Unit, Unit> AddFolderCommand { get; }
    public ReactiveCommand<string, Unit> RemoveFolderCommand { get; }
    public ReactiveCommand<Unit, Unit> ScanAllCommand { get; }
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
        LoadSettings();

        AddFolderCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var window = _windowProvider.CurrentWindow;
            if (window == null) return;

            var folders = await window.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select Music Folder",
                AllowMultiple = true
            });

            foreach (var folder in folders)
            {
                var path = folder.Path.LocalPath;
                if (!ScanFolders.Contains(path))
                {
                    ScanFolders.Add(path);
                    await _configService.AddScanFolderAsync(path);
                }
            }
        });

        RemoveFolderCommand = ReactiveCommand.CreateFromTask(async (string path) =>
        {
            ScanFolders.Remove(path);
            await _configService.RemoveScanFolderAsync(path);
        });

        ScanAllCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            if (ScanFolders.Count == 0) return;

            IsScanning = true;

            await _scanService.ScanAllFoldersAsync();

            IsScanning = false;
            LastScanTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            SongCount = _musicLibraryService.Songs.Count;
            AlbumCount = _musicLibraryService.Songs.Select(s => s.Album).Distinct().Count();

            // 保存配置
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

    private void LoadSettings()
    {
        var settings = _configService.CurrentSettings;

        ScanFolders.Clear();
        foreach (var folder in settings.ScanFolders)
        {
            ScanFolders.Add(folder);
        }

        IncludeSubfolders = settings.IncludeSubfolders;
        ThemeMode = settings.Theme;
        SongCount = _musicLibraryService.Songs.Count;
        AlbumCount = _musicLibraryService.Songs.Select(s => s.Album).Distinct().Count();

        if (settings.LastScanTime.HasValue)
        {
            LastScanTime = settings.LastScanTime.Value.ToString("yyyy-MM-dd HH:mm");
        }
    }

    private async System.Threading.Tasks.Task SaveSettingsAsync()
    {
        _configService.CurrentSettings.IncludeSubfolders = IncludeSubfolders;
        await _configService.SaveSettingsAsync();
    }
}
