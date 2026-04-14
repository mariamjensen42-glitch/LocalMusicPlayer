using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LocalMusicPlayer.Models;

namespace LocalMusicPlayer.Services;

public class SmartPlaylistService : ISmartPlaylistService
{
    private readonly IMusicLibraryService _musicLibraryService;
    private readonly IStatisticsService _statisticsService;
    private readonly List<SmartPlaylist> _playlists = new();

    private readonly string _filePath;

    public SmartPlaylistService(
        IMusicLibraryService musicLibraryService,
        IStatisticsService statisticsService)
    {
        _musicLibraryService = musicLibraryService;
        _statisticsService = statisticsService;

        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appFolder = Path.Combine(appDataPath, "LocalMusicPlayer");
        Directory.CreateDirectory(appFolder);
        _filePath = Path.Combine(appFolder, "smart_playlists.json");

        LoadPlaylists();
    }

    private void LoadPlaylists()
    {
        _playlists.Clear();

        // Default playlists
        _playlists.Add(new SmartPlaylist { Name = "播放最多", Rule = SmartPlaylistRule.MostPlayed, Limit = 50 });
        _playlists.Add(new SmartPlaylist { Name = "最近播放", Rule = SmartPlaylistRule.RecentlyPlayed, Limit = 50 });
        _playlists.Add(new SmartPlaylist { Name = "从未播放", Rule = SmartPlaylistRule.NeverPlayed, Limit = 50 });
        _playlists.Add(new SmartPlaylist { Name = "最近添加", Rule = SmartPlaylistRule.RecentlyAdded, Limit = 50 });

        if (File.Exists(_filePath))
        {
            try
            {
                var json = File.ReadAllText(_filePath);
                var saved = JsonSerializer.Deserialize<List<SmartPlaylist>>(json);
                if (saved != null)
                {
                    _playlists.Clear();
                    _playlists.AddRange(saved);
                }
            }
            catch
            {
                // Use defaults
            }
        }
    }

    private void Persist()
    {
        var json = JsonSerializer.Serialize(_playlists, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_filePath, json);
    }

    public IReadOnlyList<SmartPlaylist> GetSmartPlaylists() => _playlists.AsReadOnly();

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

    public void SaveSmartPlaylist(SmartPlaylist playlist)
    {
        var existing = _playlists.FirstOrDefault(p => p.Name == playlist.Name);
        if (existing != null)
        {
            _playlists.Remove(existing);
        }
        _playlists.Add(playlist);
        Persist();
    }

    public void DeleteSmartPlaylist(string name)
    {
        var existing = _playlists.FirstOrDefault(p => p.Name == name);
        if (existing != null)
        {
            _playlists.Remove(existing);
            Persist();
        }
    }
}
