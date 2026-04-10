using System.Collections.ObjectModel;
using LocalMusicPlayer.Models;
using System.Collections.Generic;
using System.Linq;
using System.IO;

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

    public List<ArtistInfo> GetArtists()
    {
        return Songs
            .GroupBy(s => s.Artist)
            .Select(g => new ArtistInfo
            {
                Name = g.Key,
                SongCount = g.Count(),
                AlbumCount = g.Select(s => s.Album).Distinct().Count(),
                CoverPath = g.FirstOrDefault(s => !string.IsNullOrEmpty(s.AlbumArtPath))?.AlbumArtPath,
                Songs = g.ToList()
            })
            .OrderBy(a => a.Name)
            .ToList();
    }

    public List<AlbumInfo> GetAlbums()
    {
        return Songs
            .GroupBy(s => new { s.Album, s.Artist })
            .Select(g => new AlbumInfo
            {
                Title = g.Key.Album,
                Artist = g.Key.Artist,
                SongCount = g.Count(),
                Year = g.Max(s => s.TrackNumber),
                CoverPath = g.FirstOrDefault(s => !string.IsNullOrEmpty(s.AlbumArtPath))?.AlbumArtPath,
                Songs = g.ToList()
            })
            .OrderBy(a => a.Title)
            .ToList();
    }

    public List<GenreInfo> GetGenres()
    {
        return Songs
            .Where(s => !string.IsNullOrEmpty(s.Genre))
            .GroupBy(s => s.Genre)
            .Select(g => new GenreInfo
            {
                Name = g.Key,
                SongCount = g.Count(),
                ArtistCount = g.Select(s => s.Artist).Distinct().Count(),
                Songs = g.ToList()
            })
            .OrderBy(g => g.Name)
            .ToList();
    }

    public List<Song> GetSongsByArtist(string artist)
    {
        return Songs
            .Where(s => s.Artist.Equals(artist, System.StringComparison.OrdinalIgnoreCase))
            .OrderBy(s => s.Album)
            .ThenBy(s => s.TrackNumber)
            .ToList();
    }

    public List<Song> GetSongsByAlbum(string album)
    {
        return Songs
            .Where(s => s.Album.Equals(album, System.StringComparison.OrdinalIgnoreCase))
            .OrderBy(s => s.TrackNumber)
            .ToList();
    }

    public List<Song> GetSongsByGenre(string genre)
    {
        return Songs
            .Where(s => s.Genre.Equals(genre, System.StringComparison.OrdinalIgnoreCase))
            .OrderBy(s => s.Artist)
            .ThenBy(s => s.Album)
            .ToList();
    }

    public List<FolderNode> GetFolderStructure()
    {
        var rootNodes = new List<FolderNode>();

        foreach (var song in Songs)
        {
            var directory = Path.GetDirectoryName(song.FilePath);
            if (string.IsNullOrEmpty(directory)) continue;

            var parts = directory.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            AddToFolderTree(rootNodes, parts, 0, song);
        }

        return rootNodes.OrderBy(n => n.Name).ToList();
    }

    private void AddToFolderTree(List<FolderNode> nodes, string[] parts, int index, Song song)
    {
        if (index >= parts.Length) return;

        var name = parts[index];
        var fullPath = string.Join(Path.DirectorySeparatorChar.ToString(), parts.Take(index + 1));

        var existingNode = nodes.FirstOrDefault(n => n.Name == name);
        if (existingNode == null)
        {
            existingNode = new FolderNode
            {
                Name = name,
                FullPath = fullPath
            };
            nodes.Add(existingNode);
        }

        existingNode.Songs.Add(song);
        existingNode.SongCount = existingNode.Songs.Count;

        if (index < parts.Length - 1)
        {
            AddToFolderTree(existingNode.Children.ToList(), parts, index + 1, song);
        }
    }
}
