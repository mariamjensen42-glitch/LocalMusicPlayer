using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using LocalMusicPlayer.Models;
using LocalMusicPlayer.ViewModels;
using LocalMusicPlayer.Views.Editors;
using Microsoft.Extensions.DependencyInjection;

namespace LocalMusicPlayer.Services;

public class DialogService : IDialogService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IViewModelFactory _viewModelFactory;
    private readonly IUserPlaylistService _userPlaylistService;

    public DialogService(IServiceProvider serviceProvider, IViewModelFactory viewModelFactory, IUserPlaylistService userPlaylistService)
    {
        _serviceProvider = serviceProvider;
        _viewModelFactory = viewModelFactory;
        _userPlaylistService = userPlaylistService;
    }

    public async Task<string?> ShowInputDialogAsync(string title, string defaultValue = "")
    {
        var mainWindow = GetMainWindow();
        if (mainWindow == null) return null;

        var dialog = new Window
        {
            Title = title,
            Width = 400,
            Height = 180,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            Background = new SolidColorBrush(Color.Parse("#1E1E2E")),
            BorderBrush = new SolidColorBrush(Color.Parse("#3B3B5C")),
            BorderThickness = new Thickness(1)
        };

        var result = defaultValue;
        var isConfirmed = false;

        var panel = new StackPanel { Margin = new Thickness(24), Spacing = 16 };

        var textBox = new TextBox
        {
            Text = defaultValue,
            Watermark = "Enter name..."
        };

        var buttonPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            Spacing = 12
        };

        var cancelButton = new Button { Content = "Cancel", MinWidth = 80 };
        var confirmButton = new Button { Content = "OK", MinWidth = 80, IsDefault = true };

        cancelButton.Click += (_, _) =>
        {
            isConfirmed = false;
            dialog.Close();
        };
        confirmButton.Click += (_, _) =>
        {
            isConfirmed = true;
            result = textBox.Text ?? string.Empty;
            dialog.Close();
        };

        buttonPanel.Children.Add(cancelButton);
        buttonPanel.Children.Add(confirmButton);

        panel.Children.Add(textBox);
        panel.Children.Add(buttonPanel);

        dialog.Content = panel;

        await dialog.ShowDialog(mainWindow);

        return isConfirmed ? result : null;
    }

    public async Task ShowMessageDialogAsync(string title, string message)
    {
        var mainWindow = GetMainWindow();
        if (mainWindow == null) return;

        var dialog = new Window
        {
            Title = title,
            Width = 360,
            Height = 160,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            Background = new SolidColorBrush(Color.Parse("#1E1E2E")),
            BorderBrush = new SolidColorBrush(Color.Parse("#3B3B5C")),
            BorderThickness = new Thickness(1)
        };

        var panel = new StackPanel { Margin = new Thickness(24), Spacing = 16 };

        panel.Children.Add(new TextBlock { Text = message, TextWrapping = Avalonia.Media.TextWrapping.Wrap });

        var okButton = new Button
        {
            Content = "OK",
            MinWidth = 80,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        };
        okButton.Click += (_, _) => dialog.Close();

        panel.Children.Add(okButton);

        dialog.Content = panel;

        await dialog.ShowDialog(mainWindow);
    }

    public async Task<string?> ShowOpenFileDialogAsync(string title, string[]? filters = null)
    {
        var mainWindow = GetMainWindow();
        if (mainWindow == null) return null;

#pragma warning disable CS0618
        var dialog = new OpenFileDialog
        {
            Title = title,
            AllowMultiple = false,
            Filters = filters?.Select(f => new FileDialogFilter
                          { Name = f, Extensions = f.Split(',').Select(e => e.Trim()).ToList() }).ToList() ??
                      new List<FileDialogFilter>()
        };

        var result = await dialog.ShowAsync(mainWindow);
        return result?.FirstOrDefault();
#pragma warning restore CS0618
    }

    public async Task<string?> ShowSaveFileDialogAsync(string title, string[]? filters = null)
    {
        var mainWindow = GetMainWindow();
        if (mainWindow == null) return null;

        var fileTypeChoices = filters?.Select(f => new FilePickerFileType(f)
        {
            Patterns = f.Split(',').Select(e => $"*{e.Trim()}").ToList()
        }).ToList();

        var result = await mainWindow.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = title,
            FileTypeChoices = fileTypeChoices
        });

        return result?.Path.LocalPath;
    }

    public async Task ShowMetadataEditorDialogAsync(Song song, Action? onSaved = null)
    {
        var mainWindow = GetMainWindow();
        if (mainWindow == null) return;

        var viewModel = _viewModelFactory.CreateMetadataEditorViewModel(song, onSaved);
        var dialog = new MetadataEditorView
        {
            DataContext = viewModel
        };
        await dialog.ShowDialog(mainWindow);
    }

    public async Task<IReadOnlyList<string>?> ShowFolderPickerAsync()
    {
        var mainWindow = GetMainWindow();
        if (mainWindow == null) return null;

        var folders = await mainWindow.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select Music Folder",
            AllowMultiple = true
        });

        return folders.Select(f => f.Path.LocalPath).ToList();
    }

    public async Task ShowAddToPlaylistDialogAsync(Song song)
    {
        var mainWindow = GetMainWindow();
        if (mainWindow == null) return;

        var playlists = _userPlaylistService.UserPlaylists
            .Where(p => p.Id != "favorites")
            .ToList();

        if (playlists.Count == 0)
        {
            await ShowMessageDialogAsync("Add to Playlist", "No playlists available. Please create a playlist first.");
            return;
        }

        var dialog = new Window
        {
            Title = "Add to Playlist",
            Width = 320,
            Height = 440,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            Background = new SolidColorBrush(Color.Parse("#1E1E2E")),
            BorderBrush = new SolidColorBrush(Color.Parse("#3B3B5C")),
            BorderThickness = new Thickness(1)
        };

        var outerPanel = new StackPanel { Margin = new Thickness(20), Spacing = 16 };

        var titleText = new TextBlock
        {
            Text = $"Add \"{song.Title}\" to:",
            FontSize = 14,
            FontWeight = Avalonia.Media.FontWeight.SemiBold,
            Foreground = new SolidColorBrush(Color.Parse("#E0E0E0")),
            TextWrapping = Avalonia.Media.TextWrapping.Wrap
        };
        outerPanel.Children.Add(titleText);

        var scrollViewer = new ScrollViewer
        {
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        };

        var itemsPanel = new StackPanel { Spacing = 4 };

        foreach (var playlist in playlists)
        {
            var playlistBorder = new Border
            {
                Background = new SolidColorBrush(Color.Parse("#2A2A3E")),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12, 10),
                Cursor = new Cursor(StandardCursorType.Hand)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

            var icon = new TextBlock
            {
                Text = "\uE8FA",
                FontFamily = new FontFamily("Segoe Fluent Icons"),
                FontSize = 16,
                Foreground = new SolidColorBrush(Color.Parse("#B088F9")),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 12, 0)
            };
            Grid.SetColumn(icon, 0);

            var nameText = new TextBlock
            {
                Text = playlist.Name,
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.Parse("#E0E0E0")),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(nameText, 1);

            grid.Children.Add(icon);
            grid.Children.Add(nameText);
            playlistBorder.Child = grid;

            var currentPlaylist = playlist;
            playlistBorder.PointerPressed += async (_, _) =>
            {
                await _userPlaylistService.AddSongToPlaylistAsync(currentPlaylist.Id, song);
                dialog.Close();
            };

            itemsPanel.Children.Add(playlistBorder);
        }

        scrollViewer.Content = itemsPanel;
        outerPanel.Children.Add(scrollViewer);

        dialog.Content = outerPanel;

        await dialog.ShowDialog(mainWindow);
    }

    private Window? GetMainWindow()
    {
        if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow;
        }

        return null;
    }
}
