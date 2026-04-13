using System;
using Avalonia.Controls;
using LocalMusicPlayer.Models;
using LocalMusicPlayer.ViewModels;

namespace LocalMusicPlayer.Views.Library;

public partial class SongListView : UserControl
{
    private SongListViewModel? _viewModel;

    public SongListView()
    {
        InitializeComponent();
        InitializeEvents();
    }

    private void InitializeEvents()
    {
        SubscribeToViewModel(DataContext as SongListViewModel);
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        SubscribeToViewModel(DataContext as SongListViewModel);
    }

    private void SubscribeToViewModel(SongListViewModel? viewModel)
    {
        if (_viewModel != null)
            _viewModel.ScrollToCurrentSongRequested -= OnScrollToCurrentSongRequested;

        _viewModel = viewModel;

        if (_viewModel != null)
            _viewModel.ScrollToCurrentSongRequested += OnScrollToCurrentSongRequested;
    }

    private void OnScrollToCurrentSongRequested(Song? song)
    {
        if (song == null || SongList == null) return;

        ScrollToCurrentSong(song);
    }

    public void ScrollToCurrentSong(Song targetSong)
    {
        try
        {
            if (SongList?.Items == null) return;

            for (int i = 0; i < SongList.Items.Count; i++)
            {
                if (SongList.Items[i] is Song song && song.FilePath == targetSong.FilePath)
                {
                    var container = SongList.ContainerFromIndex(i);
                    if (container is Control control)
                    {
                        control.BringIntoView();
                    }

                    break;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to scroll to current song: {ex.Message}");
        }
    }
}
