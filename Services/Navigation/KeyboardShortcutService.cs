using System;
using LocalMusicPlayer.Models;

namespace LocalMusicPlayer.Services;

public class KeyboardShortcutService : IKeyboardShortcutService
{
    private readonly IMusicPlayerService _musicPlayerService;
    private readonly IPlaylistService _playlistService;
    private Action? _navigateBackAction;

    public event EventHandler<string>? ShortcutExecuted;

    public KeyboardShortcutService(
        IMusicPlayerService musicPlayerService,
        IPlaylistService playlistService)
    {
        _musicPlayerService = musicPlayerService;
        _playlistService = playlistService;
    }

    public void SetNavigateBackAction(Action action)
    {
        _navigateBackAction = action;
    }

    public void PlayPause()
    {
        if (_musicPlayerService.IsPlaying)
        {
            _musicPlayerService.Pause();
            ShortcutExecuted?.Invoke(this, "Paused");
        }
        else
        {
            _musicPlayerService.Resume();
            ShortcutExecuted?.Invoke(this, "Playing");
        }
    }

    public void SeekForward(int seconds = 5)
    {
        var newPosition = _musicPlayerService.Position + TimeSpan.FromSeconds(seconds);
        var maxPosition = _musicPlayerService.Duration;
        if (newPosition > maxPosition)
            newPosition = maxPosition;
        _musicPlayerService.Seek(newPosition);
        ShortcutExecuted?.Invoke(this, $"Seek forward {seconds}s");
    }

    public void SeekBackward(int seconds = 5)
    {
        var newPosition = _musicPlayerService.Position - TimeSpan.FromSeconds(seconds);
        if (newPosition < TimeSpan.Zero)
            newPosition = TimeSpan.Zero;
        _musicPlayerService.Seek(newPosition);
        ShortcutExecuted?.Invoke(this, $"Seek backward {seconds}s");
    }

    public void NextTrack()
    {
        if (_playlistService.PlayNext())
        {
            var currentSong = _playlistService.CurrentSong;
            if (currentSong != null)
            {
                _musicPlayerService.Play(currentSong);
                ShortcutExecuted?.Invoke(this, "Next track");
            }
        }
    }

    public void PreviousTrack()
    {
        if (_playlistService.PlayPrevious())
        {
            var currentSong = _playlistService.CurrentSong;
            if (currentSong != null)
            {
                _musicPlayerService.Play(currentSong);
                ShortcutExecuted?.Invoke(this, "Previous track");
            }
        }
    }

    public void ToggleMute()
    {
        _musicPlayerService.Mute();
        ShortcutExecuted?.Invoke(this, _musicPlayerService.IsMuted ? "Muted" : "Unmuted");
    }

    public void VolumeUp(int amount = 5)
    {
        var newVolume = _musicPlayerService.Volume + amount;
        if (newVolume > 100)
            newVolume = 100;
        _musicPlayerService.SetVolume(newVolume);
        ShortcutExecuted?.Invoke(this, $"Volume: {newVolume}");
    }

    public void VolumeDown(int amount = 5)
    {
        var newVolume = _musicPlayerService.Volume - amount;
        if (newVolume < 0)
            newVolume = 0;
        _musicPlayerService.SetVolume(newVolume);
        ShortcutExecuted?.Invoke(this, $"Volume: {newVolume}");
    }

    public void NavigateBack()
    {
        _navigateBackAction?.Invoke();
        ShortcutExecuted?.Invoke(this, "Navigate back");
    }
}
