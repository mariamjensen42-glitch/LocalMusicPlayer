using System.Collections.ObjectModel;
using LocalMusicPlayer.Models;
using System.Collections.Generic;

namespace LocalMusicPlayer.Services;

public class MusicLibraryService : IMusicLibraryService
{
    public ObservableCollection<Song> Songs { get; } = [];
    public ObservableCollection<Song> FilteredSongs { get; } = [];

    public void Clear()
    {
        Songs.Clear();
        FilteredSongs.Clear();
    }

    public void AddSongs(IEnumerable<Song> songs)
    {
        foreach (var song in songs)
        {
            Songs.Add(song);
            FilteredSongs.Add(song);
        }
    }

    public void AddSong(Song song)
    {
        Songs.Add(song);
        FilteredSongs.Add(song);
    }

    public void RemoveSong(Song song)
    {
        Songs.Remove(song);
        FilteredSongs.Remove(song);
    }
}
