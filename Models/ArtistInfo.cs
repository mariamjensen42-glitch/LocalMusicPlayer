using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LocalMusicPlayer.Models;

public partial class ArtistInfo : ObservableObject
{
    [ObservableProperty] private string _name = string.Empty;

    [ObservableProperty] private int _songCount;

    [ObservableProperty] private int _albumCount;

    [ObservableProperty] private string? _coverPath;

    public List<Song> Songs { get; set; } = [];
}
