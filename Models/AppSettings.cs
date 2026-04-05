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
    public string Theme { get; set; } = "Dark";
    public string PlaybackMode { get; set; } = "Normal";
}