using System.Threading.Tasks;
using Avalonia.Controls;

namespace LocalMusicPlayer.Services;

public interface IDialogService
{
    Task<string?> ShowInputDialogAsync(string title, string defaultValue = "");
    Task ShowMessageDialogAsync(string title, string message);
    Task<string?> ShowOpenFileDialogAsync(string title, string[]? filters = null);
    Task<string?> ShowSaveFileDialogAsync(string title, string[]? filters = null);
}
