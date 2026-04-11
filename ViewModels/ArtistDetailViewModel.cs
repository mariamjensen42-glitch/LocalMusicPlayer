using CommunityToolkit.Mvvm.ComponentModel;
using LocalMusicPlayer.Models;
using LocalMusicPlayer.Services;

namespace LocalMusicPlayer.ViewModels;

public partial class ArtistDetailViewModel : DetailViewModelBase
{
    public override string DetailName => ArtistName;
    public override string? CoverArtPath { get; }

    public string ArtistName { get; }
    public string Subtitle => $"本地: {SongCount} 首";

    public ArtistDetailViewModel(
        ArtistGroup artistGroup,
        IMusicPlayerService musicPlayerService,
        IPlaylistService playlistService,
        IStatisticsService statisticsService,
        INavigationService navigationService,
        IDialogService dialogService)
        : base(musicPlayerService, playlistService, statisticsService, navigationService, dialogService)
    {
        ArtistName = artistGroup.ArtistName;
        CoverArtPath = artistGroup.CoverArtPath;

        LoadSongs(artistGroup.Songs);
    }
}
