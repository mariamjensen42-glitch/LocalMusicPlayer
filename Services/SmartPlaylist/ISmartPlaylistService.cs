using System.Collections.Generic;
using System.Threading.Tasks;
using LocalMusicPlayer.Models;

namespace LocalMusicPlayer.Services;

public interface ISmartPlaylistService
{
    IReadOnlyList<SmartPlaylist> GetSmartPlaylists();
    Task<List<Song>> GetSongsForSmartPlaylistAsync(SmartPlaylist smartPlaylist);
}
