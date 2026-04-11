using Avalonia.Controls;

namespace LocalMusicPlayer.Views.Editors;

public partial class BatchMetadataEditorView : Window
{
    public BatchMetadataEditorView()
    {
        InitializeComponent();
    }

    private void OnFieldTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox && textBox.DataContext is Models.BatchEditField field)
        {
            field.IsModified = true;
        }
    }

    private void OnCancelClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close();
    }
}
