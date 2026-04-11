using System;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalMusicPlayer.Models;
using LocalMusicPlayer.Services;
using TagLib;

namespace LocalMusicPlayer.ViewModels;

public partial class MetadataEditorViewModel : ViewModelBase
{
    private readonly Song _song;
    private readonly IDialogService _dialogService;
    private readonly Action? _onSaved;

    [ObservableProperty]
    private string _title;

    [ObservableProperty]
    private string _artist;

    [ObservableProperty]
    private string _album;

    [ObservableProperty]
    private int _trackNumber;

    [ObservableProperty]
    private string? _albumArtPath;

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            using var file = TagLib.File.Create(_song.FilePath);

            file.Tag.Title = string.IsNullOrWhiteSpace(Title) ? Path.GetFileNameWithoutExtension(_song.FilePath) : Title;
            file.Tag.Album = Album;
            file.Tag.Performers = string.IsNullOrWhiteSpace(Artist) ? Array.Empty<string>() : new[] { Artist };
            file.Tag.Track = (uint)TrackNumber;

            file.Save();

            _song.Title = file.Tag.Title;
            _song.Artist = Artist;
            _song.Album = Album;
            _song.TrackNumber = TrackNumber;

            _onSaved?.Invoke();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowMessageDialogAsync("Error", $"Failed to save metadata: {ex.Message}");
        }
    }

    [RelayCommand]
    private void Cancel()
    {
    }

    [RelayCommand]
    private async Task ChangeCoverAsync()
    {
        var imagePath = await _dialogService.ShowOpenFileDialogAsync(
            "Select Cover Image",
            new[] { "jpg,jpeg,png" });

        if (string.IsNullOrEmpty(imagePath))
            return;

        try
        {
            using var file = TagLib.File.Create(_song.FilePath);

            var picture = new Picture
            {
                Type = PictureType.FrontCover,
                Description = "Cover",
                MimeType = "image/jpeg",
                Data = new ByteVector(System.IO.File.ReadAllBytes(imagePath))
            };

            file.Tag.Pictures = new IPicture[] { picture };
            file.Save();

            AlbumArtPath = imagePath;
            _song.AlbumArtPath = imagePath;
        }
        catch (Exception ex)
        {
            await _dialogService.ShowMessageDialogAsync("Error", $"Failed to change cover: {ex.Message}");
        }
    }

    public MetadataEditorViewModel(Song song, IDialogService dialogService, Action? onSaved = null)
    {
        _song = song;
        _dialogService = dialogService;
        _onSaved = onSaved;

        _title = song.Title;
        _artist = song.Artist;
        _album = song.Album;
        _trackNumber = song.TrackNumber;
        _albumArtPath = song.AlbumArtPath;
    }
}
