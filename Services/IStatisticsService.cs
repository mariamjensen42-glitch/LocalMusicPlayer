using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LocalMusicPlayer.Models;

namespace LocalMusicPlayer.Services;

public interface IStatisticsService
{
    int TotalPlayCount { get; }
    TimeSpan TotalPlayTime { get; }
    int UsageDays { get; }

    LibraryStatistics GetLibraryStatistics();
    void RecordPlayStart(Song song);
    void RecordPlayEnd();
    IReadOnlyList<SongPlayRecord> GetTopPlayedSongs(int count);
    IReadOnlyList<SongPlayRecord> GetRecentlyPlayedSongs(int count);
    Task SaveStatisticsAsync();
    Task LoadStatisticsAsync();
    event EventHandler? StatisticsChanged;

    Task TrackPlayAsync(Song song, TimeSpan playedDuration);
    Task<StatisticsReport> GetStatisticsReportAsync();
    Task<IReadOnlyList<ListeningHistoryItem>> GetListeningHistoryAsync(DateTime? startDate, DateTime? endDate);
    Task<IReadOnlyList<ArtistStatistics>> GetTopArtistsAsync(int count);
    Task<IReadOnlyList<AlbumStatistics>> GetTopAlbumsAsync(int count);
    Task<IReadOnlyList<SongPlayRecord>> GetTopSongsAsync(int count);
    Task<IReadOnlyList<GenreDistribution>> GetGenreDistributionAsync();
}
