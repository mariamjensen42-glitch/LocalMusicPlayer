using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace LocalMusicPlayer.Services;

public class DialogService : IDialogService
{
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

#pragma warning disable CS0618 // OpenFileDialog is obsolete in Avalonia 11.x
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

    private Window? GetMainWindow()
    {
        if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow;
        }

        return null;
    }
}
