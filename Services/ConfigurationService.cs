using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using LocalMusicPlayer.Models;

namespace LocalMusicPlayer.Services;

public class ConfigurationService : IConfigurationService
{
    private readonly string _configFilePath;
    private readonly string _configDirectory;
    private AppSettings _currentSettings = new();

    public AppSettings CurrentSettings => _currentSettings;

    public ConfigurationService()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _configDirectory = Path.Combine(appDataPath, "LocalMusicPlayer");
        _configFilePath = Path.Combine(_configDirectory, "settings.json");

        EnsureDirectoryExists();
        LoadSettingsSync();
    }

    private void EnsureDirectoryExists()
    {
        if (!Directory.Exists(_configDirectory))
        {
            Directory.CreateDirectory(_configDirectory);
        }
    }

    private void LoadSettingsSync()
    {
        if (!System.IO.File.Exists(_configFilePath))
        {
            _currentSettings = new AppSettings();
            SaveSettingsSync();
            return;
        }

        try
        {
            var json = System.IO.File.ReadAllText(_configFilePath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            _currentSettings = settings ?? new AppSettings();
        }
        catch (JsonException)
        {
            // 配置文件损坏，使用默认配置
            _currentSettings = new AppSettings();
            SaveSettingsSync();
        }
        catch (IOException)
        {
            _currentSettings = new AppSettings();
        }
    }

    private void SaveSettingsSync()
    {
        try
        {
            EnsureDirectoryExists();
            var json = JsonSerializer.Serialize(_currentSettings, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            System.IO.File.WriteAllText(_configFilePath, json);
        }
        catch (IOException)
        {
            // 忽略保存错误，继续使用内存配置
        }
    }

    public async Task LoadSettingsAsync()
    {
        await Task.Run(() => LoadSettingsSync());
    }

    public async Task SaveSettingsAsync()
    {
        try
        {
            EnsureDirectoryExists();
            var json = JsonSerializer.Serialize(_currentSettings, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            await File.WriteAllTextAsync(_configFilePath, json);
        }
        catch (IOException)
        {
            // 忽略保存错误，继续使用内存配置
        }
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
}