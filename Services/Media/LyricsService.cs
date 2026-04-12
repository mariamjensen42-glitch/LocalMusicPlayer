using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace LocalMusicPlayer.Services;

public class LyricsService : ILyricsService
{
    private static readonly Regex LrcRegex = new(@"\[(\d{2}):(\d{2})\.(\d{2,3})\](.*)", RegexOptions.Compiled);
    private const int TimeToleranceMs = 50;

    public List<LyricLine> GetLyrics(string filePath)
    {
        var lyrics = new List<LyricLine>();

        if (!System.IO.File.Exists(filePath))
            return lyrics;

        try
        {
            string? lrcContent = null;
            string? translatedLrc = null;

            using (var file = TagLib.File.Create(filePath))
            {
                if (!string.IsNullOrEmpty(file.Tag.Lyrics))
                {
                    lrcContent = file.Tag.Lyrics;
                }
            }

            var directory = Path.GetDirectoryName(filePath);
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
            var lrcPath = Path.Combine(directory!, $"{fileNameWithoutExt}.lrc");

            if (System.IO.File.Exists(lrcPath))
            {
                lrcContent = System.IO.File.ReadAllText(lrcPath);
            }

            var transFileName = $"{fileNameWithoutExt}_Translated.lrc";
            var transLrcPath = Path.Combine(directory!, transFileName);
            if (System.IO.File.Exists(transLrcPath))
            {
                translatedLrc = System.IO.File.ReadAllText(transLrcPath);
            }

            if (!string.IsNullOrEmpty(lrcContent))
            {
                lyrics = ParseLrc(lrcContent, translatedLrc);
            }
        }
        catch (Exception)
        {
        }

        return lyrics;
    }

    public int GetCurrentLyricIndex(List<LyricLine> lyrics, TimeSpan position)
    {
        if (lyrics == null || lyrics.Count == 0)
            return -1;

        for (int i = lyrics.Count - 1; i >= 0; i--)
        {
            if (lyrics[i].Timestamp <= position)
                return i;
        }

        return -1;
    }

    private List<LyricLine> ParseLrc(string lrcContent, string? translatedLrc = null)
    {
        var lyrics = new List<LyricLine>();

        if (string.IsNullOrWhiteSpace(lrcContent))
            return lyrics;

        ParseLrcToLines(lrcContent, (time, text) =>
        {
            var existingLine = lyrics.FirstOrDefault(l =>
                Math.Abs((l.Timestamp - time).TotalMilliseconds) <= TimeToleranceMs);

            if (existingLine != null)
            {
                existingLine.Text += "\n" + text;
            }
            else
            {
                lyrics.Add(new LyricLine
                {
                    Timestamp = time,
                    Text = text,
                    LineAnimateDuration = TimeSpan.FromSeconds(5)
                });
            }
        });

        if (!string.IsNullOrEmpty(translatedLrc))
        {
            ParseLrcToLines(translatedLrc, (time, transText) =>
            {
                var lyric = lyrics.FirstOrDefault(l =>
                    Math.Abs((l.Timestamp - time).TotalMilliseconds) <= TimeToleranceMs);
                if (lyric != null)
                {
                    lyric.Translation = transText;
                }
            });
        }

        var sortedLyrics = lyrics.OrderBy(l => l.Timestamp).ToList();
        for (int i = 0; i < sortedLyrics.Count; i++)
        {
            if (i < sortedLyrics.Count - 1)
            {
                sortedLyrics[i].LineAnimateDuration =
                    sortedLyrics[i + 1].Timestamp - sortedLyrics[i].Timestamp;
            }
        }

        return sortedLyrics;
    }

    private void ParseLrcToLines(string content, Action<TimeSpan, string> onLineParsed)
    {
        if (string.IsNullOrEmpty(content))
            return;

        var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

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
                var text = match.Groups[4].Value.Trim();

                if (!string.IsNullOrEmpty(text))
                {
                    onLineParsed(new TimeSpan(0, 0, minutes, seconds, milliseconds), text);
                }
            }
        }
    }
}
