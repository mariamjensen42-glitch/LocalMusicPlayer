using System;
using System.Collections.ObjectModel;
using System.Linq;
using ReactiveUI;

namespace LocalMusicPlayer.Models;

public class AlbumGroup : ReactiveObject
{
    private string _albumName = string.Empty;

    public string AlbumName
    {
        get => _albumName;
        set => this.RaiseAndSetIfChanged(ref _albumName, value);
    }

    private string _artistName = string.Empty;

    public string ArtistName
    {
        get => _artistName;
        set => this.RaiseAndSetIfChanged(ref _artistName, value);
    }

    public ObservableCollection<Song> Songs { get; init; } = new();

    public int SongCount => Songs.Count;

    public TimeSpan TotalDuration => Songs.Aggregate(TimeSpan.Zero, (acc, s) => acc + s.Duration);

    public string? CoverArtPath => Songs.FirstOrDefault()?.AlbumArtPath;
}