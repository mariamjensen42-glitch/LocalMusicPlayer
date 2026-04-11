using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalMusicPlayer.Models;
using LocalMusicPlayer.Services;

namespace LocalMusicPlayer.ViewModels;

public partial class PlaylistListViewModel : ViewModelBase
{
    private readonly IUserPlaylistService _playlistService;
    private readonly IDialogService _dialogService;

    [ObservableProperty] private UserPlaylist? _selectedPlaylist;

    public ObservableCollection<UserPlaylist> UserPlaylists => _playlistService.UserPlaylists;

    public Action<UserPlaylist>? OnPlaylistSelected { get; set; }

    public PlaylistListViewModel(IUserPlaylistService playlistService, IDialogService dialogService)
    {
        _playlistService = playlistService;
        _dialogService = dialogService;

        _playlistService.PlaylistsChanged += OnPlaylistsChanged;
    }

    private void OnPlaylistsChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(UserPlaylists));
    }

    protected override void DisposeCore()
    {
        _playlistService.PlaylistsChanged -= OnPlaylistsChanged;
        base.DisposeCore();
    }

    [RelayCommand]
    private async Task CreatePlaylistAsync()
    {
        var name = await _dialogService.ShowInputDialogAsync("New Playlist", "");
        if (!string.IsNullOrWhiteSpace(name))
        {
            await _playlistService.CreatePlaylistAsync(name);
        }
    }

    [RelayCommand]
    private void SelectPlaylist(UserPlaylist playlist)
    {
        SelectedPlaylist = playlist;
        OnPlaylistSelected?.Invoke(playlist);
    }

    [RelayCommand]
    private async Task DeletePlaylistAsync(UserPlaylist playlist)
    {
        if (playlist.Id == "favorites") return;
        await _playlistService.DeletePlaylistAsync(playlist.Id);
    }
}
