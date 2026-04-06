using System.Collections.ObjectModel;
using System.Linq;
using ReactiveUI;

namespace LocalMusicPlayer.Models;

public class ArtistGroup : ReactiveObject
{
    private string _artistName = string.Empty;

    public string ArtistName
    {
        get => _artistName;
        set => this.RaiseAndSetIfChanged(ref _artistName, value);
    }

    public ObservableCollection<Song> Songs { get; init; } = new();

    public int SongCount => Songs.Count;

    public string? CoverArtPath => Songs.FirstOrDefault()?.AlbumArtPath;
}