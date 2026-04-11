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
    public string Subtitle => $"{ArtistName} · {SongCount} 首";

    public AlbumDetailViewModel(
        AlbumGroup albumGroup,
        IMusicPlayerService musicPlayerService,
        IPlaylistService playlistService,
        IStatisticsService statisticsService,
        INavigationService navigationService,
        IDialogService dialogService)
        : base(musicPlayerService, playlistService, statisticsService, navigationService, dialogService)
    {
        AlbumName = albumGroup.AlbumName;
        ArtistName = albumGroup.ArtistName;
        CoverArtPath = albumGroup.CoverArtPath;

        LoadSongs(albumGroup.Songs);
    }
}
