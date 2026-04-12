using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LocalMusicPlayer.Models;
using LocalMusicPlayer.Services.OnlineLyrics.Models;
using LocalMusicPlayer.Services.OnlineLyrics.QQMusic;
using LocalMusicPlayer.Services.OnlineLyrics.Searchers;
using Microsoft.Extensions.Logging;

namespace LocalMusicPlayer.Services;

public class OnlineLyricsService : IOnlineLyricsService
{
    private static readonly Regex LrcRegex = new(@"\[(\d{2}):(\d{2})\.(\d{2,3})\](.*)", RegexOptions.Compiled);

    private readonly QQMusicSearcher _searcher = new();
    private readonly Services.OnlineLyrics.QQMusic.Api _api = new();
    private readonly ILogger<OnlineLyricsService> _logger;

    public OnlineLyricsService(ILogger<OnlineLyricsService> logger)
    {
        _logger = logger;
    }

    public async Task<OnlineLyricResult?> SearchLyricsAsync(Models.Song song)
    {
        try
        {
            var trackMetadata = new TrackMetadata
            {
                Title = song.Title,
                Artist = song.Artist,
                Album = song.Album,
                DurationMs = (int)song.Duration.TotalMilliseconds
            };

            _logger.LogInformation("搜索歌词: {Title} - {Artist}", trackMetadata.Title, trackMetadata.Artist);

            var searchResult = await _searcher.SearchForResult(trackMetadata);
            if (searchResult == null)
            {
                _logger.LogWarning("未找到搜索结果");
                return null;
            }

            var qqResult = searchResult as QQMusicSearchResult;
            if (qqResult == null)
            {
                _logger.LogWarning("搜索结果类型不匹配");
                return null;
            }

            _logger.LogInformation("找到歌曲: {Title}, Mid: {Mid}", qqResult.Title, qqResult.Mid);

            var lyricResult = await _api.GetLyric(qqResult.Mid);
            if (lyricResult == null || string.IsNullOrEmpty(lyricResult.Lyric))
            {
                _logger.LogWarning("未找到歌词或歌词为空, Mid: {Mid}", qqResult.Mid);
                return null;
            }

            _logger.LogInformation("获取到歌词内容, 长度: {Length}", lyricResult.Lyric.Length);

            var lyrics = ParseLrc(lyricResult.Lyric);

            return new OnlineLyricResult
            {
                Lyrics = lyrics,
                Source = "QQ Music",
                Title = searchResult.Title,
                Artist = searchResult.Artist
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "搜索歌词时发生异常");
            return null;
        }
    }

    private List<LyricLine> ParseLrc(string lrcContent)
    {
        var lyrics = new List<LyricLine>();

        if (string.IsNullOrWhiteSpace(lrcContent))
            return lyrics;

        var lines = lrcContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var match = LrcRegex.Match(line);
            if (match.Success)
            {
                var minutes = int.Parse(match.Groups[1].Value);
                var seconds = int.Parse(match.Groups[2].Value);
                var milliseconds = match.Groups[3].Value.Length == 2
                    ? int.Parse(match.Groups[3].Value) * 10
                    : int.Parse(match.Groups[3].Value);
                var content = match.Groups[4].Value.Trim();

                if (!string.IsNullOrEmpty(content))
                {
                    var parts = content.Split(new[] { "\\\\n", "\\n" }, StringSplitOptions.RemoveEmptyEntries);
                    var text = parts[0].Trim();
                    var translation = parts.Length > 1 ? parts[1].Trim() : null;

                    if (!string.IsNullOrEmpty(text))
                    {
                        lyrics.Add(new LyricLine
                        {
                            Timestamp = new TimeSpan(0, 0, minutes, seconds, milliseconds),
                            Text = text,
                            Translation = translation
                        });
                    }
                }
            }
        }

        return lyrics.OrderBy(l => l.Timestamp).ToList();
    }
}
