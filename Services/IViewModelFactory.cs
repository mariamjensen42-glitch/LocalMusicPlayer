using LocalMusicPlayer.Models;
using LocalMusicPlayer.ViewModels;

namespace LocalMusicPlayer.Services;

public interface IViewModelFactory
{
    ViewModels.PlayerPageViewModel CreatePlayerPageViewModel();
    ViewModels.PlaylistManagementViewModel CreatePlaylistManagementViewModel();
    ViewModels.SettingsViewModel CreateSettingsViewModel();
    ViewModels.StatisticsViewModel CreateStatisticsViewModel();
    ViewModels.LibraryCategoryViewModel CreateLibraryCategoryViewModel();
    ViewModels.QueueViewModel CreateQueueViewModel();
    ViewModels.PlayHistoryViewModel CreatePlayHistoryViewModel();
    ViewModels.LibraryBrowserViewModel CreateLibraryBrowserViewModel();
    ViewModels.StatisticsReportViewModel CreateStatisticsReportViewModel();
    ViewModels.ArtistDetailViewModel CreateArtistDetailViewModel(ArtistGroup artistGroup);
    ViewModels.AlbumDetailViewModel CreateAlbumDetailViewModel(AlbumGroup albumGroup);
}
