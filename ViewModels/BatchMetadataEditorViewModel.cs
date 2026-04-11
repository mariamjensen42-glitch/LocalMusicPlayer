using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalMusicPlayer.Models;
using LocalMusicPlayer.Services;

namespace LocalMusicPlayer.ViewModels;

public partial class BatchMetadataEditorViewModel : ViewModelBase
{
    private readonly IEnumerable<Song> _selectedSongs;
    private readonly IDialogService _dialogService;
    private readonly Action? _onSaved;
    private readonly CancellationTokenSource _cancellationTokenSource;

    [ObservableProperty] private BatchEditField _titleField;

    [ObservableProperty] private BatchEditField _artistField;

    [ObservableProperty] private BatchEditField _albumField;

    [ObservableProperty] private BatchEditField _genreField;

    [ObservableProperty] private BatchEditField _yearField;

    [ObservableProperty] private int _totalCount;

    [ObservableProperty] private int _processedCount;

    [ObservableProperty] private int _successCount;

    [ObservableProperty] private int _errorCount;

    [ObservableProperty] private bool _isSaving;

    [ObservableProperty] private bool _isCompleted;

    [ObservableProperty] private string _statusMessage = string.Empty;

    public double ProgressPercentage => TotalCount > 0 ? (double)ProcessedCount / TotalCount * 100 : 0;

    public bool HasErrors => ErrorCount > 0;

    public BatchMetadataEditorViewModel(IEnumerable<Song> selectedSongs, IDialogService dialogService, Action? onSaved = null)
    {
        _selectedSongs = selectedSongs;
        _dialogService = dialogService;
        _onSaved = onSaved;
        _cancellationTokenSource = new CancellationTokenSource();

        var songs = selectedSongs.ToList();
        TotalCount = songs.Count;

        TitleField = CreateBatchField("Title", songs, s => s.Title);
        ArtistField = CreateBatchField("Artist", songs, s => s.Artist);
        AlbumField = CreateBatchField("Album", songs, s => s.Album);
        GenreField = CreateBatchField("Genre", songs, s => s.Genre);
        YearField = CreateBatchField("Year", songs, s => s.Year.ToString());
    }

    private static BatchEditField CreateBatchField(string fieldName, List<Song> songs, Func<Song, string> valueSelector)
    {
        var values = songs.Select(valueSelector).Distinct().ToList();
        var isMixed = values.Count > 1;
        var displayValue = isMixed ? "<Mixed Values>" : values.FirstOrDefault() ?? string.Empty;
        return new BatchEditField(fieldName, displayValue, isMixed);
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        IsSaving = true;
        IsCompleted = false;
        StatusMessage = "Saving...";

        var songs = _selectedSongs.ToList();
        var modifiedFields = GetModifiedFields();

        if (modifiedFields.Count == 0)
        {
            StatusMessage = "No changes to save";
            IsSaving = false;
            return;
        }

        foreach (var song in songs)
        {
            if (_cancellationTokenSource.Token.IsCancellationRequested)
            {
                StatusMessage = "Cancelled";
                break;
            }

            try
            {
                await SaveSongMetadataAsync(song, modifiedFields);
                SuccessCount++;
            }
            catch (Exception)
            {
                ErrorCount++;
            }

            ProcessedCount++;
            OnPropertyChanged(nameof(ProgressPercentage));
        }

        IsSaving = false;
        IsCompleted = true;

        if (ErrorCount > 0)
        {
            StatusMessage = $"Completed with {ErrorCount} errors";
        }
        else if (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            StatusMessage = "All songs saved successfully";
            _onSaved?.Invoke();
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        if (IsSaving)
        {
            _cancellationTokenSource.Cancel();
        }
    }

    private Dictionary<string, string> GetModifiedFields()
    {
        var fields = new Dictionary<string, string>();

        if (TitleField.IsModified && !string.IsNullOrEmpty(TitleField.Value))
            fields["Title"] = TitleField.Value;

        if (ArtistField.IsModified && !string.IsNullOrEmpty(ArtistField.Value))
            fields["Artist"] = ArtistField.Value;

        if (AlbumField.IsModified && !string.IsNullOrEmpty(AlbumField.Value))
            fields["Album"] = AlbumField.Value;

        if (GenreField.IsModified && !string.IsNullOrEmpty(GenreField.Value))
            fields["Genre"] = GenreField.Value;

        if (YearField.IsModified && int.TryParse(YearField.Value, out var year))
            fields["Year"] = year.ToString();

        return fields;
    }

    private async Task SaveSongMetadataAsync(Song song, Dictionary<string, string> modifiedFields)
    {
        await Task.Run(() =>
        {
            using var file = TagLib.File.Create(song.FilePath);

            if (modifiedFields.TryGetValue("Title", out var title))
            {
                file.Tag.Title = title;
                song.Title = title;
            }

            if (modifiedFields.TryGetValue("Artist", out var artist))
            {
                file.Tag.Performers = new[] { artist };
                song.Artist = artist;
            }

            if (modifiedFields.TryGetValue("Album", out var album))
            {
                file.Tag.Album = album;
                song.Album = album;
            }

            if (modifiedFields.TryGetValue("Genre", out var genre))
            {
                file.Tag.Genres = new[] { genre };
                song.Genre = genre;
            }

            if (modifiedFields.TryGetValue("Year", out var yearStr) && uint.TryParse(yearStr, out var year))
            {
                file.Tag.Year = year;
            }

            file.Save();
        });
    }
}
