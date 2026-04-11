using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using LocalMusicPlayer.ViewModels;

namespace LocalMusicPlayer.ViewModels;

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? param)
    {
        if (param is null)
            return null;

        return param switch
        {
            SettingsViewModel => new Views.Settings.SettingsView(),
            StatisticsViewModel => new Views.Statistics.StatisticsView(),
            LibraryBrowserViewModel => new Views.Library.LibraryBrowserView(),
            LibraryCategoryViewModel => new Views.Library.LibraryCategoryView(),
            ArtistDetailViewModel => new Views.Details.ArtistDetailView(),
            AlbumDetailViewModel => new Views.Details.AlbumDetailView(),
            PlayHistoryViewModel => new Views.Statistics.PlayHistoryView(),
            PlaylistManagementViewModel => new Views.Playlist.PlaylistManagementView(),
            PlaylistListViewModel => new Views.Playlist.PlaylistListView(),
            StatisticsReportViewModel => new Views.Statistics.StatisticsReportView(),
            PlayerPageViewModel => new Views.Player.PlayerPageView(),
            QueueViewModel => new Views.Playlist.QueueView(),
            MetadataEditorViewModel => new Views.Editors.MetadataEditorView(),
            BatchMetadataEditorViewModel => new Views.Editors.BatchMetadataEditorView(),
            HomeViewModel => new Views.Library.HomeView(),
            ArtistsPageViewModel => new Views.Library.ArtistsPageView(),
            AlbumsPageViewModel => new Views.Library.AlbumsPageView(),
            _ => CreateByConvention(param)
        };
    }

    private static Control? CreateByConvention(object data)
    {
        var name = data.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var type = assembly.GetType(name);
            if (type != null)
                return (Control)Activator.CreateInstance(type)!;
        }

        return new TextBlock { Text = "Not Found: " + name };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}
