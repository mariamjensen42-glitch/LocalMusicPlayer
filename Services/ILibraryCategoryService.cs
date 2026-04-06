using System;
using System.Collections.Generic;
using LocalMusicPlayer.Models;

namespace LocalMusicPlayer.Services;

public interface ILibraryCategoryService
{
    LibraryCategory CurrentCategory { get; set; }

    // 分类数据获取
    IReadOnlyList<ArtistGroup> GetArtistGroups();
    IReadOnlyList<AlbumGroup> GetAlbumGroups();
    IReadOnlyList<FolderGroup> GetFolderGroups();
    IReadOnlyList<Song> GetFavoriteSongs();

    // 分类切换事件
    event EventHandler<LibraryCategory>? CategoryChanged;

    // 数据刷新
    void RefreshCategories();
}