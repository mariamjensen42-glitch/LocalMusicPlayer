using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using LocalMusicPlayer.ViewModels;

namespace LocalMusicPlayer.ViewModels;

public class ViewLocator : IDataTemplate
{
    private static readonly Dictionary<Type, Func<Control>> ViewMappings = new()
    {
        [typeof(SettingsViewModel)] = () => new Views.Settings.SettingsView(),
        [typeof(ArtistDetailViewModel)] = () => new Views.Details.ArtistDetailView(),
        [typeof(AlbumDetailViewModel)] = () => new Views.Details.AlbumDetailView(),
        [typeof(PlaylistManagementViewModel)] = () => new Views.Playlist.PlaylistManagementView(),
        [typeof(PlaylistListViewModel)] = () => new Views.Playlist.PlaylistListView(),
        [typeof(PlayerPageViewModel)] = () => new Views.Player.PlayerPageView(),
        [typeof(QueueViewModel)] = () => new Views.Playlist.QueueView(),
        [typeof(MetadataEditorViewModel)] = () => new Views.Editors.MetadataEditorView(),
        [typeof(MusicLibraryViewModel)] = () => new Views.Library.MusicLibraryView(),
    };

    public Control? Build(object? param)
    {
        if (param is null)
            return null;

        var paramType = param.GetType();

        if (ViewMappings.TryGetValue(paramType, out var createView))
        {
            return createView();
        }

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
