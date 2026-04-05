using System.Collections.Generic;

namespace LocalMusicPlayer.Models;

public class Playlist
{
    public string Name { get; init; } = string.Empty;
    public List<Song> Songs { get; init; } = [];
}
