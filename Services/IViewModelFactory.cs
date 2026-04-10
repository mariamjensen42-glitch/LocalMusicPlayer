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
}