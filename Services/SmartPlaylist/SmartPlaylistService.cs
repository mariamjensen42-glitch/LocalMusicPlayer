using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LocalMusicPlayer.Models;

namespace LocalMusicPlayer.Services;

public class SmartPlaylistService : ISmartPlaylistService
{
    private readonly IMusicLibraryService _musicLibraryService;
    private readonly IStatisticsService _statisticsService;

    public SmartPlaylistService(
        IMusicLibraryService musicLibraryService,
        IStatisticsService statisticsService)
    {
        _musicLibraryService = musicLibraryService;
        _statisticsService = statisticsService;
    }

    public IReadOnlyList<SmartPlaylist> GetSmartPlaylists()
    {
        return new List<SmartPlaylist>
        {
            new() { Name = "播放最多", Rule = SmartPlaylistRule.MostPlayed, Limit = 50 },
            new() { Name = "最近播放", Rule = SmartPlaylistRule.RecentlyPlayed, Limit = 50 },
            new() { Name = "从未播放", Rule = SmartPlaylistRule.NeverPlayed, Limit = 50 },
            new() { Name = "最近添加", Rule = SmartPlaylistRule.RecentlyAdded, Limit = 50 },
        };
    }

    public Task<List<Song>> GetSongsForSmartPlaylistAsync(SmartPlaylist smartPlaylist)
    {
        var songs = _musicLibraryService.Songs.ToList();
        IEnumerable<Song> result = smartPlaylist.Rule switch
        {
            SmartPlaylistRule.MostPlayed =>
                songs.Where(s => s.PlayCount > 0)
                     .OrderByDescending(s => s.PlayCount)
                     .Take(smartPlaylist.Limit),

            SmartPlaylistRule.RecentlyPlayed =>
                songs.Where(s => s.LastPlayedTime.HasValue)
                     .OrderByDescending(s => s.LastPlayedTime)
                     .Take(smartPlaylist.Limit),

            SmartPlaylistRule.LeastPlayed =>
                songs.OrderBy(s => s.PlayCount)
                     .Take(smartPlaylist.Limit),

            SmartPlaylistRule.RecentlyAdded =>
                songs.OrderByDescending(s => s.AddedAt)
                     .Take(smartPlaylist.Limit),

            SmartPlaylistRule.NeverPlayed =>
                songs.Where(s => s.PlayCount == 0)
                     .Take(smartPlaylist.Limit),

            _ => songs
        };

        return Task.FromResult(result.ToList());
    }
}
