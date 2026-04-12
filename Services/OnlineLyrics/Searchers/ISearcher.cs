using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LocalMusicPlayer.Services.OnlineLyrics.Models;

namespace LocalMusicPlayer.Services.OnlineLyrics.Searchers;

public interface ISearcher
{
    string Name { get; }
    string DisplayName { get; }
    Searchers SearcherType { get; }

    Task<ISearchResult?> SearchForResult(ITrackMetadata track);
    Task<ISearchResult?> SearchForResult(ITrackMetadata track, CompareHelper.MatchType minimumMatch);
    Task<List<ISearchResult>> SearchForResults(ITrackMetadata track);
    Task<List<ISearchResult>> SearchForResults(ITrackMetadata track, bool fullSearch);
    Task<List<ISearchResult>?> SearchForResults(string searchString);
}

public interface ISearchResult
{
    ISearcher Searcher { get; }
    string Title { get; }
    string[] Artists { get; }
    string Artist => string.Join(", ", Artists);
    string Album { get; }
    string[]? AlbumArtists { get; }
    string? AlbumArtist => string.Join(", ", AlbumArtists ?? Array.Empty<string>());
    int? DurationMs { get; }
    CompareHelper.MatchType? MatchType { get; }
    void SetMatchType(CompareHelper.MatchType? matchType);
}

public enum Searchers
{
    QQMusic,
    Netease,
    Kugou,
    Musixmatch,
    SodaMusic,
    AppleMusic,
    Spotify,
    LRCLIB,
}
