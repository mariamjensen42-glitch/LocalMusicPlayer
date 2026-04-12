using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using LocalMusicPlayer.ViewModels;

namespace LocalMusicPlayer.Views.Player;

public partial class PlayerPageView : UserControl
{
    private Popup? _speedPopup;

    public PlayerPageView()
    {
        InitializeComponent();

        DataContextChanged += (_, _) =>
        {
            if (DataContext is PlayerPageViewModel viewModel)
            {
                _speedPopup = this.FindControl<Popup>("SpeedPopup");
            }
        };
    }

    public void OnSpeedButtonClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_speedPopup != null)
        {
            _speedPopup.IsOpen = !_speedPopup.IsOpen;
        }
    }
}
