using System;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;

namespace LocalMusicPlayer.Controls;

public partial class LyricsLineControl : UserControl
{
    public static readonly StyledProperty<string> LyricsTextProperty =
        AvaloniaProperty.Register<LyricsLineControl, string>(nameof(LyricsText), string.Empty);

    public string LyricsText
    {
        get => GetValue(LyricsTextProperty);
        set => SetValue(LyricsTextProperty, value);
    }

    public static readonly StyledProperty<string> TranslateTextProperty =
        AvaloniaProperty.Register<LyricsLineControl, string>(nameof(TranslateText), string.Empty);

    public string TranslateText
    {
        get => GetValue(TranslateTextProperty);
        set => SetValue(TranslateTextProperty, value);
    }

    public static readonly StyledProperty<bool> IsTranslationVisibleProperty =
        AvaloniaProperty.Register<LyricsLineControl, bool>(nameof(IsTranslationVisible), false);

    public bool IsTranslationVisible
    {
        get => GetValue(IsTranslationVisibleProperty);
        set => SetValue(IsTranslationVisibleProperty, value);
    }

    public static readonly StyledProperty<double> LyricsFontSizeProperty =
        AvaloniaProperty.Register<LyricsLineControl, double>(nameof(LyricsFontSize), 32.0);

    public double LyricsFontSize
    {
        get => GetValue(LyricsFontSizeProperty);
        set => SetValue(LyricsFontSizeProperty, value);
    }

    public static readonly StyledProperty<double> TranslateFontSizeProperty =
        AvaloniaProperty.Register<LyricsLineControl, double>(nameof(TranslateFontSize), 24.0);

    public double TranslateFontSize
    {
        get => GetValue(TranslateFontSizeProperty);
        set => SetValue(TranslateFontSizeProperty, value);
    }

    public static readonly StyledProperty<bool> IsCurrentLineProperty =
        AvaloniaProperty.Register<LyricsLineControl, bool>(nameof(IsCurrentLine), false);

    public bool IsCurrentLine
    {
        get => GetValue(IsCurrentLineProperty);
        set => SetValue(IsCurrentLineProperty, value);
    }

    public static readonly StyledProperty<TimeSpan> LineAnimateDurationProperty =
        AvaloniaProperty.Register<LyricsLineControl, TimeSpan>(nameof(LineAnimateDuration), TimeSpan.Zero);

    public TimeSpan LineAnimateDuration
    {
        get => GetValue(LineAnimateDurationProperty);
        set => SetValue(LineAnimateDurationProperty, value);
    }

    public static readonly StyledProperty<bool> IsWFWLyricsProperty =
        AvaloniaProperty.Register<LyricsLineControl, bool>(nameof(IsWFWLyrics), true);

    public bool IsWFWLyrics
    {
        get => GetValue(IsWFWLyricsProperty);
        set => SetValue(IsWFWLyricsProperty, value);
    }

    public static readonly StyledProperty<IBrush> ActiveForegroundProperty =
        AvaloniaProperty.Register<LyricsLineControl, IBrush>(nameof(ActiveForeground), Brushes.White);

    public IBrush ActiveForeground
    {
        get => GetValue(ActiveForegroundProperty);
        set => SetValue(ActiveForegroundProperty, value);
    }

    public static readonly StyledProperty<IBrush> InactiveForegroundProperty =
        AvaloniaProperty.Register<LyricsLineControl, IBrush>(nameof(InactiveForeground),
            new SolidColorBrush(Avalonia.Media.Color.FromRgb(161, 161, 170)));

    public IBrush InactiveForeground
    {
        get => GetValue(InactiveForegroundProperty);
        set => SetValue(InactiveForegroundProperty, value);
    }

    private Animation? _currentAnimation;

    public LyricsLineControl()
    {
        InitializeComponent();

        LyricsTextProperty.Changed.AddClassHandler<LyricsLineControl>((x, _) => x.UpdateText());
        TranslateTextProperty.Changed.AddClassHandler<LyricsLineControl>((x, _) => x.UpdateTranslationText());
        IsTranslationVisibleProperty.Changed.AddClassHandler<LyricsLineControl>((x, _) => x.UpdateTranslationVisibility());
        LyricsFontSizeProperty.Changed.AddClassHandler<LyricsLineControl>((x, _) => x.UpdateFontSizes());
        TranslateFontSizeProperty.Changed.AddClassHandler<LyricsLineControl>((x, _) => x.UpdateFontSizes());
        IsCurrentLineProperty.Changed.AddClassHandler<LyricsLineControl>((x, _) => x.OnIsCurrentLineChanged());
        ActiveForegroundProperty.Changed.AddClassHandler<LyricsLineControl>((x, _) => x.UpdateForeground());
        InactiveForegroundProperty.Changed.AddClassHandler<LyricsLineControl>((x, _) => x.UpdateForeground());
    }

    private void UpdateText()
    {
        LyricsTextBlockBase.Text = LyricsText;
        LyricsTextBlock.Text = LyricsText;
    }

    private void UpdateTranslationText()
    {
        LyricsTransTextBlock.Text = TranslateText;
    }

    private void UpdateTranslationVisibility()
    {
        LyricsTransTextBlock.IsVisible = IsTranslationVisible;
    }

    private void UpdateFontSizes()
    {
        LyricsTextBlockBase.FontSize = LyricsFontSize;
        LyricsTextBlock.FontSize = LyricsFontSize;
        LyricsTransTextBlock.FontSize = TranslateFontSize;
    }

    private void UpdateForeground()
    {
        var baseBrush = IsCurrentLine ? ActiveForeground : InactiveForeground;
        LyricsTextBlockBase.Foreground = InactiveForeground;
        LyricsTextBlock.Foreground = baseBrush;
    }

    private void OnIsCurrentLineChanged()
    {
        UpdateForeground();

        if (IsCurrentLine)
        {
            StartClipAnimation();
        }
        else
        {
            CancelCurrentAnimation();
            HighlightContainer.Width = double.NaN;
        }
    }

    private void StartClipAnimation()
    {
        CancelCurrentAnimation();

        if (!IsWFWLyrics) return;

        var duration = LineAnimateDuration;
        if (duration <= TimeSpan.Zero) return;

        LyricsTextBlock.Measure(new Avalonia.Size(double.PositiveInfinity, double.PositiveInfinity));
        double targetWidth = LyricsTextBlock.DesiredSize.Width;

        if (targetWidth <= 0) return;

        HighlightContainer.Width = 0;

        var animation = new Animation
        {
            Duration = duration,
            Easing = new LinearEasing(),
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0d),
                    Setters = { new Setter(WidthProperty, 0d) }
                },
                new KeyFrame
                {
                    Cue = new Cue(1d),
                    Setters = { new Setter(WidthProperty, targetWidth) }
                }
            }
        };

        _currentAnimation = animation;
        animation.RunAsync(HighlightContainer);
    }

    private void CancelCurrentAnimation()
    {
        _currentAnimation = null;
    }
}
