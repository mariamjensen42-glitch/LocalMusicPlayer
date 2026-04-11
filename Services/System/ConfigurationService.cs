using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LocalMusicPlayer.Data;
using LocalMusicPlayer.Models;
using Microsoft.EntityFrameworkCore;

namespace LocalMusicPlayer.Services;

public class ConfigurationService : IConfigurationService
{
    private AppSettings _currentSettings = new();
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    private static readonly JsonSerializerOptions _jsonWriteOptions = new()
    {
        WriteIndented = true
    };

    public AppSettings CurrentSettings => _currentSettings;

    public async Task LoadSettingsAsync()
    {
        await MigrateFromJsonIfNeededAsync();

        var settings = new AppSettings();
        using var db = new AppDbContext();
        var entities = await db.Settings.ToListAsync();

        settings.ScanFolders = GetJsonValue(entities, nameof(AppSettings.ScanFolders), settings.ScanFolders);
        settings.IncludeSubfolders = GetJsonValue(entities, nameof(AppSettings.IncludeSubfolders), settings.IncludeSubfolders);
        settings.Volume = GetJsonValue(entities, nameof(AppSettings.Volume), settings.Volume);
        settings.IsMuted = GetJsonValue(entities, nameof(AppSettings.IsMuted), settings.IsMuted);
        settings.LastScanTime = GetJsonValue(entities, nameof(AppSettings.LastScanTime), settings.LastScanTime);
        settings.Theme = GetJsonValue(entities, nameof(AppSettings.Theme), settings.Theme);
        settings.PlaybackMode = GetJsonValue(entities, nameof(AppSettings.PlaybackMode), settings.PlaybackMode);
        settings.PlaybackRate = GetJsonValue(entities, nameof(AppSettings.PlaybackRate), settings.PlaybackRate);
        settings.CrossfadeEnabled = GetJsonValue(entities, nameof(AppSettings.CrossfadeEnabled), settings.CrossfadeEnabled);
        settings.CrossfadeDurationMs = GetJsonValue(entities, nameof(AppSettings.CrossfadeDurationMs), settings.CrossfadeDurationMs);
        settings.LyricFontSize = GetJsonValue(entities, nameof(AppSettings.LyricFontSize), settings.LyricFontSize);
        settings.LyricLineSpacing = GetJsonValue(entities, nameof(AppSettings.LyricLineSpacing), settings.LyricLineSpacing);
        settings.ShowTranslation = GetJsonValue(entities, nameof(AppSettings.ShowTranslation), settings.ShowTranslation);
        settings.ReplayGainEnabled = GetJsonValue(entities, nameof(AppSettings.ReplayGainEnabled), settings.ReplayGainEnabled);
        settings.SongStatistics = GetJsonValue(entities, nameof(AppSettings.SongStatistics), settings.SongStatistics);
        settings.TotalPlayTimeMs = GetJsonValue(entities, nameof(AppSettings.TotalPlayTimeMs), settings.TotalPlayTimeMs);
        settings.FirstScanDate = GetJsonValue(entities, nameof(AppSettings.FirstScanDate), settings.FirstScanDate);
        settings.UserPlaylists = GetJsonValue(entities, nameof(AppSettings.UserPlaylists), settings.UserPlaylists);
        settings.FavoriteFilePaths = GetJsonValue(entities, nameof(AppSettings.FavoriteFilePaths), settings.FavoriteFilePaths);
        settings.PlayHistory = GetJsonValue(entities, nameof(AppSettings.PlayHistory), settings.PlayHistory);
        settings.LastSongFilePath = GetJsonValue<string?>(entities, nameof(AppSettings.LastSongFilePath), null);
        settings.QueueFilePaths = GetJsonValue(entities, nameof(AppSettings.QueueFilePaths), settings.QueueFilePaths);
        settings.LastPlaybackPosition = GetJsonValue(entities, nameof(AppSettings.LastPlaybackPosition), settings.LastPlaybackPosition);

        _currentSettings = settings;
    }

    public async Task SaveSettingsAsync()
    {
        using var db = new AppDbContext();
        await SaveEntityAsync(db, nameof(AppSettings.ScanFolders), _currentSettings.ScanFolders);
        await SaveEntityAsync(db, nameof(AppSettings.IncludeSubfolders), _currentSettings.IncludeSubfolders);
        await SaveEntityAsync(db, nameof(AppSettings.Volume), _currentSettings.Volume);
        await SaveEntityAsync(db, nameof(AppSettings.IsMuted), _currentSettings.IsMuted);
        await SaveEntityAsync(db, nameof(AppSettings.LastScanTime), _currentSettings.LastScanTime);
        await SaveEntityAsync(db, nameof(AppSettings.Theme), _currentSettings.Theme);
        await SaveEntityAsync(db, nameof(AppSettings.PlaybackMode), _currentSettings.PlaybackMode);
        await SaveEntityAsync(db, nameof(AppSettings.PlaybackRate), _currentSettings.PlaybackRate);
        await SaveEntityAsync(db, nameof(AppSettings.CrossfadeEnabled), _currentSettings.CrossfadeEnabled);
        await SaveEntityAsync(db, nameof(AppSettings.CrossfadeDurationMs), _currentSettings.CrossfadeDurationMs);
        await SaveEntityAsync(db, nameof(AppSettings.LyricFontSize), _currentSettings.LyricFontSize);
        await SaveEntityAsync(db, nameof(AppSettings.LyricLineSpacing), _currentSettings.LyricLineSpacing);
        await SaveEntityAsync(db, nameof(AppSettings.ShowTranslation), _currentSettings.ShowTranslation);
        await SaveEntityAsync(db, nameof(AppSettings.ReplayGainEnabled), _currentSettings.ReplayGainEnabled);
        await SaveEntityAsync(db, nameof(AppSettings.SongStatistics), _currentSettings.SongStatistics);
        await SaveEntityAsync(db, nameof(AppSettings.TotalPlayTimeMs), _currentSettings.TotalPlayTimeMs);
        await SaveEntityAsync(db, nameof(AppSettings.FirstScanDate), _currentSettings.FirstScanDate);
        await SaveEntityAsync(db, nameof(AppSettings.UserPlaylists), _currentSettings.UserPlaylists);
        await SaveEntityAsync(db, nameof(AppSettings.FavoriteFilePaths), _currentSettings.FavoriteFilePaths);
        await SaveEntityAsync(db, nameof(AppSettings.PlayHistory), _currentSettings.PlayHistory);
        await SaveEntityAsync(db, nameof(AppSettings.LastSongFilePath), _currentSettings.LastSongFilePath);
        await SaveEntityAsync(db, nameof(AppSettings.QueueFilePaths), _currentSettings.QueueFilePaths);
        await SaveEntityAsync(db, nameof(AppSettings.LastPlaybackPosition), _currentSettings.LastPlaybackPosition);
        await db.SaveChangesAsync();
    }

    public List<string> GetScanFolders()
    {
        return _currentSettings.ScanFolders;
    }

    public async Task AddScanFolderAsync(string path)
    {
        if (!_currentSettings.ScanFolders.Contains(path))
        {
            _currentSettings.ScanFolders.Add(path);
            await SaveSettingsAsync();
        }
    }

    public async Task RemoveScanFolderAsync(string path)
    {
        if (_currentSettings.ScanFolders.Remove(path))
        {
            await SaveSettingsAsync();
        }
    }

    public async Task SavePlaybackStateAsync(string? lastSongFilePath, List<string> queueFilePaths, double lastPlaybackPosition)
    {
        _currentSettings.LastSongFilePath = lastSongFilePath;
        _currentSettings.QueueFilePaths = queueFilePaths;
        _currentSettings.LastPlaybackPosition = lastPlaybackPosition;
        await SaveSettingsAsync();
    }

    private T GetJsonValue<T>(List<AppSettingsEntity> entities, string key, T defaultValue)
    {
        var entity = entities.FirstOrDefault(e => e.Key == key);
        if (entity == null)
            return defaultValue;

        try
        {
            return JsonSerializer.Deserialize<T>(entity.Value, _jsonOptions) ?? defaultValue;
        }
        catch (JsonException)
        {
            return defaultValue;
        }
    }

    private async Task SaveEntityAsync(AppDbContext db, string key, object? value)
    {
        var json = JsonSerializer.Serialize(value, value?.GetType() ?? typeof(object), _jsonWriteOptions);
        var entity = await db.Settings.FirstOrDefaultAsync(e => e.Key == key);

        if (entity != null)
        {
            entity.Value = json;
            db.Settings.Update(entity);
        }
        else
        {
            db.Settings.Add(new AppSettingsEntity { Key = key, Value = json });
        }
    }

    private async Task MigrateFromJsonIfNeededAsync()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var configDirectory = Path.Combine(appDataPath, "LocalMusicPlayer");
        var configFilePath = Path.Combine(configDirectory, "settings.json");

        if (!File.Exists(configFilePath))
            return;

        using var checkDb = new AppDbContext();
        var migrationKey = "_MigratedFromJson";
        var alreadyMigrated = await checkDb.Settings.AnyAsync(e => e.Key == migrationKey);
        if (alreadyMigrated)
            return;

        try
        {
            var json = await File.ReadAllTextAsync(configFilePath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json, _jsonOptions);
            if (settings == null)
                return;

            _currentSettings = settings;
            await SaveSettingsAsync();

            using var markDb = new AppDbContext();
            markDb.Settings.Add(new AppSettingsEntity { Key = migrationKey, Value = "true" });
            await markDb.SaveChangesAsync();

            File.Delete(configFilePath);
        }
        catch (JsonException)
        {
        }
        catch (IOException)
        {
        }
    }
}
