using Avalonia.Controls;
using Avalonia.Interactivity;
using LocalMusicPlayer.ViewModels;

namespace LocalMusicPlayer.Views.Editors;

public partial class MetadataEditorView : Window
{
    public MetadataEditorView()
    {
        InitializeComponent();
    }

    private void CancelButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void SaveButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
