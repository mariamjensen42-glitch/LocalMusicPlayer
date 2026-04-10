using CommunityToolkit.Mvvm.ComponentModel;

namespace LocalMusicPlayer.Models;

public partial class BatchEditField : ObservableObject
{
    [ObservableProperty] private string _fieldName;

    [ObservableProperty] private string _value;

    [ObservableProperty] private string _placeholder;

    [ObservableProperty] private bool _isMixedValue;

    [ObservableProperty] private bool _isModified;

    public BatchEditField(string fieldName, string value, bool isMixedValue = false)
    {
        _fieldName = fieldName;
        _value = isMixedValue ? string.Empty : value;
        _placeholder = isMixedValue ? value : string.Empty;
        _isMixedValue = isMixedValue;
        _isModified = false;
    }
}
