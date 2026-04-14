using LibVLCSharp.Shared;
using System;
using System.Threading.Tasks;
using LocalMusicPlayer.Models;
using Microsoft.Extensions.Logging;

namespace LocalMusicPlayer.Services;

public class MusicPlayerService : IMusicPlayerService, IDisposable
{
    private readonly LibVLC _libVlc;
    private MediaPlayer? _mediaPlayer;
    private Song? _currentSong;
    private int _volume = 100;
    private bool _isMuted;
    private int _previousVolume;
    private bool _disposed;
    private float _playbackRate = 1.0f;
    private bool _replayGainEnabled = true;
    private float _replayGainAdjustment = 0f;
    private readonly ILogger<MusicPlayerService>? _logger;
    private Equalizer? _equalizer;
    private int _equalizerPresetId = -1; // -1 = flat / off

    public event EventHandler? PlaybackEnded;
    public event EventHandler<PlayState>? PlaybackStateChanged;
    public event EventHandler<TimeSpan>? PositionChanged;

    public TimeSpan Position => _mediaPlayer?.Time > 0 ? TimeSpan.FromMilliseconds(_mediaPlayer.Time) : TimeSpan.Zero;

    public TimeSpan Duration =>
        _mediaPlayer?.Length > 0 ? TimeSpan.FromMilliseconds(_mediaPlayer.Length) : TimeSpan.Zero;

    public bool IsPlaying => _mediaPlayer?.IsPlaying ?? false;
    public int Volume => _volume;
    public bool IsMuted => _isMuted;
    public float PlaybackRate => _playbackRate;
    public bool IsEqualizerEnabled => _equalizer != null && _equalizerPresetId >= 0;

    public int EqualizerBandCount => _equalizer != null ? (int)_equalizer.BandCount : 0;
    public float EqualizerPreamp => _equalizer?.Preamp ?? 0f;
    public int EqualizerPresetId => _equalizerPresetId;

    public MusicPlayerService(ILogger<MusicPlayerService>? logger = null)
    {
        _logger = logger;
        Core.Initialize();
        _libVlc = new LibVLC("--quiet", "--no-video");
        _mediaPlayer = new MediaPlayer(_libVlc);

        _mediaPlayer.EndReached += OnEndReached;
        _mediaPlayer.TimeChanged += (_, args) => PositionChanged?.Invoke(this, TimeSpan.FromMilliseconds(args.Time));
        _mediaPlayer.Playing += (_, _) => PlaybackStateChanged?.Invoke(this, PlayState.Playing);
        _mediaPlayer.Paused += (_, _) => PlaybackStateChanged?.Invoke(this, PlayState.Paused);
        _mediaPlayer.Stopped += (_, _) => PlaybackStateChanged?.Invoke(this, PlayState.Stopped);
        _logger?.LogInformation("[MusicPlayer] MusicPlayerService initialized");
    }

    public void Play(Song song)
    {
        if (_disposed || _mediaPlayer == null) return;

        _currentSong = song;
        _logger?.LogInformation("[MusicPlayer] Play started: {Title} from {FilePath}", song.Title, song.FilePath);
        using var media = new Media(_libVlc, new Uri(song.FilePath));
        _mediaPlayer.Play(media);

        if (_replayGainEnabled)
            ApplyReplayGain(song.ReplayGainTrackGain);
        else
            _mediaPlayer.Volume = _volume;
    }

    public void SetReplayGainEnabled(bool enabled)
    {
        _replayGainEnabled = enabled;
        if (_currentSong != null)
            ApplyReplayGain(_currentSong.ReplayGainTrackGain);
    }

    public void ApplyReplayGain(float trackGain)
    {
        _replayGainAdjustment = trackGain;
        var effectiveVolume = _replayGainEnabled
            ? Math.Clamp(_volume + (int)Math.Round(trackGain), 0, 100)
            : _volume;

        if (_mediaPlayer != null)
            _mediaPlayer.Volume = effectiveVolume;
    }

    public void Pause()
    {
        _mediaPlayer?.Pause();
    }

    public void Resume()
    {
        if (_mediaPlayer != null && !_mediaPlayer.IsPlaying)
        {
            _mediaPlayer.Play();
        }
    }

    public void Stop()
    {
        _logger?.LogInformation("[MusicPlayer] Playback stopped");
        _mediaPlayer?.Stop();
    }

    public void Next()
    {
    }

    public void Previous()
    {
    }

    public void Seek(TimeSpan position)
    {
        if (_mediaPlayer != null)
        {
            _mediaPlayer.Time = (long)position.TotalMilliseconds;
            _logger?.LogDebug("[MusicPlayer] Seek to {Position}", position);
        }
    }

    public void SetVolume(int level)
    {
        _volume = Math.Clamp(level, 0, 100);
        ApplyReplayGain(_replayGainAdjustment);
    }

    public void Mute()
    {
        if (_isMuted)
        {
            _isMuted = false;
            _volume = _previousVolume;
            ApplyReplayGain(_replayGainAdjustment);
        }
        else
        {
            _previousVolume = _volume;
            _isMuted = true;
            if (_mediaPlayer != null)
            {
                _mediaPlayer.Volume = 0;
            }
        }
    }

    public void SetPlaybackRate(float rate)
    {
        _playbackRate = Math.Clamp(rate, 0.5f, 2.0f);
        if (_mediaPlayer != null)
        {
            _mediaPlayer.SetRate(_playbackRate);
        }

        _logger?.LogDebug("[MusicPlayer] Playback rate changed to {Rate}x", _playbackRate);
    }

    public async Task FadeInAsync(int targetVolume, TimeSpan duration)
    {
        if (_mediaPlayer == null || _disposed) return;

        var steps = 20;
        var stepDuration = duration.TotalMilliseconds / steps;
        var volumeStep = (double)targetVolume / steps;

        for (int i = 1; i <= steps; i++)
        {
            if (_disposed || _mediaPlayer == null) break;
            var vol = (int)Math.Round(volumeStep * i);
            _mediaPlayer.Volume = Math.Clamp(vol, 0, 100);
            await Task.Delay((int)stepDuration);
        }

        _volume = targetVolume;
    }

    public async Task FadeOutAsync(TimeSpan duration)
    {
        if (_mediaPlayer == null || _disposed) return;

        var steps = 20;
        var stepDuration = duration.TotalMilliseconds / steps;
        var currentVol = _volume;
        var volumeStep = (double)currentVol / steps;

        for (int i = steps - 1; i >= 0; i--)
        {
            if (_disposed || _mediaPlayer == null) break;
            var vol = (int)Math.Round(volumeStep * i);
            _mediaPlayer.Volume = Math.Clamp(vol, 0, 100);
            await Task.Delay((int)stepDuration);
        }
    }

    public void SetEqualizerPreset(int presetId)
    {
        // Disable if negative or exceeds preset count
        using var tmpEq = new Equalizer();
        if (presetId < 0 || presetId >= (int)tmpEq.PresetCount)
        {
            _equalizerPresetId = -1;
            _equalizer = null;
            if (_mediaPlayer != null)
                _mediaPlayer.SetEqualizer(null!);
            _logger?.LogDebug("[MusicPlayer] Equalizer disabled");
            return;
        }

        _equalizerPresetId = presetId;
        _equalizer = new Equalizer((uint)presetId);
        if (_mediaPlayer != null)
            _mediaPlayer.SetEqualizer(_equalizer);
        _logger?.LogDebug("[MusicPlayer] Equalizer preset {PresetId} applied", presetId);
    }

    public void SetEqualizerBand(int band, float gain)
    {
        if (_equalizer == null)
        {
            _equalizer = new Equalizer();
            _equalizerPresetId = 0; // Custom
        }
        _equalizer.SetAmp(gain, (uint)band);
        if (_mediaPlayer != null)
            _mediaPlayer.SetEqualizer(_equalizer);
    }

    public void SetEqualizerPreamp(float preamp)
    {
        if (_equalizer == null)
            _equalizer = new Equalizer();
        _equalizer.SetPreamp(preamp);
        if (_mediaPlayer != null)
            _mediaPlayer.SetEqualizer(_equalizer);
    }

    public float GetEqualizerBand(int band)
    {
        if (_equalizer == null)
            return 0f;
        return _equalizer.Amp((uint)band);
    }

    public float[] GetAllEqualizerBands()
    {
        if (_equalizer == null)
            return Array.Empty<float>();
        var bandCount = (int)_equalizer.BandCount;
        var bands = new float[bandCount];
        for (int i = 0; i < bandCount; i++)
            bands[i] = _equalizer.Amp((uint)i);
        return bands;
    }

    public static string GetPresetName(int presetId)
    {
        if (presetId < 0)
            return "Flat";
        using var tmpEq = new Equalizer();
        if (presetId >= (int)tmpEq.PresetCount)
            return "Flat";
        return tmpEq.PresetName((uint)presetId) ?? "Flat";
    }

    public static int GetPresetCount()
    {
        using var tmp = new Equalizer();
        return (int)tmp.PresetCount;
    }

    // Instance methods matching IMusicPlayerService
    string IMusicPlayerService.GetEqualizerPresetName(int presetId) => GetPresetName(presetId);
    int IMusicPlayerService.GetEqualizerPresetCount() => GetPresetCount();
    float IMusicPlayerService.GetEqualizerBand(int band) => GetEqualizerBand(band);
    float[] IMusicPlayerService.GetAllEqualizerBands() => GetAllEqualizerBands();

    private void OnEndReached(object? sender, EventArgs e)
    {
        _logger?.LogInformation("[MusicPlayer] Playback ended");
        PlaybackEnded?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        _disposed = true;
        _logger?.LogInformation("[MusicPlayer] MusicPlayerService disposing");

        if (disposing)
        {
            _mediaPlayer?.Stop();
            _mediaPlayer?.Dispose();
            _libVlc.Dispose();
        }
    }
}
