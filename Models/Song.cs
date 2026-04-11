using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LocalMusicPlayer.Models;

public partial class Song : ObservableObject
{
    [ObservableProperty] private string _title = string.Empty;

    [ObservableProperty] private string _artist = string.Empty;

    [ObservableProperty] private string _album = string.Empty;

    [ObservableProperty] private string _genre = string.Empty;

    [ObservableProperty] private string _filePath = string.Empty;

    [ObservableProperty] private TimeSpan _duration;

    [ObservableProperty] private int _trackNumber;

    [ObservableProperty] private int _year;

    [ObservableProperty] private string? _albumArtPath;

    public bool HasAlbumArt => !string.IsNullOrEmpty(AlbumArtPath);

    [ObservableProperty] private bool _isFavorite;

    [ObservableProperty] private int _playCount;

    [ObservableProperty] private DateTime? _lastPlayedTime;

    [ObservableProperty] private float _replayGainTrackGain;

    [ObservableProperty] private long _fileSizeBytes;

    [ObservableProperty] private DateTime _addedAt = DateTime.Now;
}
