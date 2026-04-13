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

    public StatisticsViewModel CreateStatisticsViewModel()
        => ActivatorUtilities.CreateInstance<StatisticsViewModel>(_serviceProvider);

    public LibraryCategoryViewModel CreateLibraryCategoryViewModel()
        => ActivatorUtilities.CreateInstance<LibraryCategoryViewModel>(_serviceProvider);

    public QueueViewModel CreateQueueViewModel()
        => ActivatorUtilities.CreateInstance<QueueViewModel>(_serviceProvider);

    public PlayHistoryViewModel CreatePlayHistoryViewModel()
        => ActivatorUtilities.CreateInstance<PlayHistoryViewModel>(_serviceProvider);

    public LibraryBrowserViewModel CreateLibraryBrowserViewModel(
        BrowserCategory initialCategory = BrowserCategory.Songs)
        => ActivatorUtilities.CreateInstance<LibraryBrowserViewModel>(_serviceProvider, initialCategory);

    public StatisticsReportViewModel CreateStatisticsReportViewModel()
        => ActivatorUtilities.CreateInstance<StatisticsReportViewModel>(_serviceProvider);

    public ArtistDetailViewModel CreateArtistDetailViewModel(ArtistGroup artistGroup)
        => ActivatorUtilities.CreateInstance<ArtistDetailViewModel>(_serviceProvider, artistGroup);

    public AlbumDetailViewModel CreateAlbumDetailViewModel(AlbumGroup albumGroup)
        => ActivatorUtilities.CreateInstance<AlbumDetailViewModel>(_serviceProvider, albumGroup);

    public HomeViewModel CreateHomeViewModel()
        => ActivatorUtilities.CreateInstance<HomeViewModel>(_serviceProvider);

    public ArtistsPageViewModel CreateArtistsPageViewModel()
        => ActivatorUtilities.CreateInstance<ArtistsPageViewModel>(_serviceProvider);

    public AlbumsPageViewModel CreateAlbumsPageViewModel()
        => ActivatorUtilities.CreateInstance<AlbumsPageViewModel>(_serviceProvider);

    public MetadataEditorViewModel CreateMetadataEditorViewModel(Song song, Action? onSaved = null)
        => ActivatorUtilities.CreateInstance<MetadataEditorViewModel>(_serviceProvider, song!, onSaved!);

    public BatchMetadataEditorViewModel CreateBatchMetadataEditorViewModel(
        System.Collections.Generic.IEnumerable<Song> songs, Action? onSaved = null)
        => ActivatorUtilities.CreateInstance<BatchMetadataEditorViewModel>(_serviceProvider, songs, onSaved!);

    public SmartPlaylistSongsViewModel CreateSmartPlaylistSongsViewModel(SmartPlaylist smartPlaylist)
        => ActivatorUtilities.CreateInstance<SmartPlaylistSongsViewModel>(_serviceProvider, smartPlaylist);
}
