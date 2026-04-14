using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using LocalMusicPlayer.ViewModels;

namespace LocalMusicPlayer.Views.Player;

public partial class PlayerPageView : UserControl
{
    private Popup? _speedPopup;
    private Popup? _speedPopupNarrow;

    public PlayerPageView()
    {
        InitializeComponent();

        DataContextChanged += (_, _) =>
        {
            if (DataContext is PlayerPageViewModel viewModel)
            {
                _speedPopup = this.FindControl<Popup>("SpeedPopup");
                _speedPopupNarrow = this.FindControl<Popup>("SpeedPopupNarrow");
            }
        };
    }

    public void OnSpeedButtonClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        TogglePopup(_speedPopup);
        TogglePopup(_speedPopupNarrow);
    }

    private static void TogglePopup(Popup? popup)
    {
        if (popup != null)
        {
            popup.IsOpen = !popup.IsOpen;
        }
    }
}
