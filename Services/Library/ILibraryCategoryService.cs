using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LocalMusicPlayer.Models;

namespace LocalMusicPlayer.Services;

public interface ILibraryCategoryService
{
    LibraryCategory CurrentCategory { get; set; }

    IReadOnlyList<ArtistGroup> GetArtistGroups();
    IReadOnlyList<AlbumGroup> GetAlbumGroups();
    IReadOnlyList<FolderGroup> GetFolderGroups();
    Task<IReadOnlyList<Song>> GetFavoriteSongsAsync();

    event EventHandler<LibraryCategory>? CategoryChanged;

    void RefreshCategories();
}