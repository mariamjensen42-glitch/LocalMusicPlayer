using Avalonia;
using Avalonia.Controls.Primitives;

namespace LocalMusicPlayer.Behaviors;

public class ClickBehavior : TemplatedControl
{
    public static readonly StyledProperty<int> ClickCountProperty =
        AvaloniaProperty.Register<ClickBehavior, int>(nameof(ClickCount));

    public int ClickCount
    {
        get => GetValue(ClickCountProperty);
        set => SetValue(ClickCountProperty, value);
    }
}
