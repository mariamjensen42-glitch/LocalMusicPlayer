using System.Collections.ObjectModel;
using System.Linq;
using ReactiveUI;

namespace LocalMusicPlayer.Models;

public class FolderGroup : ReactiveObject
{
    private string _folderPath = string.Empty;

    public string FolderPath
    {
        get => _folderPath;
        set => this.RaiseAndSetIfChanged(ref _folderPath, value);
    }

    public string FolderName => System.IO.Path.GetFileName(FolderPath);

    public ObservableCollection<Song> Songs { get; init; } = new();

    public int SongCount => Songs.Count;

    public string? CoverArtPath => Songs.FirstOrDefault()?.AlbumArtPath;
}