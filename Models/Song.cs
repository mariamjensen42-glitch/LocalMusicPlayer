using ReactiveUI;
using System;

namespace LocalMusicPlayer.Models;

public class Song : ReactiveObject
{
    private string _title = string.Empty;

    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    private string _artist = string.Empty;

    public string Artist
    {
        get => _artist;
        set => this.RaiseAndSetIfChanged(ref _artist, value);
    }

    private string _album = string.Empty;

    public string Album
    {
        get => _album;
        set => this.RaiseAndSetIfChanged(ref _album, value);
    }

    private string _filePath = string.Empty;

    public string FilePath
    {
        get => _filePath;
        set => this.RaiseAndSetIfChanged(ref _filePath, value);
    }

    private TimeSpan _duration;

    public TimeSpan Duration
    {
        get => _duration;
        set => this.RaiseAndSetIfChanged(ref _duration, value);
    }

    private int _trackNumber;

    public int TrackNumber
    {
        get => _trackNumber;
        set => this.RaiseAndSetIfChanged(ref _trackNumber, value);
    }

    private string? _albumArtPath;

    public string? AlbumArtPath
    {
        get => _albumArtPath;
        set
        {
            this.RaiseAndSetIfChanged(ref _albumArtPath, value);
            this.RaisePropertyChanged(nameof(HasAlbumArt));
        }
    }

    public bool HasAlbumArt => !string.IsNullOrEmpty(_albumArtPath);

    private bool _isFavorite;

    public bool IsFavorite
    {
        get => _isFavorite;
        set => this.RaiseAndSetIfChanged(ref _isFavorite, value);
    }
}