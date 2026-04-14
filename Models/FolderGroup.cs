using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LocalMusicPlayer.Models;

public partial class FolderGroup : ObservableObject
{
    [ObservableProperty]
    private string _folderPath = string.Empty;

    public string FolderName => System.IO.Path.GetFileName(FolderPath);

    public string DisplayName => string.IsNullOrEmpty(FolderPath) ? "Root" : FolderName;

    public string Name => DisplayName;

    public ObservableCollection<Song> Songs { get; init; } = new();

    public int SongCount => Songs.Count;

    public string? CoverArtPath => Songs.FirstOrDefault()?.AlbumArtPath;

    public FolderGroup() { }

    public FolderGroup(string folderPath, IEnumerable<Song> songs)
    {
        _folderPath = folderPath;
        foreach (var song in songs)
        {
            Songs.Add(song);
        }
    }
}
