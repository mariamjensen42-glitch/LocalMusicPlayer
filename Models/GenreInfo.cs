using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LocalMusicPlayer.Models;

public partial class GenreInfo : ObservableObject
{
    [ObservableProperty] private string _name = string.Empty;

    [ObservableProperty] private int _songCount;

    [ObservableProperty] private int _artistCount;

    public List<Song> Songs { get; set; } = [];
}
