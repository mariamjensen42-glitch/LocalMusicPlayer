using System;
using System.Collections.Generic;
using System.Linq;
using LocalMusicPlayer.Services.OnlineLyrics.Models;
using LocalMusicPlayer.Services.OnlineLyrics.Helpers;

namespace LocalMusicPlayer.Services.OnlineLyrics.Searchers;

public static partial class CompareHelper
{
    public static MatchType CompareTrack(ITrackMetadata track, ISearchResult searchResult)
    {
        return CompareTrack(TrackMultiArtistMetadata.GetTrackMultiArtistMetadata(track), searchResult);
    }

    public static MatchType CompareTrack(TrackMultiArtistMetadata track, ISearchResult searchResult)
    {
        var trackMatch = CompareName(track.Title, searchResult.Title);
        var artistMatch = CompareArtist(track.Artists, searchResult.Artists);
        var albumMatch = CompareName(track.Album, searchResult.Album);
        var albumArtistMatch = CompareArtist(track.AlbumArtists, searchResult.AlbumArtists);
        var durationMatch = CompareDuration(track.DurationMs, searchResult.DurationMs);

        var totalScore = 0.0;
        totalScore += trackMatch.GetMatchScore();
        totalScore += artistMatch.GetMatchScore();
        totalScore += albumMatch.GetMatchScore() * 0.4;
        totalScore += albumArtistMatch.GetMatchScore() * 0.2;
        totalScore += durationMatch.GetMatchScore();

        var nullCount = 0.0;
        const int fullScore = 30;
        nullCount += albumMatch is null ? 0.4 : 0;
        nullCount += albumArtistMatch is null ? 0.2 : 0;
        nullCount += durationMatch is null ? 1 : 0;
        totalScore = totalScore * fullScore / (fullScore - nullCount * 7);

        return totalScore switch
        {
            > 21 => MatchType.Perfect,
            > 19 => MatchType.VeryHigh,
            > 17 => MatchType.High,
            > 15 => MatchType.PrettyHigh,
            > 11 => MatchType.Medium,
            > 8 => MatchType.Low,
            > 3 => MatchType.VeryLow,
            _ => MatchType.NoMatch,
        };
    }

    public static NameMatchType? CompareName(string? name1, string? name2)
    {
        if (name1 == null || name2 == null) return null;

        name1 = name1.ToSC(true).ToLower().Trim();
        name2 = name2.ToSC(true).ToLower().Trim();

        if (name1 == name2) return NameMatchType.Perfect;

        name1 = name1
            .Replace('\u2019', '\'')
            .Replace('，', ',')
            .Replace("（", " (")
            .Replace("）", " )")
            .Replace('[', '(')
            .Replace(']', ')')
            .RemoveDuoSpaces();
        name2 = name2
            .Replace('\u2019', '\'')
            .Replace('，', ',')
            .Replace("（", " (")
            .Replace("）", " )")
            .Replace('[', '(')
            .Replace(']', ')')
            .RemoveDuoSpaces();
        name1 = name1.Replace("acoustic version", "acoustic");
        name2 = name2.Replace("acoustic version", "acoustic");

        if ((name1.Replace(" - ", " (").Trim() + ")").Remove(" ")
            == (name2.Replace(" - ", " (").Trim() + ")").Remove(" "))
            return NameMatchType.VeryHigh;

        static bool SpecialCompare(string str1, string str2, string special)
        {
            special = "(" + special;
            bool c1 = str1.Contains(special);
            bool c2 = str2.Contains(special);
            if (c1 && !c2 && str1[..str1.IndexOf(special)].Trim() == str2) return true;
            if (c2 && !c1 && str2[..str2.IndexOf(special)].Trim() == str1) return true;
            return false;
        }

        static bool SingleSpecialCompare(string str1, string str2, string special)
        {
            special = "(" + special;
            if (str1.Contains(special) && str2.Contains(special)
                && str1[..str1.IndexOf(special)].Trim() == str2[..str2.IndexOf(special)].Trim()) return true;
            return false;
        }

        static bool DuoSpecialCompare(string str1, string str2, string special1, string special2)
        {
            special1 = "(" + special1;
            special2 = "(" + special2;
            if (str1.Contains(special1) && str2.Contains(special2)
                && str1[..str1.IndexOf(special1)].Trim() == str2[..str2.IndexOf(special2)].Trim()) return true;
            if (str1.Contains(special2) && str2.Contains(special1)
                && str1[..str1.IndexOf(special2)].Trim() == str2[..str2.IndexOf(special1)].Trim()) return true;
            return false;
        }

        static bool BracketsCompare(string str1, string str2)
        {
            if (str1.Contains('(') && !str2.Contains('(')
                && str1[..str1.IndexOf('(')].Trim() == str2) return true;
            if (str2.Contains('(') && !str2.Contains('(')
                && str2[..str2.IndexOf('(')].Trim() == str1) return true;
            return false;
        }

        if (SpecialCompare(name1, name2, "deluxe")) return NameMatchType.VeryHigh;
        if (SpecialCompare(name1, name2, "explicit")) return NameMatchType.VeryHigh;
        if (SpecialCompare(name1, name2, "special edition")) return NameMatchType.VeryHigh;
        if (SpecialCompare(name1, name2, "bonus track")) return NameMatchType.VeryHigh;
        if (SpecialCompare(name1, name2, "feat")) return NameMatchType.VeryHigh;
        if (SpecialCompare(name1, name2, "with")) return NameMatchType.VeryHigh;

        if (DuoSpecialCompare(name1, name2, "feat", "explicit")) return NameMatchType.High;
        if (DuoSpecialCompare(name1, name2, "with", "explicit")) return NameMatchType.High;
        if (SingleSpecialCompare(name1, name2, "feat")) return NameMatchType.High;
        if (SingleSpecialCompare(name1, name2, "with")) return NameMatchType.High;

        if (BracketsCompare(name1, name2)) return NameMatchType.Medium;

        int count = 0;
        if (name1.Length == name2.Length)
        {
            for (int i = 0; i < name1.Length; i++)
                if (name1[i] == name2[i]) count++;

            if ((double)count / name1.Length >= 0.8 && name1.Length >= 4
                || (double)count / name1.Length >= 0.5 && name1.Length >= 2 && name1.Length <= 3)
                return NameMatchType.High;
        }

        if (StringHelper.ComputeTextSame(name1, name2, true) > 90) return NameMatchType.VeryHigh;
        if (StringHelper.ComputeTextSame(name1, name2, true) > 80) return NameMatchType.High;
        if (StringHelper.ComputeTextSame(name1, name2, true) > 68) return NameMatchType.Medium;
        if (StringHelper.ComputeTextSame(name1, name2, true) > 55) return NameMatchType.Low;

        return NameMatchType.NoMatch;
    }

    public static ArtistMatchType? CompareArtist(IEnumerable<string>? artist1, IEnumerable<string>? artist2)
    {
        if (artist1 == null || artist2 == null) return null;

        var list1 = artist1.ToList();
        var list2 = artist2.ToList();

        for (int i = 0; i < list1.Count; i++)
            list1[i] = list1[i].ToLower().ToSC(true);
        for (int i = 0; i < list2.Count; i++)
            list2[i] = list2[i].ToLower().ToSC(true);

        int count = 0;
        foreach (var art in list2)
            if (list1.Contains(art)) count++;

        if (count == list1.Count && list1.Count == list2.Count)
            return ArtistMatchType.Perfect;

        if (count + 1 >= list1.Count && list1.Count >= 2 || list1.Count > 6 && (double)count / list1.Count > 0.8)
            return ArtistMatchType.VeryHigh;

        if (count == 1 && list1.Count == 1 && list2.Count == 2)
            return ArtistMatchType.High;

        if (list1.Count > 5 && (list2[0].Contains("Various") || list2[0].Contains("群星")))
            return ArtistMatchType.VeryHigh;

        if (list1.Count > 7 && list2.Count > 7 && (double)count / list1.Count > 0.66)
            return ArtistMatchType.High;

        if (list1.Count == 1 && list2.Count > 1 && list1[0].StartsWith(list2[0]))
            return ArtistMatchType.High;

        if (list1.Count == 1 && list2.Count > 1 && list2[0].Length > 3 && list1[0].Contains(list2[0]))
            return ArtistMatchType.High;

        if (list1.Count == 1 && list2.Count > 1 && list2[0].Length > 1 && list1[0].Contains(list2[0]))
            return ArtistMatchType.Medium;

        if (count == 1 && list1.Count == 1 && list2.Count >= 3)
            return ArtistMatchType.Medium;

        if (count >= 2)
            return ArtistMatchType.Low;

        return ArtistMatchType.NoMatch;
    }

    public static DurationMatchType? CompareDuration(int? duration1, int? duration2)
    {
        if (duration1 == null || duration2 == null || duration1 == 0 || duration2 == 0) return null;

        return Math.Abs(duration1.Value - duration2.Value) switch
        {
            0 => DurationMatchType.Perfect,
            < 300 => DurationMatchType.VeryHigh,
            < 700 => DurationMatchType.High,
            < 1500 => DurationMatchType.Medium,
            < 3500 => DurationMatchType.Low,
            _ => DurationMatchType.NoMatch,
        };
    }

    public static int GetMatchScore(this NameMatchType matchType) => matchType switch
    {
        NameMatchType.Perfect => 7,
        NameMatchType.VeryHigh => 6,
        NameMatchType.High => 5,
        NameMatchType.Medium => 4,
        NameMatchType.Low => 2,
        NameMatchType.NoMatch => 0,
        _ => 0,
    };

    public static int GetMatchScore(this NameMatchType? matchType) => matchType switch
    {
        NameMatchType.Perfect => 7,
        NameMatchType.VeryHigh => 6,
        NameMatchType.High => 5,
        NameMatchType.Medium => 4,
        NameMatchType.Low => 2,
        NameMatchType.NoMatch => 0,
        _ => 0,
    };

    public static int GetMatchScore(this ArtistMatchType matchType) => matchType switch
    {
        ArtistMatchType.Perfect => 7,
        ArtistMatchType.VeryHigh => 6,
        ArtistMatchType.High => 5,
        ArtistMatchType.Medium => 4,
        ArtistMatchType.Low => 2,
        ArtistMatchType.NoMatch => 0,
        _ => 0,
    };

    public static int GetMatchScore(this ArtistMatchType? matchType) => matchType switch
    {
        ArtistMatchType.Perfect => 7,
        ArtistMatchType.VeryHigh => 6,
        ArtistMatchType.High => 5,
        ArtistMatchType.Medium => 4,
        ArtistMatchType.Low => 2,
        ArtistMatchType.NoMatch => 0,
        _ => 0,
    };

    public static int GetMatchScore(this DurationMatchType matchType) => matchType switch
    {
        DurationMatchType.Perfect => 7,
        DurationMatchType.VeryHigh => 6,
        DurationMatchType.High => 5,
        DurationMatchType.Medium => 4,
        DurationMatchType.Low => 2,
        DurationMatchType.NoMatch => 0,
        _ => 0,
    };

    public static int GetMatchScore(this DurationMatchType? matchType) => matchType switch
    {
        DurationMatchType.Perfect => 7,
        DurationMatchType.VeryHigh => 6,
        DurationMatchType.High => 5,
        DurationMatchType.Medium => 4,
        DurationMatchType.Low => 2,
        DurationMatchType.NoMatch => 0,
        _ => 0,
    };

    public enum MatchType
    {
        Perfect = 100,
        VeryHigh = 99,
        High = 95,
        PrettyHigh = 90,
        Medium = 70,
        Low = 30,
        VeryLow = 10,
        NoMatch = -1,
    }

    public enum NameMatchType
    {
        Perfect,
        VeryHigh,
        High,
        Medium,
        Low,
        NoMatch = -1,
    }

    public enum ArtistMatchType
    {
        Perfect,
        VeryHigh,
        High,
        Medium,
        Low,
        NoMatch = -1,
    }

    public enum DurationMatchType
    {
        Perfect,
        VeryHigh,
        High,
        Medium,
        Low,
        NoMatch = -1,
    }
}

public class MatchTypeComparer : IComparer<CompareHelper.MatchType>
{
    public int Compare(CompareHelper.MatchType x, CompareHelper.MatchType y)
    {
        return ((int)x).CompareTo((int)y);
    }
}
