using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using LocalMusicPlayer.Models;

namespace LocalMusicPlayer.Services;

public interface IDialogService
{
    Task<string?> ShowInputDialogAsync(string title, string defaultValue = "");
    Task ShowMessageDialogAsync(string title, string message);
    Task<string?> ShowOpenFileDialogAsync(string title, string[]? filters = null);
    Task<string?> ShowSaveFileDialogAsync(string title, string[]? filters = null);
    Task ShowMetadataEditorDialogAsync(Song song, Action? onSaved = null);
    Task ShowBatchMetadataEditorDialogAsync(System.Collections.IList songs, Action? onSaved = null);
    Task<IReadOnlyList<string>?> ShowFolderPickerAsync();
}
