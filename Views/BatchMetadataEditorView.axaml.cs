using Avalonia.Controls;
using Avalonia.Interactivity;
using LocalMusicPlayer.Models;
using LocalMusicPlayer.ViewModels;

namespace LocalMusicPlayer.Views;

public partial class BatchMetadataEditorView : Window
{
    public BatchMetadataEditorView()
    {
        InitializeComponent();
    }

    private void OnFieldTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox && textBox.DataContext is BatchEditField field)
        {
            field.IsModified = true;
        }
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is BatchMetadataEditorViewModel vm)
        {
            vm.CancelCommand.Execute(null);
        }
        Close();
    }
}
