using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using ReactiveUI;
using LocalMusicPlayer.Models;
using LocalMusicPlayer.Services;

namespace LocalMusicPlayer.ViewModels;

public class LibraryCategoryViewModel : ViewModelBase
{
    private readonly ILibraryCategoryService _categoryService;
    private readonly IMusicPlayerService _musicPlayerService;
    private readonly IPlaylistService _playlistService;
    private readonly IStatisticsService _statisticsService;

    private LibraryCategory _currentCategory = LibraryCategory.Songs;
    private object? _selectedItem;

    public LibraryCategory CurrentCategory
    {
        get => _currentCategory;
        set
        {
            this.RaiseAndSetIfChanged(ref _currentCategory, value);
            this.RaisePropertyChanged(nameof(ShowArtistGroups));
            this.RaisePropertyChanged(nameof(ShowAlbumGroups));
            this.RaisePropertyChanged(nameof(ShowFolderGroups));
            this.RaisePropertyChanged(nameof(ShowFavorites));
            RefreshItems();
        }
    }

    public bool ShowArtistGroups => CurrentCategory == LibraryCategory.Artists;
    public bool ShowAlbumGroups => CurrentCategory == LibraryCategory.Albums;
    public bool ShowFolderGroups => CurrentCategory == LibraryCategory.Folders;
    public bool ShowFavorites => CurrentCategory == LibraryCategory.Favorites;

    public ObservableCollection<ArtistGroup> ArtistGroups { get; } = new();
    public ObservableCollection<AlbumGroup> AlbumGroups { get; } = new();
    public ObservableCollection<FolderGroup> FolderGroups { get; } = new();
    public ObservableCollection<Song> FavoriteSongs { get; } = new();
    public ObservableCollection<Song> SelectedGroupSongs { get; } = new();

    public object? SelectedItem
    {
        get => _selectedItem;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedItem, value);
            UpdateSelectedGroupSongs();
        }
    }

    public ReactiveCommand<LibraryCategory, Unit> SwitchCategoryCommand { get; }
    public ReactiveCommand<string, Unit> PlaySongCommand { get; }
    public ReactiveCommand<Unit, Unit> PlayAllSelectedCommand { get; }

    public LibraryCategoryViewModel(
        ILibraryCategoryService categoryService,
        IMusicPlayerService musicPlayerService,
        IPlaylistService playlistService,
        IStatisticsService statisticsService)
    {
        _categoryService = categoryService;
        _musicPlayerService = musicPlayerService;
        _playlistService = playlistService;
        _statisticsService = statisticsService;

        SwitchCategoryCommand = ReactiveCommand.Create<LibraryCategory>(category =>
        {
            CurrentCategory = category;
            _categoryService.CurrentCategory = category;
        });

        PlaySongCommand = ReactiveCommand.Create<string>(path =>
        {
            var song = SelectedGroupSongs.FirstOrDefault(s => s.FilePath == path);
            if (song != null)
            {
                PlaySong(song);
            }
        });

        PlayAllSelectedCommand = ReactiveCommand.Create(() =>
        {
            if (SelectedGroupSongs.Count == 0) return;

            var playlist = _playlistService.CreatePlaylist("临时播放");
            _playlistService.SetCurrentPlaylist(playlist);

            foreach (var song in SelectedGroupSongs)
            {
                _playlistService.AddSongToPlaylist(playlist, song);
            }

            _playlistService.PlayNext();
            if (_playlistService.CurrentSong != null)
            {
                PlaySong(_playlistService.CurrentSong);
            }
        });

        // 监听分类变更
        _categoryService.CategoryChanged += (_, category) =>
        {
            if (CurrentCategory != category)
            {
                CurrentCategory = category;
            }
        };

        // 初始加载数据
        RefreshItems();
    }

    private void RefreshItems()
    {
        ArtistGroups.Clear();
        AlbumGroups.Clear();
        FolderGroups.Clear();
        FavoriteSongs.Clear();
        SelectedGroupSongs.Clear();

        switch (CurrentCategory)
        {
            case LibraryCategory.Artists:
                foreach (var group in _categoryService.GetArtistGroups())
                    ArtistGroups.Add(group);
                break;
            case LibraryCategory.Albums:
                foreach (var group in _categoryService.GetAlbumGroups())
                    AlbumGroups.Add(group);
                break;
            case LibraryCategory.Folders:
                foreach (var group in _categoryService.GetFolderGroups())
                    FolderGroups.Add(group);
                break;
            case LibraryCategory.Favorites:
                foreach (var song in _categoryService.GetFavoriteSongs())
                    FavoriteSongs.Add(song);
                break;
        }
    }

    private void UpdateSelectedGroupSongs()
    {
        SelectedGroupSongs.Clear();

        switch (CurrentCategory)
        {
            case LibraryCategory.Artists:
                if (SelectedItem is ArtistGroup artistGroup)
                    foreach (var song in artistGroup.Songs)
                        SelectedGroupSongs.Add(song);
                break;
            case LibraryCategory.Albums:
                if (SelectedItem is AlbumGroup albumGroup)
                    foreach (var song in albumGroup.Songs)
                        SelectedGroupSongs.Add(song);
                break;
            case LibraryCategory.Folders:
                if (SelectedItem is FolderGroup folderGroup)
                    foreach (var song in folderGroup.Songs)
                        SelectedGroupSongs.Add(song);
                break;
            case LibraryCategory.Favorites:
                foreach (var song in FavoriteSongs)
                    SelectedGroupSongs.Add(song);
                break;
        }
    }

    private void PlaySong(Song song)
    {
        _statisticsService.RecordPlayStart(song);
        _musicPlayerService.Play(song);
    }
}