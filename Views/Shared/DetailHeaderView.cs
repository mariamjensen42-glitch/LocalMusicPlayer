using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;

namespace LocalMusicPlayer.Views.Shared;

public partial class DetailHeaderView : UserControl
{
    public DetailHeaderView()
    {
        InitializeComponent();
    }

    public static readonly DirectProperty<DetailHeaderView, string> TitleProperty =
        AvaloniaProperty.RegisterDirect<DetailHeaderView, string>(
            nameof(Title),
            o => o.Title,
            (o, v) => o.Title = v);

    public static readonly DirectProperty<DetailHeaderView, string> SubtitleProperty =
        AvaloniaProperty.RegisterDirect<DetailHeaderView, string>(
            nameof(Subtitle),
            o => o.Subtitle,
            (o, v) => o.Subtitle = v);

    public static readonly DirectProperty<DetailHeaderView, object> CoverImageSourceProperty =
        AvaloniaProperty.RegisterDirect<DetailHeaderView, object>(
            nameof(CoverImageSource),
            o => o.CoverImageSource,
            (o, v) => o.CoverImageSource = v);

    public static readonly DirectProperty<DetailHeaderView, ICommand?> PlayAllCommandProperty =
        AvaloniaProperty.RegisterDirect<DetailHeaderView, ICommand?>(
            nameof(PlayAllCommand),
            o => o.PlayAllCommand,
            (o, v) => o.PlayAllCommand = v);

    public static readonly DirectProperty<DetailHeaderView, ICommand?> ShufflePlayCommandProperty =
        AvaloniaProperty.RegisterDirect<DetailHeaderView, ICommand?>(
            nameof(ShufflePlayCommand),
            o => o.ShufflePlayCommand,
            (o, v) => o.ShufflePlayCommand = v);

    private string _title = "";
    private string _subtitle = "";
    private object _coverImageSource = null!;
    private ICommand? _playAllCommand;
    private ICommand? _shufflePlayCommand;

    public string Title
    {
        get => _title;
        set => SetAndRaise(TitleProperty, ref _title, value);
    }

    public string Subtitle
    {
        get => _subtitle;
        set => SetAndRaise(SubtitleProperty, ref _subtitle, value);
    }

    public object CoverImageSource
    {
        get => _coverImageSource;
        set => SetAndRaise(CoverImageSourceProperty, ref _coverImageSource, value);
    }

    public ICommand? PlayAllCommand
    {
        get => _playAllCommand;
        set => SetAndRaise(PlayAllCommandProperty, ref _playAllCommand, value);
    }

    public ICommand? ShufflePlayCommand
    {
        get => _shufflePlayCommand;
        set => SetAndRaise(ShufflePlayCommandProperty, ref _shufflePlayCommand, value);
    }
}
