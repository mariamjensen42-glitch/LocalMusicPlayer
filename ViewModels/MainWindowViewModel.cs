using ReactiveUI;
using System.Reactive;

namespace LocalMusicPlayer.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public string Greeting { get; } = "Welcome to Avalonia with ReactiveUI!";

    public ReactiveCommand<Unit, Unit> TestCommand { get; }

    public MainWindowViewModel()
    {
        TestCommand = ReactiveCommand.Create(() => { });
    }
}
