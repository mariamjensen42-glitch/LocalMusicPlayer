using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LocalMusicPlayer.Models;

namespace LocalMusicPlayer.Services;

public class LibraryCategoryService : ILibraryCategoryService
{
    private readonly IMusicLibraryService _libraryService;
    private readonly IUserPlaylistService _playlistService;

    private LibraryCategory _currentCategory = LibraryCategory.Songs;
    private List<ArtistGroup> _artistGroups = new();
    private List<AlbumGroup> _albumGroups = new();
    private List<FolderGroup> _folderGroups = new();

    public LibraryCategory CurrentCategory
    {
        get => _currentCategory;
        set
        {
            if (_currentCategory != value)
            {
                _currentCategory = value;
                CategoryChanged?.Invoke(this, _currentCategory);
            }
        }
    }

    public event EventHandler<LibraryCategory>? CategoryChanged;

    public LibraryCategoryService(IMusicLibraryService libraryService, IUserPlaylistService playlistService)
    {
        _libraryService = libraryService;
        _playlistService = playlistService;

        // 监听歌曲库变更
        _libraryService.Songs.CollectionChanged += (_, _) => RefreshCategories();
    }

    public IReadOnlyList<ArtistGroup> GetArtistGroups()
    {
        if (_artistGroups.Count == 0)
            RefreshCategories();
        return _artistGroups;
    }

    public IReadOnlyList<AlbumGroup> GetAlbumGroups()
    {
        if (_albumGroups.Count == 0)
            RefreshCategories();
        return _albumGroups;
    }

    public IReadOnlyList<FolderGroup> GetFolderGroups()
    {
        if (_folderGroups.Count == 0)
            RefreshCategories();
        return _folderGroups;
    }

    public IReadOnlyList<Song> GetFavoriteSongs()
    {
        return _playlistService.GetFavoriteSongs();
    }

    public void RefreshCategories()
    {
        var songs = _libraryService.Songs;

        // 按艺术家分组
        _artistGroups = songs
            .GroupBy(s => s.Artist)
            .Where(g => !string.IsNullOrEmpty(g.Key))
            .Select(g => new ArtistGroup
            {
                ArtistName = g.Key,
                Songs = new System.Collections.ObjectModel.ObservableCollection<Song>(g.OrderBy(s => s.Title))
            })
            .OrderBy(g => g.ArtistName)
            .ToList();

        // 按专辑分组
        _albumGroups = songs
            .GroupBy(s => new { s.Album, s.Artist })
            .Where(g => !string.IsNullOrEmpty(g.Key.Album))
            .Select(g => new AlbumGroup
            {
                AlbumName = g.Key.Album,
                ArtistName = g.Key.Artist,
                Songs = new System.Collections.ObjectModel.ObservableCollection<Song>(g.OrderBy(s => s.TrackNumber)
                    .ThenBy(s => s.Title))
            })
            .OrderBy(g => g.ArtistName)
            .ThenBy(g => g.AlbumName)
            .ToList();

        // 按文件夹分组
        _folderGroups = songs
            .GroupBy(s => Path.GetDirectoryName(s.FilePath) ?? "Unknown")
            .Select(g => new FolderGroup
            {
                FolderPath = g.Key,
                Songs = new System.Collections.ObjectModel.ObservableCollection<Song>(g.OrderBy(s => s.Title))
            })
            .OrderBy(g => g.FolderName)
            .ToList();

        CategoryChanged?.Invoke(this, _currentCategory);
    }
}