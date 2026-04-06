using System;

namespace LocalMusicPlayer.Services;

public interface IKeyboardShortcutService
{
    void SetNavigateBackAction(Action action);
    void PlayPause();
    void SeekForward(int seconds = 5);
    void SeekBackward(int seconds = 5);
    void NextTrack();
    void PreviousTrack();
    void ToggleMute();
    void VolumeUp(int amount = 5);
    void VolumeDown(int amount = 5);
    void NavigateBack();

    event EventHandler<string>? ShortcutExecuted;
}