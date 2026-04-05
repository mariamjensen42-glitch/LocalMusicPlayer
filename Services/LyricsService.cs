using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace LocalMusicPlayer.Services;

public class LyricsService : ILyricsService
{
    private static readonly Regex LrcRegex = new(@"\[(\d{2}):(\d{2})\.(\d{2,3})\](.*)", RegexOptions.Compiled);

    public List<LyricLine> GetLyrics(string filePath)
    {
        var lyrics = new List<LyricLine>();

        if (!System.IO.File.Exists(filePath))
            return lyrics;

        try
        {
            // 首先尝试从 TagLib 读取内嵌歌词
            using (var file = TagLib.File.Create(filePath))
            {
                if (!string.IsNullOrEmpty(file.Tag.Lyrics))
                {
                    lyrics = ParseLrc(file.Tag.Lyrics);
                    if (lyrics.Count > 0)
                        return lyrics;
                }
            }

            // 尝试查找同目录下的 .lrc 文件
            var directory = Path.GetDirectoryName(filePath);
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
            var lrcPath = Path.Combine(directory!, $"{fileNameWithoutExt}.lrc");

            if (System.IO.File.Exists(lrcPath))
            {
                var lrcContent = System.IO.File.ReadAllText(lrcPath);
                lyrics = ParseLrc(lrcContent);
            }
        }
        catch (Exception)
        {
            // 忽略错误，返回空列表
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
                var text = match.Groups[4].Value.Trim();

                if (!string.IsNullOrEmpty(text))
                {
                    lyrics.Add(new LyricLine
                    {
                        Timestamp = new TimeSpan(0, 0, minutes, seconds, milliseconds),
                        Text = text
                    });
                }
            }
        }

        return lyrics.OrderBy(l => l.Timestamp).ToList();
    }
}
