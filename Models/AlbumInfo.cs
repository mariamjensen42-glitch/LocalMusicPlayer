using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LocalMusicPlayer.Models;

public partial class AlbumInfo : ObservableObject
{
    [ObservableProperty] private string _title = string.Empty;

    [ObservableProperty] private string _artist = string.Empty;

    [ObservableProperty] private int _songCount;

    [ObservableProperty] private int _year;

    [ObservableProperty] private string? _coverPath;

    public List<Song> Songs { get; set; } = [];
}
