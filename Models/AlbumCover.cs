using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LocalMusicPlayer.Models;

public partial class AlbumCover : ObservableObject
{
    [ObservableProperty] private string _album = string.Empty;

    [ObservableProperty] private string _artist = string.Empty;

    [ObservableProperty] private string? _coverPath;

    [ObservableProperty] private DateTime _cachedAt;

    [ObservableProperty] private long _fileSizeBytes;

    [ObservableProperty] private int _width;

    [ObservableProperty] private int _height;

    [ObservableProperty] private string _mimeType = string.Empty;

    public bool HasCover => !string.IsNullOrEmpty(CoverPath);

    public string CacheKey => $"{Artist}_{Album}".Replace(" ", "_").Replace("/", "_");
}
