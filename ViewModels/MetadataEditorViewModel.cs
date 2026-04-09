using System;
using System.IO;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using ReactiveUI;
using LocalMusicPlayer.Models;
using LocalMusicPlayer.Services;
using TagLib;

namespace LocalMusicPlayer.ViewModels;

public class MetadataEditorViewModel : ViewModelBase
{
    private readonly Song _song;
    private readonly IDialogService _dialogService;
    private readonly Action? _onSaved;

    private string _title;
    private string _artist;
    private string _album;
    private int _trackNumber;
    private string? _albumArtPath;
    private Bitmap? _albumArtBitmap;

    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    public string Artist
    {
        get => _artist;
        set => this.RaiseAndSetIfChanged(ref _artist, value);
    }

    public string Album
    {
        get => _album;
        set => this.RaiseAndSetIfChanged(ref _album, value);
    }

    public int TrackNumber
    {
        get => _trackNumber;
        set => this.RaiseAndSetIfChanged(ref _trackNumber, value);
    }

    public string? AlbumArtPath
    {
        get => _albumArtPath;
        set => this.RaiseAndSetIfChanged(ref _albumArtPath, value);
    }

    public Bitmap? AlbumArtBitmap
    {
        get => _albumArtBitmap;
        set => this.RaiseAndSetIfChanged(ref _albumArtBitmap, value);
    }

    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }
    public ReactiveCommand<Unit, Unit> ChangeCoverCommand { get; }

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

        LoadAlbumArt();

        SaveCommand = ReactiveCommand.CreateFromTask(SaveMetadataAsync);

        CancelCommand = ReactiveCommand.Create(() => { });

        ChangeCoverCommand = ReactiveCommand.CreateFromTask(ChangeCoverAsync);
    }

    private void LoadAlbumArt()
    {
        if (!string.IsNullOrEmpty(_song.AlbumArtPath) && System.IO.File.Exists(_song.AlbumArtPath))
        {
            try
            {
                AlbumArtBitmap = new Bitmap(_song.AlbumArtPath);
            }
            catch
            {
                AlbumArtBitmap = null;
            }
        }
    }

    private async Task SaveMetadataAsync()
    {
        try
        {
            using var file = TagLib.File.Create(_song.FilePath);

            file.Tag.Title = string.IsNullOrWhiteSpace(Title) ? System.IO.Path.GetFileNameWithoutExtension(_song.FilePath) : Title;
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
                Data = new ByteVector(imagePath)
            };

            file.Tag.Pictures = new IPicture[] { picture };
            file.Save();

            AlbumArtPath = imagePath;
            _song.AlbumArtPath = imagePath;

            LoadAlbumArt();
            this.RaisePropertyChanged(nameof(AlbumArtBitmap));
        }
        catch (Exception ex)
        {
            await _dialogService.ShowMessageDialogAsync("Error", $"Failed to change cover: {ex.Message}");
        }
    }
}
