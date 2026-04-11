using CommunityToolkit.Mvvm.ComponentModel;
using LocalMusicPlayer.Models;
using LocalMusicPlayer.Services;

namespace LocalMusicPlayer.ViewModels;

public partial class ArtistDetailViewModel : DetailViewModelBase
{
    public override string DetailName => ArtistName;
    public override string? CoverArtPath { get; }

    public string ArtistName { get; }

    public ArtistDetailViewModel(
        ArtistGroup artistGroup,
        IMusicPlayerService musicPlayerService,
        IPlaylistService playlistService,
        IStatisticsService statisticsService,
        INavigationService navigationService)
        : base(musicPlayerService, playlistService, statisticsService, navigationService)
    {
        ArtistName = artistGroup.ArtistName;
        CoverArtPath = artistGroup.CoverArtPath;

        LoadSongs(artistGroup.Songs);
    }
}
