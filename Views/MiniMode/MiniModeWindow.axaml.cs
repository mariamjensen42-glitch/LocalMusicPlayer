using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace LocalMusicPlayer.Views.MiniMode;

public partial class MiniModeWindow : Window
{
    public MiniModeWindow()
    {
        InitializeComponent();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(e);
        }
        base.OnPointerPressed(e);
    }
}
