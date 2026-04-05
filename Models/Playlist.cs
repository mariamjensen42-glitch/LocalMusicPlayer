using System.Collections.ObjectModel;

namespace LocalMusicPlayer.Models;

public class Playlist
{
    public string Name { get; init; } = string.Empty;
    public ObservableCollection<Song> Songs { get; set; } = new();
}
