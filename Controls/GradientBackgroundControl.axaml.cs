using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using Color = Avalonia.Media.Color;

namespace LocalMusicPlayer.Controls;

public partial class GradientBackgroundControl : UserControl, IDisposable
{
    public static readonly StyledProperty<bool> IsBackgroundEnableProperty =
        AvaloniaProperty.Register<GradientBackgroundControl, bool>(nameof(IsBackgroundEnable), false);

    public bool IsBackgroundEnable
    {
        get => GetValue(IsBackgroundEnableProperty);
        set => SetValue(IsBackgroundEnableProperty, value);
    }

    public static readonly StyledProperty<byte[]?> ImageBytesProperty =
        AvaloniaProperty.Register<GradientBackgroundControl, byte[]?>(nameof(ImageBytes));

    public byte[]? ImageBytes
    {
        get => GetValue(ImageBytesProperty);
        set => SetValue(ImageBytesProperty, value);
    }

    public static readonly StyledProperty<bool> IsDarkProperty =
        AvaloniaProperty.Register<GradientBackgroundControl, bool>(nameof(IsDark), true);

    public bool IsDark
    {
        get => GetValue(IsDarkProperty);
        set => SetValue(IsDarkProperty, value);
    }

    public static readonly StyledProperty<bool> UseImageDominantThemeProperty =
        AvaloniaProperty.Register<GradientBackgroundControl, bool>(nameof(UseImageDominantTheme), false);

    public bool UseImageDominantTheme
    {
        get => GetValue(UseImageDominantThemeProperty);
        set => SetValue(UseImageDominantThemeProperty, value);
    }

    public event EventHandler<bool>? ThemeResolved;

    private CancellationTokenSource? _loadCts;
    private bool _isDisposed;
    private readonly Random _random = new();

    private Color[] _currentColors = [Colors.Transparent, Colors.Transparent, Colors.Transparent, Colors.Transparent];
    private Color[] _targetColors = [Colors.Transparent, Colors.Transparent, Colors.Transparent, Colors.Transparent];

    private readonly Ellipse[] _blobs;

    public GradientBackgroundControl()
    {
        InitializeComponent();

        _blobs = [Blob1, Blob2, Blob3, Blob4];

        ImageBytesProperty.Changed.AddClassHandler<GradientBackgroundControl>((x, _) => x.OnImageBytesChanged());
        IsDarkProperty.Changed.AddClassHandler<GradientBackgroundControl>((x, _) => x.OnIsDarkChanged());
        IsBackgroundEnableProperty.Changed.AddClassHandler<GradientBackgroundControl>((x, _) => x.OnIsBackgroundEnableChanged());

        Loaded += (_, _) => InitializePositions();
    }

    private void InitializePositions()
    {
        PositionBlob(Blob1, 0.15, 0.2);
        PositionBlob(Blob2, 0.7, 0.15);
        PositionBlob(Blob3, 0.3, 0.75);
        PositionBlob(Blob4, 0.8, 0.7);

        ApplyDefaultColors();
        UpdateDarkOverlay();
    }

    private void PositionBlob(Ellipse blob, double xRatio, double yRatio)
    {
        Canvas.SetLeft(blob, Bounds.Width * xRatio - blob.Width / 2);
        Canvas.SetTop(blob, Bounds.Height * yRatio - blob.Height / 2);
    }

    private void OnImageBytesChanged()
    {
        if (ImageBytes is { Length: > 0 } bytes)
            _ = LoadImageFromBytesAsync(bytes);
        else
            ApplyDefaultColors();
    }

    private void OnIsDarkChanged()
    {
        UpdateDarkOverlay();
        if (ImageBytes is { Length: > 0 } bytes)
            _ = LoadImageFromBytesAsync(bytes);
        else
            ApplyDefaultColors();
    }

    private void OnIsBackgroundEnableChanged()
    {
        UpdateDarkOverlay();
    }

    private void UpdateDarkOverlay()
    {
        DarkOverlay.IsVisible = IsDark;
        DarkOverlay.Opacity = IsDark ? 0.55 : 0.3;

        if (IsDark)
            DarkOverlay.Background = new SolidColorBrush(Color.FromRgb(0, 0, 0));
        else
            DarkOverlay.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
    }

    private void ApplyDefaultColors()
    {
        bool isDark = IsDark;
        _targetColors = isDark
            ? [
                Color.FromRgb(20, 20, 20),
                Color.FromRgb(64, 71, 69),
                Color.FromRgb(18, 18, 18),
                Color.FromRgb(13, 13, 13)
            ]
            : [
                Color.FromRgb(242, 242, 242),
                Color.FromRgb(179, 173, 181),
                Color.FromRgb(204, 204, 204),
                Color.FromRgb(217, 217, 217)
            ];

        AnimateToColors(_targetColors);
    }

    private async Task LoadImageFromBytesAsync(byte[] imageBytes)
    {
        _loadCts?.Cancel();
        _loadCts?.Dispose();
        var cts = new CancellationTokenSource();
        _loadCts = cts;

        try
        {
            var result = await Task.Run(() =>
            {
                cts.Token.ThrowIfCancellationRequested();
                using var stream = new MemoryStream(imageBytes, writable: false);
                var bitmap = new Bitmap(stream);

                int sampleSize = 150;
                int targetW = Math.Min(sampleSize, bitmap.PixelSize.Width);
                int targetH = Math.Min(sampleSize, bitmap.PixelSize.Height);

                var resized = bitmap.CreateScaledBitmap(new PixelSize(targetW, targetH));

                int stride = targetW * 4;
                byte[] pixels = new byte[stride * targetH];
                var srcRect = new Avalonia.PixelRect(0, 0, targetW, targetH);
                var handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
                try
                {
                    resized.CopyPixels(srcRect, handle.AddrOfPinnedObject(), stride * targetH, stride);
                }
                finally
                {
                    handle.Free();
                }

                var palette = ExtractDominantColors(pixels, targetW, targetH, 4);

                bool effectiveIsDark = UseImageDominantTheme
                    ? IsImageDark(pixels, targetW, targetH)
                    : IsDark;

                ScalePaletteLuminance(palette, effectiveIsDark);

                var colors = palette.Select(c => Color.FromRgb(
                    (byte)(c.R * 255),
                    (byte)(c.G * 255),
                    (byte)(c.B * 255))).ToArray();

                return new Tuple<Color[], bool>(colors, effectiveIsDark);
            }, cts.Token);

            cts.Token.ThrowIfCancellationRequested();

            var extractedColors = result.Item1;
            var resolvedIsDark = result.Item2;

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                _targetColors = extractedColors;
                AnimateToColors(_targetColors);

                if (UseImageDominantTheme)
                    ThemeResolved?.Invoke(this, resolvedIsDark);
            });
        }
        catch (OperationCanceledException) { }
        catch (Exception)
        {
            ApplyDefaultColors();
        }
        finally
        {
            if (ReferenceEquals(_loadCts, cts))
            {
                cts.Dispose();
                _loadCts = null;
            }
        }
    }

    private void AnimateToColors(Color[] targetColors)
    {
        for (int i = 0; i < 4 && i < targetColors.Length; i++)
        {
            var fill = new SolidColorBrush(_currentColors[i]);
            _blobs[i].Fill = fill;

            var animation = new Animation
            {
                Duration = TimeSpan.FromMilliseconds(800),
                Easing = new CubicEaseOut(),
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(0d),
                        Setters = { new Setter(SolidColorBrush.ColorProperty, _currentColors[i]) }
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(1d),
                        Setters = { new Setter(SolidColorBrush.ColorProperty, targetColors[i]) }
                    }
                }
            };

            animation.RunAsync(fill);

            _currentColors[i] = targetColors[i];
        }

        AnimateBlobPositions();
    }

    private void AnimateBlobPositions()
    {
        for (int i = 0; i < _blobs.Length; i++)
        {
            double targetX = _random.Next(0, (int)Math.Max(1, Bounds.Width - _blobs[i].Width));
            double targetY = _random.Next(0, (int)Math.Max(1, Bounds.Height - _blobs[i].Height));

            var currentX = Canvas.GetLeft(_blobs[i]);
            var currentY = Canvas.GetTop(_blobs[i]);

            if (double.IsNaN(currentX)) currentX = 0;
            if (double.IsNaN(currentY)) currentY = 0;

            var animX = new Animation
            {
                Duration = TimeSpan.FromSeconds(3 + _random.NextDouble() * 2),
                Easing = new CubicEaseInOut(),
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(0d),
                        Setters = { new Setter(Canvas.LeftProperty, currentX) }
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(1d),
                        Setters = { new Setter(Canvas.LeftProperty, targetX) }
                    }
                }
            };

            var animY = new Animation
            {
                Duration = TimeSpan.FromSeconds(3 + _random.NextDouble() * 2),
                Easing = new CubicEaseInOut(),
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(0d),
                        Setters = { new Setter(Canvas.TopProperty, currentY) }
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(1d),
                        Setters = { new Setter(Canvas.TopProperty, targetY) }
                    }
                }
            };

            animX.RunAsync(_blobs[i]);
            animY.RunAsync(_blobs[i]);
        }
    }

    private static List<(double R, double G, double B, int Population)> ExtractDominantColors(
        byte[] pixels, int width, int height, int count)
    {
        var buckets = new Dictionary<int, (double R, double G, double B, int Count)>();

        for (int i = 0; i < pixels.Length; i += 4)
        {
            byte r = pixels[i + 2];
            byte g = pixels[i + 1];
            byte b = pixels[i];

            int quantR = r / 32;
            int quantG = g / 32;
            int quantB = b / 32;
            int key = quantR * 64 + quantG * 8 + quantB;

            if (buckets.TryGetValue(key, out var existing))
            {
                buckets[key] = (existing.R + r, existing.G + g, existing.B + b, existing.Count + 1);
            }
            else
            {
                buckets[key] = (r, g, b, 1);
            }
        }

        var sorted = buckets.Values
            .OrderByDescending(b => b.Count)
            .Take(count * 3)
            .Select(b => (R: b.R / b.Count / 255.0, G: b.G / b.Count / 255.0, B: b.B / b.Count / 255.0, b.Count))
            .ToList();

        var result = new List<(double R, double G, double B, int Population)>();
        foreach (var color in sorted)
        {
            bool tooSimilar = result.Any(existing =>
            {
                double dr = existing.R - color.R;
                double dg = existing.G - color.G;
                double db = existing.B - color.B;
                return Math.Sqrt(dr * dr + dg * dg + db * db) < 0.25;
            });

            if (!tooSimilar)
                result.Add(color);

            if (result.Count >= count)
                break;
        }

        while (result.Count < count)
        {
            result.Add((0.5, 0.5, 0.5, 1));
        }

        return result;
    }

    private static bool IsImageDark(byte[] pixels, int width, int height)
    {
        long totalBrightness = 0;
        int pixelCount = width * height;

        for (int i = 0; i < pixels.Length; i += 4)
        {
            totalBrightness += (pixels[i + 2] * 299 + pixels[i + 1] * 587 + pixels[i] * 114) / 1000;
        }

        double avgBrightness = (double)totalBrightness / pixelCount;
        return avgBrightness < 128;
    }

    private static void ScalePaletteLuminance(List<(double R, double G, double B, int Population)> palette, bool isDark)
    {
        float targetAvg = isDark ? 0.45f : 0.55f;

        double totalPop = palette.Sum(p => p.Population);
        double avgL = palette.Sum(p =>
        {
            RgbToHsl(p.R, p.G, p.B, out _, out _, out float l);
            return l * p.Population;
        }) / totalPop;

        if (isDark && avgL <= targetAvg) return;
        if (!isDark && avgL >= targetAvg) return;

        float shift = (float)(targetAvg - avgL);
        float clampMin = isDark ? 0f : 0.3f;
        float clampMax = isDark ? 0.7f : 1f;

        for (int i = 0; i < palette.Count; i++)
        {
            var (r, g, b, pop) = palette[i];
            RgbToHsl(r, g, b, out float h, out float s, out float l);

            if (isDark && l < 0.3f) continue;
            if (!isDark && l > 0.7f) continue;

            float newL = Math.Clamp(l + shift, clampMin, clampMax);
            var (nr, ng, nb) = HslToRgb(h, s, newL);
            palette[i] = (nr, ng, nb, pop);
        }
    }

    private static void RgbToHsl(double r, double g, double b, out float h, out float s, out float l)
    {
        double max = Math.Max(r, Math.Max(g, b));
        double min = Math.Min(r, Math.Min(g, b));
        double delta = max - min;
        l = (float)((max + min) * 0.5);
        if (delta < 1e-5) { h = 0f; s = 0f; return; }
        s = l > 0.5f ? (float)(delta / (2.0 - max - min)) : (float)(delta / (max + min));
        if (max == r) h = (float)((g - b) / delta + (g < b ? 6.0 : 0.0)) / 6f;
        else if (max == g) h = (float)((b - r) / delta + 2.0) / 6f;
        else h = (float)((r - g) / delta + 4.0) / 6f;
    }

    private static (double R, double G, double B) HslToRgb(float h, float s, float l)
    {
        if (s < 1e-5f) return (l, l, l);
        double q = l < 0.5f ? l * (1f + s) : l + s - l * s;
        double p = 2f * l - q;
        return (HueToRgb(p, q, h + 1f / 3f), HueToRgb(p, q, h), HueToRgb(p, q, h - 1f / 3f));
    }

    private static double HueToRgb(double p, double q, float t)
    {
        if (t < 0f) t += 1f;
        if (t > 1f) t -= 1f;
        if (t < 1f / 6f) return p + (q - p) * 6f * t;
        if (t < 1f / 2f) return q;
        if (t < 2f / 3f) return p + (q - p) * (2f / 3f - t) * 6f;
        return p;
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
        _isDisposed = true;
    }
}
