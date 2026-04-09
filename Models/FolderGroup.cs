using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LocalMusicPlayer.Models;

public partial class FolderGroup : ObservableObject
{
    [ObservableProperty]
    private string _folderPath = string.Empty;

    public string FolderName => System.IO.Path.GetFileName(FolderPath);

    public ObservableCollection<Song> Songs { get; init; } = new();

    public int SongCount => Songs.Count;

    public string? CoverArtPath => Songs.FirstOrDefault()?.AlbumArtPath;
}
