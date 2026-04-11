using CommunityToolkit.Mvvm.ComponentModel;
using LocalMusicPlayer.Models;
using LocalMusicPlayer.Services;

namespace LocalMusicPlayer.ViewModels;

public partial class AlbumDetailViewModel : DetailViewModelBase
{
    public override string DetailName => AlbumName;
    public override string? CoverArtPath { get; }

    public string AlbumName { get; }
    public string ArtistName { get; }

    public AlbumDetailViewModel(
        AlbumGroup albumGroup,
        IMusicPlayerService musicPlayerService,
        IPlaylistService playlistService,
        IStatisticsService statisticsService,
        INavigationService navigationService)
        : base(musicPlayerService, playlistService, statisticsService, navigationService)
    {
        AlbumName = albumGroup.AlbumName;
        ArtistName = albumGroup.ArtistName;
        CoverArtPath = albumGroup.CoverArtPath;

        LoadSongs(albumGroup.Songs);
    }
}
