using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LocalMusicPlayer.Services.OnlineLyrics.Models;
using LocalMusicPlayer.Services.OnlineLyrics.QQMusic;

namespace LocalMusicPlayer.Services.OnlineLyrics.Searchers;

public class QQMusicSearchResult : ISearchResult
{
    public ISearcher Searcher => new QQMusicSearcher();

    public QQMusicSearchResult(string title, string[] artists, string album, string[]? albumArtists, int durationMs, string id, string mid)
    {
        Title = title;
        Artists = artists;
        Album = album;
        AlbumArtists = albumArtists;
        DurationMs = durationMs;
        Id = id;
        Mid = mid;
    }

    public QQMusicSearchResult(QQMusicSong song) : this(
        song.Title,
        song.Singer.Select(s => s.Name).ToArray(),
        song.Album.Title,
        null,
        song.Interval * 1000,
        song.Id,
        song.Mid
    )
    { }

    public QQMusicSearchResult(Song song) : this(
        song.Title,
        song.Singer.Select(s => s.Name).ToArray(),
        song.Album?.Title ?? string.Empty,
        null,
        song.Interval * 1000,
        song.Id,
        song.Mid
    )
    { }

    public string Title { get; }
    public string[] Artists { get; }
    public string Album { get; }
    public string Id { get; }
    public string Mid { get; }
    public string[]? AlbumArtists { get; }
    public int? DurationMs { get; }
    public CompareHelper.MatchType? MatchType { get; private set; }

    public void SetMatchType(CompareHelper.MatchType? matchType)
    {
        MatchType = matchType;
    }
}

public class QQMusicSearcher : Searcher
{
    public override string Name => "QQ Music";
    public override string DisplayName => "QQ Music";
    public override Searchers SearcherType => Searchers.QQMusic;

    private readonly Api _api = new();

    public override async Task<List<ISearchResult>?> SearchForResults(string searchString)
    {
        var search = new List<ISearchResult>();

        try
        {
            var result = await _api.Search(searchString, Api.SearchTypeEnum.SONG_ID);
            var results = result?.Req_1?.Data?.Body?.Song?.List;
            if (results == null) return null;
            foreach (var track in results)
            {
                search.Add(new QQMusicSearchResult(track));
                if (track.Group is { Count: > 0 } group)
                {
                    foreach (var subTrack in group)
                    {
                        search.Add(new QQMusicSearchResult(subTrack));
                    }
                }
            }
        }
        catch
        {
            return null;
        }

        return search;
    }
}
