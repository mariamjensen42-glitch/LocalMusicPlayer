using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LocalMusicPlayer.Models;

namespace LocalMusicPlayer.Services;

public class DedupService : IDedupService
{
    public IReadOnlyList<DuplicateGroup> FindDuplicates(IEnumerable<Song> songs)
    {
        var songList = songs.ToList();
        var groups = new List<DuplicateGroup>();

        var completeMatches = songList
            .GroupBy(s => new { s.Title, s.Artist, s.Duration, s.FileSizeBytes })
            .Where(g => g.Count() > 1);

        foreach (var group in completeMatches)
        {
            var duplicateGroup = new DuplicateGroup
            {
                IsCompleteMatch = true
            };
            duplicateGroup.Songs.AddRange(group.ToList());
            groups.Add(duplicateGroup);
        }

        return groups;
    }

    public void RemoveDuplicate(DuplicateGroup group, Song songToKeep)
    {
        foreach (var song in group.Songs)
        {
            if (song != songToKeep)
            {
                song.IsFavorite = false;
            }
        }
    }
}
