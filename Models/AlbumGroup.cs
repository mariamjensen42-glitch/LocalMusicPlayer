using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;
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

    public string Name => AlbumName;

    public AlbumGroup() { }

    public AlbumGroup(string albumName, IEnumerable<Song> songs)
    {
        _albumName = albumName;
        foreach (var song in songs)
        {
            Songs.Add(song);
        }
        if (Songs.Count > 0)
        {
            _artistName = Songs.First().Artist;
        }
    }
}
