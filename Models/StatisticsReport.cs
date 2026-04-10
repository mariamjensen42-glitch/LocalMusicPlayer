using System;
using System.Collections.Generic;

namespace LocalMusicPlayer.Models;

public class StatisticsReport
{
    public DateTime GeneratedAt { get; set; }
    public OverviewStatistics Overview { get; set; } = new();
    public List<SongPlayRecord> TopSongs { get; set; } = new();
    public List<ArtistStatistics> TopArtists { get; set; } = new();
    public List<AlbumStatistics> TopAlbums { get; set; } = new();
    public List<GenreDistribution> GenreDistribution { get; set; } = new();
    public List<ListeningTrend> ListeningTrends { get; set; } = new();
}

public class OverviewStatistics
{
    public int TotalSongs { get; set; }
    public int TotalPlayCount { get; set; }
    public TimeSpan TotalPlayTime { get; set; }
    public int UniqueArtists { get; set; }
    public int UniqueAlbums { get; set; }
    public DateTime? FirstListenDate { get; set; }
    public int ListeningDays { get; set; }
    public double AverageDailyPlayTime { get; set; }
}

public class ArtistStatistics
{
    public int Rank { get; set; }
    public string ArtistName { get; set; } = string.Empty;
    public int PlayCount { get; set; }
    public TimeSpan TotalPlayTime { get; set; }
    public int SongCount { get; set; }
    public DateTime? LastPlayedTime { get; set; }
}

public class AlbumStatistics
{
    public int Rank { get; set; }
    public string AlbumName { get; set; } = string.Empty;
    public string ArtistName { get; set; } = string.Empty;
    public int PlayCount { get; set; }
    public TimeSpan TotalPlayTime { get; set; }
    public int SongCount { get; set; }
    public DateTime? LastPlayedTime { get; set; }
}

public class GenreDistribution
{
    public string Genre { get; set; } = string.Empty;
    public int SongCount { get; set; }
    public int PlayCount { get; set; }
    public TimeSpan TotalPlayTime { get; set; }
    public double Percentage { get; set; }
}

public class ListeningTrend
{
    public DateTime Date { get; set; }
    public int PlayCount { get; set; }
    public TimeSpan TotalPlayTime { get; set; }
    public int UniqueSongs { get; set; }
}
