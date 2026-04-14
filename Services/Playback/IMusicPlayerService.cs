using System;
using System.Threading.Tasks;
using LocalMusicPlayer.Models;

namespace LocalMusicPlayer.Services;

public interface IMusicPlayerService
{
    void Play(Song song);
    void Pause();
    void Resume();
    void Stop();
    void Next();
    void Previous();
    void Seek(TimeSpan position);
    void SetVolume(int level);
    void Mute();
    void SetPlaybackRate(float rate);
    void SetReplayGainEnabled(bool enabled);
    void ApplyReplayGain(float trackGain);
    void SetEqualizerPreset(int presetId);
    void SetEqualizerBand(int band, float gain);
    void SetEqualizerPreamp(float preamp);
    string GetEqualizerPresetName(int presetId);
    int GetEqualizerPresetCount();
    float GetEqualizerBand(int band);
    float[] GetAllEqualizerBands();

    Task FadeInAsync(int targetVolume, TimeSpan duration);
    Task FadeOutAsync(TimeSpan duration);

    TimeSpan Position { get; }
    TimeSpan Duration { get; }
    int Volume { get; }
    bool IsPlaying { get; }
    bool IsMuted { get; }
    float PlaybackRate { get; }
    bool IsEqualizerEnabled { get; }
    int EqualizerBandCount { get; }
    float EqualizerPreamp { get; }
    int EqualizerPresetId { get; }

    event EventHandler? PlaybackEnded;
    event EventHandler<PlayState>? PlaybackStateChanged;
    event EventHandler<TimeSpan>? PositionChanged;
}