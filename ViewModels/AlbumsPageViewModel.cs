using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalMusicPlayer.Models;
using LocalMusicPlayer.Services;

namespace LocalMusicPlayer.ViewModels;

public partial class AlbumsPageViewModel : ViewModelBase
{
    private readonly ILibraryCategoryService _categoryService;

    public ObservableCollection<AlbumGroup> AlbumGroups { get; } = new();
    public ObservableCollection<AlbumGrouping> GroupedAlbums { get; } = new();

    [ObservableProperty]
    private AlbumGroup? _selectedItem;

    public Action<AlbumGroup>? OnNavigateToDetail { get; set; }

    public AlbumsPageViewModel(ILibraryCategoryService categoryService)
    {
        _categoryService = categoryService;
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        var groups = await Task.Run(() => _categoryService.GetAlbumGroups());
        AlbumGroups.Clear();
        foreach (var group in groups)
            AlbumGroups.Add(group);

        BuildGroupedAlbums();
    }

    private void BuildGroupedAlbums()
    {
        GroupedAlbums.Clear();
        var grouped = AlbumGroups
            .OrderBy(a => a.AlbumName)
            .GroupBy(a => GetGroupKey(a.AlbumName))
            .OrderBy(g => g.Key, StringComparer.Ordinal);

        foreach (var group in grouped)
        {
            GroupedAlbums.Add(new AlbumGrouping
            {
                GroupKey = group.Key,
                Albums = new ObservableCollection<AlbumGroup>(group.OrderBy(a => a.AlbumName))
            });
        }
    }

    private static string GetGroupKey(string name)
    {
        if (string.IsNullOrEmpty(name)) return "#";
        var c = char.ToUpperInvariant(name[0]);
        return c >= 'A' && c <= 'Z' ? c.ToString() : "#";
    }

    [RelayCommand]
    private void SelectItem(AlbumGroup albumGroup)
    {
        OnNavigateToDetail?.Invoke(albumGroup);
    }

    [RelayCommand]
    private void Play()
    {
        if (SelectedItem == null) return;
        OnNavigateToDetail?.Invoke(SelectedItem);
    }

    [RelayCommand]
    private void AddToFavourite()
    {
    }

    [RelayCommand]
    private void ShowPropertyWindow()
    {
    }
}
