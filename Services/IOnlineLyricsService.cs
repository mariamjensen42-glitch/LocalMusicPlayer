using System.Collections.Generic;
using System.Threading.Tasks;
using LocalMusicPlayer.Models;

namespace LocalMusicPlayer.Services;

public interface IOnlineLyricsService
{
    Task<OnlineLyricResult?> SearchLyricsAsync(Song song);
}

public class OnlineLyricResult
{
    public List<LyricLine> Lyrics { get; set; } = new();
    public string? Source { get; set; }
    public string? Title { get; set; }
    public string? Artist { get; set; }
}
