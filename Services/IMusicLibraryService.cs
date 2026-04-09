using System.Collections.Generic;
using System.Collections.ObjectModel;
using LocalMusicPlayer.Models;

namespace LocalMusicPlayer.Services;

public interface IMusicLibraryService
{
    ObservableCollection<Song> Songs { get; }
    ObservableCollection<Song> FilteredSongs { get; }
    void Clear();
    void AddSongs(IEnumerable<Song> songs);
    void AddSong(Song song);
}
