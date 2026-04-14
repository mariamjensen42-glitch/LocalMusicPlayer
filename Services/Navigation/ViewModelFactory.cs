using System;
using LocalMusicPlayer.Models;
using LocalMusicPlayer.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace LocalMusicPlayer.Services;

public class ViewModelFactory : IViewModelFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ViewModelFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public PlayerPageViewModel CreatePlayerPageViewModel()
        => ActivatorUtilities.CreateInstance<PlayerPageViewModel>(_serviceProvider);

    public PlaylistManagementViewModel CreatePlaylistManagementViewModel()
        => ActivatorUtilities.CreateInstance<PlaylistManagementViewModel>(_serviceProvider);

    public PlaylistListViewModel CreatePlaylistListViewModel()
        => ActivatorUtilities.CreateInstance<PlaylistListViewModel>(_serviceProvider);

    public SettingsViewModel CreateSettingsViewModel()
        => ActivatorUtilities.CreateInstance<SettingsViewModel>(_serviceProvider);

    public QueueViewModel CreateQueueViewModel()
        => ActivatorUtilities.CreateInstance<QueueViewModel>(_serviceProvider);

    public ArtistDetailViewModel CreateArtistDetailViewModel(ArtistGroup artistGroup)
        => ActivatorUtilities.CreateInstance<ArtistDetailViewModel>(_serviceProvider, artistGroup);

    public AlbumDetailViewModel CreateAlbumDetailViewModel(AlbumGroup albumGroup)
        => ActivatorUtilities.CreateInstance<AlbumDetailViewModel>(_serviceProvider, albumGroup);

    public MusicLibraryViewModel CreateMusicLibraryViewModel()
        => ActivatorUtilities.CreateInstance<MusicLibraryViewModel>(_serviceProvider);

    public MetadataEditorViewModel CreateMetadataEditorViewModel(Song song, Action? onSaved = null)
        => ActivatorUtilities.CreateInstance<MetadataEditorViewModel>(_serviceProvider, song!, onSaved!);
}
