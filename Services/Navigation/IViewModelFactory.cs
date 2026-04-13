using System.Collections.Generic;
using LocalMusicPlayer.Models;
using LocalMusicPlayer.ViewModels;

namespace LocalMusicPlayer.Services;

public interface IViewModelFactory
{
    PlayerPageViewModel CreatePlayerPageViewModel();
    PlaylistManagementViewModel CreatePlaylistManagementViewModel();
    PlaylistListViewModel CreatePlaylistListViewModel();
    SettingsViewModel CreateSettingsViewModel();
    StatisticsViewModel CreateStatisticsViewModel();
    LibraryCategoryViewModel CreateLibraryCategoryViewModel();
    QueueViewModel CreateQueueViewModel();
    PlayHistoryViewModel CreatePlayHistoryViewModel();
    LibraryBrowserViewModel CreateLibraryBrowserViewModel(BrowserCategory initialCategory = BrowserCategory.Songs);
    StatisticsReportViewModel CreateStatisticsReportViewModel();
    ArtistDetailViewModel CreateArtistDetailViewModel(ArtistGroup artistGroup);
    AlbumDetailViewModel CreateAlbumDetailViewModel(AlbumGroup albumGroup);
    HomeViewModel CreateHomeViewModel();
    ArtistsPageViewModel CreateArtistsPageViewModel();
    AlbumsPageViewModel CreateAlbumsPageViewModel();
    MetadataEditorViewModel CreateMetadataEditorViewModel(Song song, System.Action? onSaved = null);
    BatchMetadataEditorViewModel CreateBatchMetadataEditorViewModel(IEnumerable<Song> songs, System.Action? onSaved = null);
    SmartPlaylistSongsViewModel CreateSmartPlaylistSongsViewModel(SmartPlaylist smartPlaylist);
    MusicBrowseViewModel CreateMusicBrowseViewModel();
}
