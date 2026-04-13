using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalMusicPlayer.Models;
using LocalMusicPlayer.Services;
using Microsoft.Extensions.Logging;

namespace LocalMusicPlayer.ViewModels;

public class MenuOption
{
    public string Title { get; init; } = string.Empty;
    public object? Tag { get; init; }
    public ObservableCollection<MenuOption> Children { get; init; } = new();
}

public partial class SongListViewModel : ViewModelBase
{
    private readonly IMusicLibraryService _musicLibraryService;
    private readonly IPlaylistService _playlistService;
    private readonly IPlaybackStateService _playbackStateService;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    private readonly IUserPlaylistService _userPlaylistService;
    private readonly IOnlineLyricsService? _onlineLyricsService;
    private readonly IFileManagerService _fileManagerService;
    private readonly ILogger<SongListViewModel> _logger;

    [ObservableProperty] private Song? _selectedSong;

    [ObservableProperty] private IList<Song> _selectedSongs = new List<Song>();

    [ObservableProperty] private ObservableCollection<MenuOption> _menuOptions = new();

    public ObservableCollection<Song> Songs => _musicLibraryService.FilteredSongs;

    public event Action<Song?>? ScrollToCurrentSongRequested;

    public SongListViewModel(
        IMusicLibraryService musicLibraryService,
        IPlaylistService playlistService,
        IPlaybackStateService playbackStateService,
        INavigationService navigationService,
        IDialogService dialogService,
        IUserPlaylistService userPlaylistService,
        IFileManagerService fileManagerService,
        ILogger<SongListViewModel> logger,
        IOnlineLyricsService? onlineLyricsService = null)
    {
        _musicLibraryService = musicLibraryService;
        _playlistService = playlistService;
        _playbackStateService = playbackStateService;
        _navigationService = navigationService;
        _dialogService = dialogService;
        _userPlaylistService = userPlaylistService;
        _fileManagerService = fileManagerService;
        _logger = logger;
        _onlineLyricsService = onlineLyricsService;

        InitializeMenuOptions();

        SubscribeEvent(
            () => _playbackStateService.CurrentSongChanged += OnCurrentSongChanged,
            () => _playbackStateService.CurrentSongChanged -= OnCurrentSongChanged);

        SubscribeEvent(
            () => _userPlaylistService.PlaylistsChanged += OnPlaylistsChanged,
            () => _userPlaylistService.PlaylistsChanged -= OnPlaylistsChanged);
    }

    private void InitializeMenuOptions()
    {
        MenuOptions.Add(new MenuOption { Title = "Play", Tag = "Play" });
        MenuOptions.Add(new MenuOption { Title = "AddToFavourite", Tag = "AddToFavourite" });
        MenuOptions.Add(new MenuOption
        {
            Title = "AddToPlayList",
            Tag = "AddToPlayList",
            Children = new ObservableCollection<MenuOption>()
        });
        MenuOptions.Add(new MenuOption
        {
            Title = "ConvertAudio",
            Tag = "ConvertAudio",
            Children = new ObservableCollection<MenuOption>
            {
                new() { Title = "WAV", Tag = "wav" },
                new() { Title = "MP3", Tag = "mp3" },
                new() { Title = "FLAC", Tag = "flac" },
                new() { Title = "OGG", Tag = "ogg" },
                new() { Title = "OPUS", Tag = "opus" }
            }
        });
        MenuOptions.Add(new MenuOption { Title = "AddToCurrentPlayList", Tag = "AddToCurrentPlayList" });
        MenuOptions.Add(new MenuOption { Title = "ReGetLyrics", Tag = "ReGetLyrics" });
        MenuOptions.Add(new MenuOption { Title = "OpenInExplorer", Tag = "OpenInExplorer" });
        MenuOptions.Add(new MenuOption { Title = "Properties", Tag = "Properties" });
        MenuOptions.Add(new MenuOption { Title = "Delete", Tag = "Delete" });

        UpdatePlaylistSubMenu();
    }

    public void UpdatePlaylistSubMenu()
    {
        var playlistOption = MenuOptions.FirstOrDefault(m => (string?)m.Tag == "AddToPlayList");
        if (playlistOption == null) return;

        playlistOption.Children.Clear();
        foreach (var playlist in _userPlaylistService.UserPlaylists)
        {
            playlistOption.Children.Add(new MenuOption
            {
                Title = playlist.Name,
                Tag = playlist.Id
            });
        }
    }

    public void ReceiveNavigation()
    {
        ScrollToCurrentSong();
    }

    public void ScrollToCurrentSong()
    {
        try
        {
            var currentSong = _playbackStateService.CurrentSong;
            if (currentSong is not null)
            {
                var targetSong = Songs.FirstOrDefault(s => s.FilePath == currentSong.FilePath);
                if (targetSong is not null)
                {
                    SelectedSong = targetSong;
                    ScrollToCurrentSongRequested?.Invoke(targetSong);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ScrollToCurrentSong failed");
        }
    }

    [RelayCommand]
    private async Task DoubleTapPlayAsync()
    {
        if (SelectedSong == null) return;

        await PlayWithQueueAsync(Songs.ToList(), SelectedSong);
    }

    [RelayCommand]
    private async Task PlayAsync()
    {
        var songsToPlay = GetEffectiveSelectedSongs();
        if (songsToPlay.Count == 0) return;

        var firstSong = songsToPlay[0];
        var queueSource = SelectedSongs.Count > 1 ? songsToPlay : Songs.ToList();

        await PlayWithQueueAsync(queueSource, firstSong);
    }

    private async Task PlayWithQueueAsync(List<Song> queue, Song songToPlay)
    {
        try
        {
            _playlistService.ClearPlaylist();
            foreach (var song in queue)
            {
                _playlistService.AddSongToPlaylist(_playlistService.CurrentPlaylist!, song);
            }

            _playlistService.PlaySong(songToPlay);
            _playbackStateService.Play(songToPlay);

            _logger.LogInformation("Played song: {Title} by {Artist}", songToPlay.Title, songToPlay.Artist);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to play song: {Title}", songToPlay.Title);
        }
    }

    [RelayCommand]
    private async Task AddToFavouriteAsync()
    {
        var songs = GetEffectiveSelectedSongs();
        if (songs.Count == 0) return;

        try
        {
            foreach (var song in songs)
            {
                if (song.IsFavorite)
                    await _userPlaylistService.RemoveFromFavoritesAsync(song);
                else
                    await _userPlaylistService.AddToFavoritesAsync(song);
            }

            _logger.LogInformation("Toggled favourite for {Count} songs", songs.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to toggle favourite for songs");
        }
    }

    [RelayCommand]
    private async Task AddToPlayListAsync(object? tag)
    {
        var songs = GetEffectiveSelectedSongs();
        if (songs.Count == 0 || tag is not string playlistId) return;

        try
        {
            foreach (var song in songs)
            {
                await _userPlaylistService.AddSongToPlaylistAsync(playlistId, song);
            }

            _logger.LogInformation("Added {Count} songs to playlist {PlaylistId}", songs.Count, playlistId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add songs to playlist {PlaylistId}", playlistId);
        }
    }

    [RelayCommand]
    private async Task ConvertAudioAsync(string? format)
    {
        var songs = GetEffectiveSelectedSongs();
        if (songs.Count == 0 || string.IsNullOrEmpty(format)) return;

        try
        {
            await _dialogService.ShowMessageDialogAsync("ConvertAudio",
                $"Converting {songs.Count} files to {format.ToUpperInvariant()} format...");

            _logger.LogInformation("Starting audio conversion to {Format} for {Count} files", format, songs.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Audio conversion failed for format {Format}", format);
        }
    }

    [RelayCommand]
    private async Task AddToCurrentPlayListAsync()
    {
        var songs = GetEffectiveSelectedSongs();
        if (songs.Count == 0 || _playlistService.CurrentPlaylist == null) return;

        try
        {
            foreach (var song in songs)
            {
                _playlistService.AddSongToPlaylist(_playlistService.CurrentPlaylist, song);
            }

            _logger.LogInformation("Added {Count} songs to current playlist", songs.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add songs to current playlist");
        }
    }

    [RelayCommand]
    private async Task ReGetLyricsAsync()
    {
        var songs = GetEffectiveSelectedSongs();
        if (songs.Count == 0 || _onlineLyricsService == null) return;

        try
        {
            foreach (var song in songs)
            {
                var result = await _onlineLyricsService.SearchLyricsAsync(song);
                if (result != null)
                {
                    await _dialogService.ShowLyricSearchResultDialogAsync(song, result);
                }
            }

            _logger.LogInformation("Re-fetched lyrics for {Count} songs", songs.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to re-fetch lyrics");
        }
    }

    [RelayCommand]
    private void OpenInExplorer()
    {
        var songs = GetEffectiveSelectedSongs();
        if (songs.Count == 0) return;

        var filePath = songs[0].FilePath;
        if (!File.Exists(filePath))
        {
            _logger.LogWarning("File not found: {FilePath}", filePath);
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"/select,\"{filePath}\"",
                UseShellExecute = true
            });

            _logger.LogInformation("Opened explorer for file: {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open explorer for file: {FilePath}", filePath);
        }
    }

    [RelayCommand]
    private async Task MusicDetailAsync()
    {
        var songs = GetEffectiveSelectedSongs();
        if (songs.Count == 0) return;

        try
        {
            await _dialogService.ShowMetadataEditorDialogAsync(songs[0]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to show music details");
        }
    }

    [RelayCommand]
    private async Task DeleteMenuItemAsync()
    {
        var songs = GetEffectiveSelectedSongs();
        if (songs.Count == 0) return;

        var message = songs.Count > 1
            ? $"Are you sure you want to delete {songs.Count} selected files from disk?"
            : $"Are you sure you want to delete \"{songs[0].Title}\" from disk?";

        try
        {
            await _dialogService.ShowMessageDialogAsync("ConfirmDelete", message);

            var filePaths = songs.Select(s => s.FilePath).ToList();
            await _fileManagerService.DeleteFilesAsync(filePaths, new Progress<FileOperationProgress>());

            foreach (var song in songs)
            {
                _musicLibraryService.RemoveSong(song);
            }

            _logger.LogInformation("Deleted {Count} songs from disk", songs.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete songs from disk");
        }
    }

    [RelayCommand]
    private void NavigateToArtist(string? artist)
    {
        if (string.IsNullOrWhiteSpace(artist)) return;

        _logger.LogInformation("Navigating to artist: {Artist}", artist);
    }

    [RelayCommand]
    private void NavigateToAlbum(string? album)
    {
        if (string.IsNullOrWhiteSpace(album)) return;

        _logger.LogInformation("Navigating to album: {Album}", album);
    }

    private List<Song> GetEffectiveSelectedSongs()
    {
        if (SelectedSongs.Count > 0)
            return SelectedSongs.Cast<Song>().ToList();

        if (SelectedSong != null)
            return new List<Song> { SelectedSong };

        return new List<Song>();
    }

    private void OnCurrentSongChanged(object? sender, Song? song)
    {
        ScrollToCurrentSong();
    }

    private void OnPlaylistsChanged(object? sender, EventArgs e)
    {
        UpdatePlaylistSubMenu();
    }

    protected override void DisposeCore()
    {
        base.DisposeCore();
    }
}
