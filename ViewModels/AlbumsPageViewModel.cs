using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

    public Action<AlbumGroup>? OnNavigateToDetail { get; set; }

    public AlbumsPageViewModel(ILibraryCategoryService categoryService)
    {
        _categoryService = categoryService;
        _ = LoadDataAsync();
    }

    [RelayCommand]
    private void SelectItem(AlbumGroup albumGroup)
    {
        OnNavigateToDetail?.Invoke(albumGroup);
    }

    private async Task LoadDataAsync()
    {
        var groups = await Task.Run(() => _categoryService.GetAlbumGroups());
        AlbumGroups.Clear();
        foreach (var group in groups)
            AlbumGroups.Add(group);
    }
}
