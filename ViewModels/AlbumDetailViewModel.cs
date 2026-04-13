using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

    public string SecondTitle => string.Join(" · ",
        Songs.Select(s => s.Artist)
            .Where(a => !string.IsNullOrEmpty(a))
            .Distinct());

    public string ThirdTitle
    {
        get
        {
            var yearInfo = Songs.FirstOrDefault()?.Year > 0 ? $"{Songs.First().Year} · " : "";
            return $"{yearInfo}{SongCount} 首";
        }
    }

    [ObservableProperty]
    private Song? _selectedSong;

    [ObservableProperty]
    private List<Song> _selectedSongs = new();

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

    [RelayCommand]
    private void ToggleFavorite(Song? song)
    {
        if (song != null)
        {
            song.IsFavorite = !song.IsFavorite;
        }
    }

    [RelayCommand]
    private void AddToCurrentPlaylist(Song song)
    {
    }

    [RelayCommand]
    private void OpenInExplorer(Song song)
    {
    }

    [RelayCommand]
    private void NavigateToArtist(string artist)
    {
    }

    [RelayCommand]
    private void NavigateToAlbum(string album)
    {
    }

    partial void OnSelectedSongChanged(Song? value)
    {
        if (value != null)
        {
            SelectedSongs = new List<Song> { value };
        }
    }
}
