using System;
using System.Collections.Generic;

namespace LocalMusicPlayer.Models;

public class AppSettings
{
    public List<string> ScanFolders { get; set; } = new();
    public bool IncludeSubfolders { get; set; } = true;
    public int Volume { get; set; } = 80;
    public bool IsMuted { get; set; }
    public DateTime? LastScanTime { get; set; }
    public DateTime? FirstScanDate { get; set; }
    public string Theme { get; set; } = "Dark";
    public string PlaybackMode { get; set; } = "Normal";
    public float PlaybackRate { get; set; } = 1.0f;
    public double LyricFontSize { get; set; } = 28;
    public double LyricLineSpacing { get; set; } = 20;
    public bool ShowTranslation { get; set; } = true;

    public bool CrossfadeEnabled { get; set; }
    public int CrossfadeDurationMs { get; set; } = 3000;
    public bool ReplayGainEnabled { get; set; } = true;
    public bool MinimizeToTray { get; set; } = true;
    public bool ShowSongChangeNotification { get; set; } = true;
    public bool AutoStartOnBoot { get; set; }
    public bool ResumeLastPlayback { get; set; } = true;
    public bool DownloadAlbumArtwork { get; set; } = true;
    public bool AutoDetectMetadata { get; set; }
    public string AudioQuality { get; set; } = "Standard";
    public bool IsEqualizerEnabled { get; set; }

    public List<UserPlaylist> UserPlaylists { get; set; } = new();
    public List<string> FavoriteFilePaths { get; set; } = new();
    public string? LastSongFilePath { get; set; }
    public List<string> QueueFilePaths { get; set; } = new();
    public double LastPlaybackPosition { get; set; }
}
