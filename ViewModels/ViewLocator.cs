using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using LocalMusicPlayer.ViewModels;

namespace LocalMusicPlayer.ViewModels;

public class ViewLocator : IDataTemplate
{
    // ViewModel 类型到 View 类型的显式映射
    private static readonly Dictionary<Type, Func<Control>> ViewMappings = new()
    {
        [typeof(SettingsViewModel)] = () => new Views.Settings.SettingsView(),
        [typeof(StatisticsViewModel)] = () => new Views.Statistics.StatisticsView(),
        [typeof(LibraryBrowserViewModel)] = () => new Views.Library.LibraryBrowserView(),
        [typeof(LibraryCategoryViewModel)] = () => new Views.Library.LibraryCategoryView(),
        [typeof(ArtistDetailViewModel)] = () => new Views.Details.ArtistDetailView(),
        [typeof(AlbumDetailViewModel)] = () => new Views.Details.AlbumDetailView(),
        [typeof(PlayHistoryViewModel)] = () => new Views.Statistics.PlayHistoryView(),
        [typeof(PlaylistManagementViewModel)] = () => new Views.Playlist.PlaylistManagementView(),
        [typeof(PlaylistListViewModel)] = () => new Views.Playlist.PlaylistListView(),
        [typeof(StatisticsReportViewModel)] = () => new Views.Statistics.StatisticsReportView(),
        [typeof(PlayerPageViewModel)] = () => new Views.Player.PlayerPageView(),
        [typeof(QueueViewModel)] = () => new Views.Playlist.QueueView(),
        [typeof(MetadataEditorViewModel)] = () => new Views.Editors.MetadataEditorView(),
        [typeof(BatchMetadataEditorViewModel)] = () => new Views.Editors.BatchMetadataEditorView(),
        [typeof(HomeViewModel)] = () => new Views.Library.HomeView(),
        [typeof(ArtistsPageViewModel)] = () => new Views.Library.ArtistsPageView(),
        [typeof(AlbumsPageViewModel)] = () => new Views.Library.AlbumsPageView(),
        [typeof(MusicBrowseViewModel)] = () => new Views.Library.MusicBrowseView(),
        [typeof(FolderBrowseViewModel)] = () => new Views.Library.FolderBrowseView(),
        [typeof(SongListViewModel)] = () => new Views.Library.SongListView(),
    };

    public Control? Build(object? param)
    {
        if (param is null)
            return null;

        var paramType = param.GetType();

        // 首先检查显式映射
        if (ViewMappings.TryGetValue(paramType, out var createView))
        {
            return createView();
        }

        // 使用约定创建视图
        return CreateByConvention(paramType);
    }

    private static Control CreateByConvention(Type viewModelType)
    {
        var viewTypeName = viewModelType.FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var viewType = assembly.GetType(viewTypeName);
            if (viewType != null)
            {
                return (Control)Activator.CreateInstance(viewType)!;
            }
        }

        return new TextBlock { Text = $"Not Found: {viewTypeName}" };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}
