using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LocalMusicPlayer.Models;

public partial class AlbumGroup : ObservableObject
{
    [ObservableProperty]
    private string _albumName = string.Empty;

    [ObservableProperty]
    private string _artistName = string.Empty;

    public ObservableCollection<Song> Songs { get; init; } = new();

    public int SongCount => Songs.Count;

    public TimeSpan TotalDuration => Songs.Aggregate(TimeSpan.Zero, (acc, s) => acc + s.Duration);

    public string? CoverArtPath => Songs.FirstOrDefault()?.AlbumArtPath;
}
