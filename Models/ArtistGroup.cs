using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LocalMusicPlayer.Models;

public partial class ArtistGroup : ObservableObject
{
    [ObservableProperty]
    private string _artistName = string.Empty;

    public ObservableCollection<Song> Songs { get; init; } = new();

    public int SongCount => Songs.Count;

    public string? CoverArtPath => Songs.FirstOrDefault()?.AlbumArtPath;

    public string Name => ArtistName;

    public ArtistGroup() { }

    public ArtistGroup(string artistName, IEnumerable<Song> songs)
    {
        _artistName = artistName;
        foreach (var song in songs)
        {
            Songs.Add(song);
        }
    }
}
