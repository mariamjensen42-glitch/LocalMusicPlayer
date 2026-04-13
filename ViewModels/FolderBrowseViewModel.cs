using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalMusicPlayer.Models;
using LocalMusicPlayer.Services;
using Microsoft.Extensions.Logging;

namespace LocalMusicPlayer.ViewModels;

public partial class FolderBrowseViewModel : ViewModelBase
{
    private readonly IMusicLibraryService _musicLibraryService;
    private readonly IPlaylistService _playlistService;
    private readonly IPlaybackStateService _playbackStateService;
    private readonly INavigationService _navigationService;
    private readonly IUserPlaylistService? _userPlaylistService;
    private readonly ILogger<FolderBrowseViewModel> _logger;

    [ObservableProperty]
    private ObservableCollection<FolderGroup> _folders = [];

    [ObservableProperty]
    private FolderNode? _selectedItem;

    [ObservableProperty]
    private ObservableCollection<MenuOption> _folderMenuOptions = [];

    public FolderBrowseViewModel(
        IMusicLibraryService musicLibraryService,
        IPlaylistService playlistService,
        IPlaybackStateService playbackStateService,
        INavigationService navigationService,
        IUserPlaylistService? userPlaylistService = null,
        ILogger<FolderBrowseViewModel>? logger = null)
    {
        _musicLibraryService = musicLibraryService;
        _playlistService = playlistService;
        _playbackStateService = playbackStateService;
        _navigationService = navigationService;
        _userPlaylistService = userPlaylistService;
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance.CreateLogger<FolderBrowseViewModel>();

        InitializeMenuOptions();
        _ = LoadFoldersAsync().ContinueWith(_ => { }, System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
    }

    private void InitializeMenuOptions()
    {
        FolderMenuOptions.Add(new MenuOption { Title = "Play", Tag = "Play" });
        FolderMenuOptions.Add(new MenuOption { Title = "AddToFavourite", Tag = "AddToFavourite" });
        FolderMenuOptions.Add(new MenuOption
        {
            Title = "AddToPlayList",
            Tag = "AddToPlayList",
            Children = new ObservableCollection<MenuOption>()
        });
        FolderMenuOptions.Add(new MenuOption { Title = "RescanFolder", Tag = "RescanFolder" });

        if (_userPlaylistService != null)
        {
            UpdatePlaylistSubmenu();
            SubscribeEvent(
                () => _userPlaylistService.PlaylistsChanged += OnPlaylistsChanged,
                () => _userPlaylistService.PlaylistsChanged -= OnPlaylistsChanged);
        }
    }

    private void OnPlaylistsChanged(object? sender, System.EventArgs e)
    {
        UpdatePlaylistSubmenu();
    }

    private void UpdatePlaylistSubmenu()
    {
        var playlistOption = FolderMenuOptions.FirstOrDefault(m => (string?)m.Tag == "AddToPlayList");
        if (playlistOption == null) return;

        playlistOption.Children.Clear();
        if (_userPlaylistService == null) return;

        foreach (var playlist in _userPlaylistService.UserPlaylists)
        {
            playlistOption.Children.Add(new MenuOption
            {
                Title = playlist.Name,
                Tag = playlist.Id
            });
        }
    }

    public async Task LoadFoldersAsync()
    {
        var folderGroups = await Task.Run(() =>
        {
            return _musicLibraryService.GetFolderStructure()
                .GroupBy(f => GetGroupKey(f.FullPath))
                .OrderBy(g => g.Key)
                .Select(g => new FolderGroup
                {
                    FolderPath = g.Key,
                    Songs = new ObservableCollection<Song>(
                        g.SelectMany(f => f.Songs))
                })
                .ToList();
        });

        Folders.Clear();
        foreach (var group in folderGroups)
            Folders.Add(group);
    }

    private static string GetGroupKey(string fullPath)
    {
        var dir = System.IO.Path.GetDirectoryName(fullPath);
        return string.IsNullOrEmpty(dir) ? fullPath : dir;
    }

    [RelayCommand]
    private async Task NavigateToFolderSongsAsync(FolderNode? folder)
    {
        if (folder == null) return;

        _logger.LogInformation("Navigating to folder songs: {FolderPath}", folder.FullPath);

        await Task.CompletedTask;
    }

    [RelayCommand]
    private void Play()
    {
        if (SelectedItem == null) return;

        var folderSongs = GetFolderSongs(SelectedItem).ToList();
        if (folderSongs.Count == 0) return;

        var playlist = _playlistService.CreatePlaylist($"Folder-{SelectedItem.Name}");
        _playlistService.SetCurrentPlaylist(playlist);

        foreach (var song in folderSongs)
            _playlistService.AddSongToPlaylist(playlist, song);

        _playlistService.PlayNext();

        if (_playlistService.CurrentSong != null)
        {
            _playbackStateService.Play(_playlistService.CurrentSong);
        }

        _logger.LogInformation("Playing folder: {FolderName}, SongCount: {Count}", SelectedItem.Name, folderSongs.Count);
    }

    [RelayCommand]
    private async Task AddToFavouriteAsync()
    {
        if (SelectedItem == null || _userPlaylistService == null) return;

        var folderSongs = GetFolderSongs(SelectedItem).ToList();
        foreach (var song in folderSongs)
        {
            await _userPlaylistService.AddToFavoritesAsync(song);
        }

        _logger.LogInformation("Added {Count} songs from folder to favourites: {FolderName}", folderSongs.Count, SelectedItem.Name);
    }

    [RelayCommand]
    private async Task AddToPlayListAsync(int playlistId)
    {
        if (SelectedItem == null || _userPlaylistService == null) return;

        var folderSongs = GetFolderSongs(SelectedItem).ToList();
        foreach (var song in folderSongs)
        {
            await _userPlaylistService.AddSongToPlaylistAsync(playlistId.ToString(), song);
        }

        _logger.LogInformation("Added {Count} songs to playlist: {PlaylistId}", folderSongs.Count, playlistId);
    }

    [RelayCommand]
    private async Task RescanFolderAsync()
    {
        if (SelectedItem == null) return;

        _logger.LogInformation("Rescanning folder: {FolderPath}", SelectedItem.FullPath);

        await Task.CompletedTask;
    }

    private IEnumerable<Song> GetFolderSongs(FolderNode folder)
    {
        if (folder.HasSongs)
            return folder.Songs;

        return GetAllDescendantSongs(folder);
    }

    private static IEnumerable<Song> GetAllDescendantSongs(FolderNode node)
    {
        foreach (var song in node.Songs)
            yield return song;

        foreach (var child in node.Children)
        {
            foreach (var song in GetAllDescendantSongs(child))
                yield return song;
        }
    }

    protected override void DisposeCore()
    {
        base.DisposeCore();
    }
}
