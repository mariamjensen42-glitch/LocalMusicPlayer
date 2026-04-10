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
    void RemoveSong(Song song);

    List<ArtistInfo> GetArtists();
    List<AlbumInfo> GetAlbums();
    List<GenreInfo> GetGenres();
    List<Song> GetSongsByArtist(string artist);
    List<Song> GetSongsByAlbum(string album);
    List<Song> GetSongsByGenre(string genre);
    List<FolderNode> GetFolderStructure();
}
