using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalMusicPlayer.Models;
using LocalMusicPlayer.Services;

namespace LocalMusicPlayer.ViewModels;

public partial class ArtistsPageViewModel : ViewModelBase
{
    private readonly ILibraryCategoryService _categoryService;

    public ObservableCollection<ArtistGroup> ArtistGroups { get; } = new();

    public Action<ArtistGroup>? OnNavigateToDetail { get; set; }

    public ArtistsPageViewModel(ILibraryCategoryService categoryService)
    {
        _categoryService = categoryService;
        _ = LoadDataAsync();
    }

    [RelayCommand]
    private void SelectItem(ArtistGroup artistGroup)
    {
        OnNavigateToDetail?.Invoke(artistGroup);
    }

    private async Task LoadDataAsync()
    {
        var groups = await Task.Run(() => _categoryService.GetArtistGroups());
        ArtistGroups.Clear();
        foreach (var group in groups)
            ArtistGroups.Add(group);
    }
}
