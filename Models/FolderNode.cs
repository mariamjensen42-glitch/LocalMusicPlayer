using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LocalMusicPlayer.Models;

public partial class FolderNode : ObservableObject
{
    [ObservableProperty] private string _name = string.Empty;

    [ObservableProperty] private string _fullPath = string.Empty;

    [ObservableProperty] private bool _isExpanded;

    [ObservableProperty] private int _songCount;

    public ObservableCollection<FolderNode> Children { get; set; } = [];

    public ObservableCollection<Song> Songs { get; set; } = [];

    public bool HasChildren => Children.Count > 0;

    public bool HasSongs => Songs.Count > 0;
}
