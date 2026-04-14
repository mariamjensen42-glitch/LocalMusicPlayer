using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalMusicPlayer.Models;
using LocalMusicPlayer.Services;

namespace LocalMusicPlayer.ViewModels;

public partial class SmartPlaylistEditorViewModel : ViewModelBase
{
    private readonly ISmartPlaylistService _smartPlaylistService;
    private readonly IDialogService _dialogService;
    private readonly SmartPlaylist? _editingPlaylist;

    [ObservableProperty] private string _playlistName = string.Empty;
    [ObservableProperty] private SmartPlaylistRule _selectedRule = SmartPlaylistRule.MostPlayed;
    [ObservableProperty] private int _limit = 50;
    [ObservableProperty] private DateTime? _fromDate;
    [ObservableProperty] private DateTime? _toDate;
    [ObservableProperty] private bool _isSaving;

    public bool IsEditing => _editingPlaylist != null;
    public string PageTitle => IsEditing ? "编辑智能播放列表" : "新建智能播放列表";

    public SmartPlaylistRule[] AvailableRules { get; } = Enum.GetValues<SmartPlaylistRule>();

    public event Action? OnSaved;
    public event Action? OnCancelled;

    public SmartPlaylistEditorViewModel(
        ISmartPlaylistService smartPlaylistService,
        IDialogService dialogService,
        SmartPlaylist? editingPlaylist = null)
    {
        _smartPlaylistService = smartPlaylistService;
        _dialogService = dialogService;
        _editingPlaylist = editingPlaylist;

        if (_editingPlaylist != null)
        {
            PlaylistName = _editingPlaylist.Name;
            SelectedRule = _editingPlaylist.Rule;
            Limit = _editingPlaylist.Limit;
            FromDate = _editingPlaylist.FromDate;
            ToDate = _editingPlaylist.ToDate;
        }
    }

    [RelayCommand]
    private void Save()
    {
        if (string.IsNullOrWhiteSpace(PlaylistName))
            return;

        IsSaving = true;

        var playlist = _editingPlaylist ?? new SmartPlaylist();
        playlist.Name = PlaylistName.Trim();
        playlist.Rule = SelectedRule;
        playlist.Limit = Limit;
        playlist.FromDate = FromDate;
        playlist.ToDate = ToDate;

        _smartPlaylistService.SaveSmartPlaylist(playlist);
        OnSaved?.Invoke();
    }

    [RelayCommand]
    private void Cancel()
    {
        OnCancelled?.Invoke();
    }

    public string GetRuleDisplayName(SmartPlaylistRule rule) => rule switch
    {
        SmartPlaylistRule.MostPlayed => "播放最多",
        SmartPlaylistRule.RecentlyPlayed => "最近播放",
        SmartPlaylistRule.LeastPlayed => "播放最少",
        SmartPlaylistRule.RecentlyAdded => "最近添加",
        SmartPlaylistRule.NeverPlayed => "从未播放",
        _ => rule.ToString()
    };
}
