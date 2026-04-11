using Avalonia;
using Avalonia.Controls.Primitives;

namespace LocalMusicPlayer.Behaviors;

public class ClickAttachedProperty : TemplatedControl
{
    public static readonly StyledProperty<int> ClickCountProperty =
        AvaloniaProperty.Register<ClickAttachedProperty, int>(nameof(ClickCount));

    public int ClickCount
    {
        get => GetValue(ClickCountProperty);
        set => SetValue(ClickCountProperty, value);
    }
}
