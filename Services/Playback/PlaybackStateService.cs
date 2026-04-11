using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using LocalMusicPlayer.Models;

namespace LocalMusicPlayer.Services;

public class PlaybackStateService : IPlaybackStateService, IDisposable
{
    private readonly IPlaylistService _playlistService;
    private readonly IMusicPlayerService _playerService;
    private readonly IStatisticsService _statisticsService;
    private readonly DispatcherTimer _positionTimer;

    private Song? _currentTrackedSong;
    private TimeSpan _playStartTime;
    private TimeSpan _accumulatedPlayTime;
    private bool _hasTrackedPlay;
    private const double PlayThresholdPercentage = 0.5;

    public PlaybackStateService(IPlaylistService playlistService, IMusicPlayerService playerService, IStatisticsService statisticsService)
    {
        _playlistService = playlistService;
        _playerService = playerService;
        _statisticsService = statisticsService;

        _positionTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(250)
        };
        _positionTimer.Tick += (s, e) => { PositionChanged?.Invoke(this, Position); };

        _playerService.PlaybackEnded += OnPlaybackEnded;
        _playerService.PlaybackStateChanged += OnPlaybackStateChanged;
        _playerService.PositionChanged += OnPositionChanged;

        _playlistService.CurrentSongChanged += OnCurrentSongChanged;
        _playlistService.PlaybackModeChanged += OnPlaybackModeChanged;
    }

    private void OnPlaybackEnded(object? sender, EventArgs e)
    {
        _ = TrackPlayIfThresholdMetAsync();
        PlaybackEnded?.Invoke(this, EventArgs.Empty);

        if (IsCrossfadeEnabled)
        {
            _ = HandleCrossfadeAsync();
        }
        else
        {
            PlayNextSong();
        }
    }

    private async Task HandleCrossfadeAsync()
    {
        await _playerService.FadeOutAsync(CrossfadeDuration);

        if (PlayNextSong())
        {
            await _playerService.FadeInAsync(Volume, CrossfadeDuration);
        }
        else
        {
            Stop();
        }
    }

    private bool PlayNextSong()
    {
        if (_playlistService.PlayNext())
        {
            var song = _playlistService.CurrentSong;
            if (song != null)
            {
                _playerService.SetReplayGainEnabled(IsReplayGainEnabled);
                _playerService.Play(song);
                return true;
            }
        }

        return false;
    }

    private void OnPlaybackStateChanged(object? sender, PlayState state)
    {
        if (state == PlayState.Playing)
        {
            _playStartTime = _playerService.Position;
        }
        else if (state == PlayState.Paused || state == PlayState.Stopped)
        {
            UpdateAccumulatedPlayTime();
        }
        PlaybackStateChanged?.Invoke(this, state);
    }

    private void OnPositionChanged(object? sender, TimeSpan position)
    {
        PositionChanged?.Invoke(this, position);
    }

    private void OnCurrentSongChanged(object? sender, Song? song)
    {
        _ = TrackPlayIfThresholdMetAsync();
        StartTrackingNewSong(song);
        CurrentSongChanged?.Invoke(this, song);
    }

    private void OnPlaybackModeChanged(object? sender, PlaybackMode mode)
    {
        PlaybackModeChanged?.Invoke(this, mode);
    }

    public Song? CurrentSong => _playlistService.CurrentSong;

    public bool IsPlaying => _playerService.IsPlaying;

    public TimeSpan Position => _playerService.Position;

    public TimeSpan Duration => _playerService.Duration;

    public int Volume
    {
        get => _playerService.Volume;
        set => _playerService.SetVolume(value);
    }

    public bool IsMuted => _playerService.IsMuted;

    public float PlaybackRate => _playerService.PlaybackRate;

    public bool IsCrossfadeEnabled { get; set; }

    public TimeSpan CrossfadeDuration { get; set; } = TimeSpan.FromSeconds(3);

    public bool IsReplayGainEnabled { get; set; } = true;

    public PlaybackMode PlaybackMode
    {
        get => _playlistService.PlaybackMode;
        set => _playlistService.PlaybackMode = value;
    }

    public double PositionSeconds
    {
        get => Position.TotalSeconds;
        set => Seek(TimeSpan.FromSeconds(value));
    }

    public double DurationSeconds => Duration.TotalSeconds;

    public void Play(Song song)
    {
        _playerService.SetReplayGainEnabled(IsReplayGainEnabled);
        _playerService.Play(song);
    }

    public void Play()
    {
        var song = _playlistService.CurrentSong;
        if (song != null)
        {
            _playerService.SetReplayGainEnabled(IsReplayGainEnabled);
            _playerService.Play(song);
        }
    }

    public void Pause()
    {
        _playerService.Pause();
    }

    public void Resume()
    {
        _playerService.Resume();
    }

    public void Stop()
    {
        _playerService.Stop();
    }

    public void Seek(TimeSpan position)
    {
        _playerService.Seek(position);
    }

    public void Seek(double seconds)
    {
        Seek(TimeSpan.FromSeconds(seconds));
    }

    public void SetVolume(int level)
    {
        _playerService.SetVolume(level);
    }

    public void Mute()
    {
        _playerService.Mute();
    }

    public void SetPlaybackRate(float rate)
    {
        _playerService.SetPlaybackRate(rate);
    }

    public void PlayNext()
    {
        if (_playlistService.PlayNext())
        {
            var song = _playlistService.CurrentSong;
            if (song != null)
            {
                _playerService.SetReplayGainEnabled(IsReplayGainEnabled);
                _playerService.Play(song);
            }
        }
        else
        {
            Stop();
        }
    }

    public void PlayPrevious()
    {
        if (_playlistService.PlayPrevious())
        {
            var song = _playlistService.CurrentSong;
            if (song != null)
            {
                _playerService.SetReplayGainEnabled(IsReplayGainEnabled);
                _playerService.Play(song);
            }
        }
        else
        {
            Stop();
        }
    }

    public event EventHandler? PlaybackEnded;
    public event EventHandler<PlayState>? PlaybackStateChanged;
    public event EventHandler<TimeSpan>? PositionChanged;
    public event EventHandler<Song?>? CurrentSongChanged;
    public event EventHandler<PlaybackMode>? PlaybackModeChanged;

    private void StartTrackingNewSong(Song? song)
    {
        _currentTrackedSong = song;
        _playStartTime = TimeSpan.Zero;
        _accumulatedPlayTime = TimeSpan.Zero;
        _hasTrackedPlay = false;
    }

    private void UpdateAccumulatedPlayTime()
    {
        if (_currentTrackedSong != null && _playerService.IsPlaying)
        {
            var currentPosition = _playerService.Position;
            var delta = currentPosition - _playStartTime;
            if (delta > TimeSpan.Zero)
            {
                _accumulatedPlayTime += delta;
            }
            _playStartTime = currentPosition;
        }
    }

    private async Task TrackPlayIfThresholdMetAsync()
    {
        if (_currentTrackedSong == null || _hasTrackedPlay)
            return;

        UpdateAccumulatedPlayTime();

        var duration = _currentTrackedSong.Duration;
        if (duration.TotalSeconds <= 0)
            return;

        var playPercentage = _accumulatedPlayTime.TotalSeconds / duration.TotalSeconds;

        if (playPercentage >= PlayThresholdPercentage)
        {
            _hasTrackedPlay = true;
            await _statisticsService.TrackPlayAsync(_currentTrackedSong, _accumulatedPlayTime);
        }
    }

    public void Dispose()
    {
        _ = TrackPlayIfThresholdMetAsync();
        _positionTimer.Stop();

        _playerService.PlaybackEnded -= OnPlaybackEnded;
        _playerService.PlaybackStateChanged -= OnPlaybackStateChanged;
        _playerService.PositionChanged -= OnPositionChanged;

        _playlistService.CurrentSongChanged -= OnCurrentSongChanged;
        _playlistService.PlaybackModeChanged -= OnPlaybackModeChanged;
    }
}
