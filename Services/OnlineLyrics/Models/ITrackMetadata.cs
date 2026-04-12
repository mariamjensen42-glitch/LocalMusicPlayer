using System.Collections.Generic;
using System.Linq;

namespace LocalMusicPlayer.Services.OnlineLyrics.Models;

public interface ITrackMetadata
{
    string? Title { get; set; }
    string? Artist { get; set; }
    string? Album { get; set; }
    string? AlbumArtist { get; set; }
    int? DurationMs { get; set; }
    string? Isrc { get; set; }
    List<string>? Language { get; set; }
}

public class TrackMetadata : ITrackMetadata
{
    public string? Title { get; set; }
    public string? Artist { get; set; }
    public string? Album { get; set; }
    public string? AlbumArtist { get; set; }
    public int? DurationMs { get; set; }
    public string? Isrc { get; set; }
    public List<string>? Language { get; set; }
}

public class TrackMultiArtistMetadata : ITrackMetadata
{
    public string? Title { get; set; }

    public string? Artist
    {
        get => string.Join(", ", Artists);
        set => Artists = (value ?? string.Empty).Split(", ").ToList();
    }

    public List<string> Artists { get; set; } = new();

    public string? Album { get; set; }

    public string? AlbumArtist
    {
        get => string.Join(", ", AlbumArtists);
        set => AlbumArtists = (value ?? string.Empty).Split(", ").ToList();
    }

    public List<string> AlbumArtists { get; set; } = new();

    public int? DurationMs { get; set; }
    public string? Isrc { get; set; }
    public List<string>? Language { get; set; }

    public static TrackMultiArtistMetadata GetTrackMultiArtistMetadata(ITrackMetadata track)
    {
        if (track is TrackMultiArtistMetadata trackMultiArtist)
            return trackMultiArtist;

        return new TrackMultiArtistMetadata
        {
            Artist = track.Artist,
            Album = track.Album,
            AlbumArtist = track.AlbumArtist,
            DurationMs = track.DurationMs,
            Isrc = track.Isrc,
            Language = track.Language,
            Title = track.Title
        };
    }
}
