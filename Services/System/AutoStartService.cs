using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace LocalMusicPlayer.Services;

internal class AutoStartService : IAutoStartService
{
    private const string AppName = "LocalMusicPlayer";
    private static readonly string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

    public bool IsAutoStartEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, false);
            var value = key?.GetValue(AppName) as string;
            return !string.IsNullOrEmpty(value);
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task SetAutoStartAsync(bool enabled)
    {
        await Task.Run(() =>
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true);
                if (key == null) return;

                if (enabled)
                {
                    var exePath = Process.GetCurrentProcess().MainModule?.FileName;
                    if (!string.IsNullOrEmpty(exePath))
                    {
                        key.SetValue(AppName, $"\"{exePath}\"");
                    }
                }
                else
                {
                    key.DeleteValue(AppName, false);
                }
            }
            catch (Exception)
            {
            }
        });
    }
}
