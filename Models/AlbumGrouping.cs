using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LocalMusicPlayer.Models;

public partial class AlbumGrouping : ObservableObject
{
    [ObservableProperty]
    private string _groupKey = "";

    public ObservableCollection<AlbumGroup> Albums { get; set; } = new();
}
