using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;

namespace LocalMusicPlayer.Controls;

public partial class AlbumArtControl : UserControl, IDisposable
{
    public static readonly StyledProperty<byte[]?> ImageBytesProperty =
        AvaloniaProperty.Register<AlbumArtControl, byte[]?>(nameof(ImageBytes));

    public byte[]? ImageBytes
    {
        get => GetValue(ImageBytesProperty);
        set => SetValue(ImageBytesProperty, value);
    }

    public static readonly StyledProperty<bool> IsDarkProperty =
        AvaloniaProperty.Register<AlbumArtControl, bool>(nameof(IsDark), true);

    public bool IsDark
    {
        get => GetValue(IsDarkProperty);
        set => SetValue(IsDarkProperty, value);
    }

    public static readonly StyledProperty<double> CornerRadiusValueProperty =
        AvaloniaProperty.Register<AlbumArtControl, double>(nameof(CornerRadiusValue), 16.0);

    public double CornerRadiusValue
    {
        get => GetValue(CornerRadiusValueProperty);
        set => SetValue(CornerRadiusValueProperty, value);
    }

    public static readonly StyledProperty<bool> IsShadowEnabledProperty =
        AvaloniaProperty.Register<AlbumArtControl, bool>(nameof(IsShadowEnabled), true);

    public bool IsShadowEnabled
    {
        get => GetValue(IsShadowEnabledProperty);
        set => SetValue(IsShadowEnabledProperty, value);
    }

    public static readonly StyledProperty<bool> IsActiveProperty =
        AvaloniaProperty.Register<AlbumArtControl, bool>(nameof(IsActive), false);

    public bool IsActive
    {
        get => GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    private Bitmap? _currentBitmap;
    private Bitmap? _incomingBitmap;
    private bool _isFading;
    private CancellationTokenSource? _loadCts;
    private long _lastLength = -1;
    private int _lastHash;
    private bool _isDisposed;

    private static readonly TimeSpan FadeDuration = TimeSpan.FromMilliseconds(400);

    public AlbumArtControl()
    {
        InitializeComponent();

        ImageBytesProperty.Changed.AddClassHandler<AlbumArtControl>((x, _) => x.OnImageBytesChanged());
        IsDarkProperty.Changed.AddClassHandler<AlbumArtControl>((x, _) => x.OnIsDarkChanged());
        CornerRadiusValueProperty.Changed.AddClassHandler<AlbumArtControl>((x, _) => x.OnCornerRadiusChanged());
        IsShadowEnabledProperty.Changed.AddClassHandler<AlbumArtControl>((x, _) => x.OnShadowEnabledChanged());
        IsActiveProperty.Changed.AddClassHandler<AlbumArtControl>((x, _) => x.OnIsActiveChanged());

        Loaded += (_, _) => LoadDefaultCover();
    }

    private void OnImageBytesChanged()
    {
        if (!IsActive) return;

        var newBytes = ImageBytes;
        if (IsDuplicateAndUpdate(newBytes)) return;

        if (newBytes is { Length: > 0 })
            _ = LoadBitmapAsync(newBytes);
        else
            LoadDefaultCover();
    }

    private void OnIsDarkChanged()
    {
        InvalidateDedup();
        if (!IsActive) return;

        if (ImageBytes is { Length: > 0 })
            _ = LoadBitmapAsync(ImageBytes);
        else
            LoadDefaultCover();
    }

    private void OnCornerRadiusChanged()
    {
        var radius = CornerRadiusValue;
        CurrentImageBorder.CornerRadius = new CornerRadius(radius);
        IncomingImageBorder.CornerRadius = new CornerRadius(radius);
        DefaultCoverBorder.CornerRadius = new CornerRadius(radius);
    }

    private void OnShadowEnabledChanged()
    {
        var opacity = IsShadowEnabled ? 0.6 : 0;
        if (CurrentImageBorder.Effect is DropShadowEffect se1)
            se1.Opacity = opacity;
        if (IncomingImageBorder.Effect is DropShadowEffect se2)
            se2.Opacity = opacity;
        if (DefaultCoverBorder.Effect is DropShadowEffect se3)
            se3.Opacity = opacity;
    }

    private void OnIsActiveChanged()
    {
        if (IsActive)
        {
            if (ImageBytes is { Length: > 0 } bytes)
                _ = LoadBitmapAsync(bytes);
            else
                LoadDefaultCover();
        }
        else
        {
            _loadCts?.Cancel();
            _isFading = false;
        }
    }

    public bool IsDuplicateAndUpdate(byte[]? newBytes)
    {
        if (newBytes is not { Length: > 0 })
        {
            bool wasEmpty = _lastLength == 0;
            _lastLength = 0;
            _lastHash = 0;
            return wasEmpty;
        }

        int hash = ComputeFastHash(newBytes);
        if (newBytes.Length == _lastLength && hash == _lastHash)
            return true;

        _lastLength = newBytes.Length;
        _lastHash = hash;
        return false;
    }

    public void InvalidateDedup()
    {
        _lastLength = -1;
        _lastHash = 0;
    }

    private static int ComputeFastHash(byte[] data)
    {
        unchecked
        {
            const int sampleStep = 4096;
            int hash = 17;
            for (int i = 0; i < data.Length; i += sampleStep)
            {
                hash = hash * 31 + data[i];
            }
            hash = hash * 31 + data.Length;
            return hash;
        }
    }

    public async Task LoadBitmapAsync(byte[] imageBytes)
    {
        _loadCts?.Cancel();
        var cts = new CancellationTokenSource();
        _loadCts = cts;

        try
        {
            var bitmap = await Task.Run(() =>
            {
                cts.Token.ThrowIfCancellationRequested();
                using var stream = new MemoryStream(imageBytes, writable: false);
                return new Bitmap(stream);
            }, cts.Token);

            cts.Token.ThrowIfCancellationRequested();

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                StartTransition(bitmap);
            });
        }
        catch (OperationCanceledException) { }
        catch (Exception)
        {
            InvalidateDedup();
            LoadDefaultCover();
        }
        finally
        {
            if (ReferenceEquals(_loadCts, cts)) _loadCts = null;
            cts.Dispose();
        }
    }

    private void LoadDefaultCover()
    {
        try
        {
            var uri = new Uri("avares://LocalMusicPlayer/Assets/default-album-art.png");
            using var stream = Avalonia.Platform.AssetLoader.Open(uri);
            var bitmap = new Bitmap(stream);

            DefaultCoverImage.Source = bitmap;
            DefaultCoverBorder.Opacity = 1;
            CurrentImageBorder.Opacity = 0;
            IncomingImageBorder.Opacity = 0;
        }
        catch { }
    }

    private void StartTransition(Bitmap newBitmap)
    {
        if (_isFading)
        {
            _incomingBitmap = newBitmap;
            return;
        }

        var oldBitmap = _currentBitmap;
        _currentBitmap = newBitmap;

        CurrentImage.Source = oldBitmap;
        IncomingImage.Source = newBitmap;

        CurrentImageBorder.Opacity = 1;
        IncomingImageBorder.Opacity = 0;
        DefaultCoverBorder.Opacity = 0;

        _isFading = true;
        RunCrossFade();
    }

    private async void RunCrossFade()
    {
        try
        {
            var fadeOut = new Animation
            {
                Duration = FadeDuration,
                Easing = new CubicEaseOut(),
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(0d),
                        Setters = { new Setter(OpacityProperty, 1.0) }
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(1d),
                        Setters = { new Setter(OpacityProperty, 0.0) }
                    }
                }
            };

            var fadeIn = new Animation
            {
                Duration = FadeDuration,
                Easing = new CubicEaseOut(),
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(0d),
                        Setters = { new Setter(OpacityProperty, 0.0) }
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(1d),
                        Setters = { new Setter(OpacityProperty, 1.0) }
                    }
                }
            };

            await Task.WhenAll(
                fadeOut.RunAsync(CurrentImageBorder),
                fadeIn.RunAsync(IncomingImageBorder)
            );

            CurrentImageBorder.Opacity = 0;
            IncomingImageBorder.Opacity = 1;

            CurrentImage.Source = _currentBitmap;
            IncomingImage.Source = null;

            (CurrentImageBorder, IncomingImageBorder) = (IncomingImageBorder, CurrentImageBorder);
            (CurrentImage, IncomingImage) = (IncomingImage, CurrentImage);

            _isFading = false;

            if (_incomingBitmap != null)
            {
                var next = _incomingBitmap;
                _incomingBitmap = null;
                StartTransition(next);
            }
        }
        catch { }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed) return;
        if (!disposing) return;

        _loadCts?.Cancel();
        _loadCts?.Dispose();
        _loadCts = null;

        _currentBitmap?.Dispose();
        _currentBitmap = null;
        _incomingBitmap?.Dispose();
        _incomingBitmap = null;

        _isDisposed = true;
    }
}
