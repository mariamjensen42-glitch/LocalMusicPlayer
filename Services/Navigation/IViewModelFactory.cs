using System;
using LocalMusicPlayer.Models;
using LocalMusicPlayer.ViewModels;

namespace LocalMusicPlayer.Services;

public interface IViewModelFactory
{
    PlayerPageViewModel CreatePlayerPageViewModel();
    PlaylistManagementViewModel CreatePlaylistManagementViewModel();
    PlaylistListViewModel CreatePlaylistListViewModel();
    SettingsViewModel CreateSettingsViewModel();
    QueueViewModel CreateQueueViewModel();
    ArtistDetailViewModel CreateArtistDetailViewModel(ArtistGroup artistGroup);
    AlbumDetailViewModel CreateAlbumDetailViewModel(AlbumGroup albumGroup);
    MusicLibraryViewModel CreateMusicLibraryViewModel();
    MetadataEditorViewModel CreateMetadataEditorViewModel(Song song, Action? onSaved = null);
}
