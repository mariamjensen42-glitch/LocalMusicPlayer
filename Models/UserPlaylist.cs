using System;
using System.Collections.Generic;

namespace LocalMusicPlayer.Models;

public class UserPlaylist
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedTime { get; init; } = DateTime.Now;
    public DateTime? ModifiedTime { get; set; }
    public List<string> SongFilePaths { get; set; } = new();
    public string? CoverArtPath { get; set; }
}