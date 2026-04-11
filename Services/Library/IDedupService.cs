using System.Collections.Generic;
using LocalMusicPlayer.Models;

namespace LocalMusicPlayer.Services;

public interface IDedupService
{
    IReadOnlyList<DuplicateGroup> FindDuplicates(IEnumerable<Song> songs);
    void RemoveDuplicate(DuplicateGroup group, Song songToKeep);
}

public class DuplicateGroup
{
    public List<Song> Songs { get; } = new();

    public int DuplicateCount => Songs.Count - 1;

    public bool IsCompleteMatch { get; set; }
}
