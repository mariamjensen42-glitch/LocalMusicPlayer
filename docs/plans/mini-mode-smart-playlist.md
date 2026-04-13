# Mini Mode & Smart Playlist Implementation Plan

> **For Hermes:** Use subagent-driven-development skill to implement this plan task-by-task.

**Goal:** 为 LocalMusicPlayer 添加两个功能：(1) 迷你模式 — 精简悬浮播放器；(2) 智能播放列表 — 按规则自动生成。

**Architecture:**
- **迷你模式**：在 MainWindow 底部播放栏旁添加"迷你模式"按钮，点击后弹出可拖拽的紧凑悬浮窗（封面+标题+播放控制），置顶显示。再点一次关闭。
- **智能播放列表**：新增 `SmartPlaylist` 类型，通过规则（播放次数最多、最近播放等）动态生成歌曲列表，存储于 `UserPlaylist`，在侧边栏"智能播放列表"分类下展示。

**Tech Stack:** .NET 9 + Avalonia UI + CommunityToolkit.Mvvm + LibVLCSharp

---

## Feature 1: Mini Mode (迷你模式)

### Task 1: Add `IsMiniMode` observable property and toggle command to MainWindowViewModel

**Files:**
- Modify: `ViewModels/MainWindowViewModel.cs`

**Step 1: Add property**

In `MainWindowViewModel`, add:
```csharp
[ObservableProperty] private bool _isMiniMode;

[RelayCommand]
private void ToggleMiniMode()
{
    IsMiniMode = !IsMiniMode;
}
```

Also add `OnPropertyChanged(nameof(IsMiniMode))` to `OnPlaybackStateChanged`, `OnCurrentSongChanged`, `OnPositionChanged`.

**Step 2: Commit**

```bash
git add ViewModels/MainWindowViewModel.cs
git commit -m "feat: add IsMiniMode toggle to MainWindowViewModel"
```

---

### Task 2: Create MiniModeWindow XAML view

**Files:**
- Create: `Views/MiniMode/MiniModeWindow.axaml`
- Create: `Views/MiniMode/MiniModeWindow.axaml.cs`

**Step 1: Create MiniModeWindow.axaml**

```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:vm="using:LocalMusicPlayer.ViewModels"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        mc:Ignorable="d" d:DesignWidth="320" d:DesignHeight="90"
        Width="320" Height="90"
        x:Class="LocalMusicPlayer.Views.MiniMode.MiniModeWindow"
        x:DataType="vm:MainWindowViewModel"
        WindowStyle="None"
        Background="Transparent"
        CanResize="False"
        Topmost="True"
        ShowInTaskbar="False"
        Decorations="None"
        ExtendClientAreaToDecorationsHint="True"
        ExtendClientAreaChromeHints="NoChrome">
    <Border Background="{StaticResource BgCardBrush}"
            CornerRadius="12"
            Margin="8"
            BoxShadow="0 4 16 0 #40000000">
        <Grid ColumnDefinitions="Auto,*,Auto" Margin="12,8">
            <!-- Album Art -->
            <Border Grid.Column="0" Width="56" Height="56" CornerRadius="6" Margin="0,0,12,0">
                <Panel>
                    <Image Source="{Binding CurrentSong.AlbumArtPath}"
                           IsVisible="{Binding CurrentSong.HasAlbumArt}"
                           Stretch="UniformToFill" />
                    <Border Background="{StaticResource BgSecondaryBrush}"
                            IsVisible="{Binding !CurrentSong.HasAlbumArt}"
                            CornerRadius="6">
                        <TextBlock Text="&#xE8D6;"
                                   FontFamily="Segoe Fluent Icons"
                                   FontSize="24"
                                   Foreground="{StaticResource TextMutedBrush}"
                                   HorizontalAlignment="Center"
                                   VerticalAlignment="Center" />
                    </Border>
                </Panel>
            </Border>

            <!-- Title & Artist -->
            <StackPanel Grid.Column="1" VerticalAlignment="Center" Spacing="2">
                <TextBlock Text="{Binding CurrentSong.Title, FallbackValue='Not Playing'}"
                           FontSize="14" FontWeight="SemiBold"
                           Foreground="{StaticResource TextPrimaryBrush}"
                           TextTrimming="CharacterEllipsis" />
                <TextBlock Text="{Binding CurrentSong.Artist, FallbackValue='-'}"
                           FontSize="12"
                           Foreground="{StaticResource TextMutedBrush}"
                           TextTrimming="CharacterEllipsis" />
            </StackPanel>

            <!-- Controls -->
            <StackPanel Grid.Column="2" Orientation="Horizontal" Spacing="4" VerticalAlignment="Center">
                <Button Command="{Binding PauseCommand}" Width="32" Height="32"
                        Background="Transparent" CornerRadius="16"
                        IsVisible="{Binding IsPlaying}">
                    <TextBlock Text="&#xE769;" FontFamily="Segoe Fluent Icons" FontSize="14"
                               Foreground="{StaticResource TextPrimaryBrush}" />
                </Button>
                <Button Command="{Binding PlayCommand}" Width="32" Height="32"
                        Background="Transparent" CornerRadius="16"
                        IsVisible="{Binding !IsPlaying}">
                    <TextBlock Text="&#xE768;" FontFamily="Segoe Fluent Icons" FontSize="14"
                               Foreground="{StaticResource TextPrimaryBrush}" />
                </Button>
                <Button Command="{Binding NextCommand}" Width="32" Height="32"
                        Background="Transparent" CornerRadius="16">
                    <TextBlock Text="&#xE893;" FontFamily="Segoe Fluent Icons" FontSize="14"
                               Foreground="{StaticResource TextPrimaryBrush}" />
                </Button>
                <Button Command="{Binding ToggleMiniModeCommand}" Width="32" Height="32"
                        Background="Transparent" CornerRadius="16">
                    <TextBlock Text="&#xE8BB;" FontFamily="Segoe Fluent Icons" FontSize="14"
                               Foreground="{StaticResource TextMutedBrush}" />
                </Button>
            </StackPanel>
        </Grid>
    </Border>
</Window>
```

**Step 2: Create MiniModeWindow.axaml.cs**

```csharp
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

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
```

**Step 3: Commit**

```bash
git add Views/MiniMode/MiniModeWindow.axaml Views/MiniMode/MiniModeWindow.axaml.cs
git commit -m "feat: add MiniModeWindow XAML and code-behind"
```

---

### Task 3: Wire up MiniModeWindow open/close in MainWindowViewModel

**Files:**
- Modify: `ViewModels/MainWindowViewModel.cs`
- Modify: `Program.cs` or DI setup (check `Helpers/ServiceCollectionExtensions.cs`)

**Step 1: Add MiniModeWindow management**

In `MainWindowViewModel`, add:
```csharp
private Window? _miniModeWindow;

partial void OnIsMiniModeChanged(bool value)
{
    if (value)
    {
        if (_miniModeWindow == null)
        {
            _miniModeWindow = new MiniModeWindow();
            _miniModeWindow.DataContext = this;
            _miniModeWindow.Closed += (s, e) => { IsMiniMode = false; _miniModeWindow = null; };
            _miniModeWindow.Show();
        }
    }
    else
    {
        _miniModeWindow?.Close();
        _miniModeWindow = null;
    }
}
```

**Step 2: Commit**

```bash
git add ViewModels/MainWindowViewModel.cs
git commit -m "feat: wire up MiniModeWindow open/close via IsMiniMode property"
```

---

### Task 4: Add "Mini Mode" button to bottom player bar

**Files:**
- Modify: `Views/Main/MainWindow.axaml` (find bottom player bar section)

**Step 1: Add button**

In the bottom bar near the existing control buttons, add:
```xml
<Button Command="{Binding ToggleMiniModeCommand}"
        ToolTip.Tip="Mini Mode"
        Background="Transparent" Padding="8,4">
    <TextBlock Text="&#xE8A7;" FontFamily="Segoe Fluent Icons" FontSize="16"
               Foreground="{StaticResource TextSecondaryBrush}" />
</Button>
```

**Step 2: Commit**

```bash
git add Views/Main/MainWindow.axaml
git commit -m "feat: add mini mode button to bottom player bar"
```

---

## Feature 2: Smart Playlist (智能播放列表)

### Task 5: Add SmartPlaylist model

**Files:**
- Create: `Models/SmartPlaylist.cs`
- Modify: `Models/UserPlaylist.cs` — add `SmartPlaylistType` property (optional discriminator)

**Step 1: Create SmartPlaylist.cs**

```csharp
using System;

namespace LocalMusicPlayer.Models;

public enum SmartPlaylistRule
{
    MostPlayed,       // 播放次数最多
    RecentlyPlayed,   // 最近播放
    LeastPlayed,     // 播放次数最少
    RecentlyAdded,    // 最近添加
    NeverPlayed,     // 从未播放
}

public class SmartPlaylist
{
    public string Name { get; set; } = string.Empty;
    public SmartPlaylistRule Rule { get; set; }
    public int Limit { get; set; } = 50;  // 返回的最大歌曲数
    public DateTime? FromDate { get; set; }  // 用于 RecentlyPlayed 等的时间范围起点
    public DateTime? ToDate { get; set; }
}
```

**Step 2: Commit**

```bash
git add Models/SmartPlaylist.cs
git commit -m "feat: add SmartPlaylist model with rule types"
```

---

### Task 6: Create ISmartPlaylistService and implementation

**Files:**
- Create: `Services/SmartPlaylist/ISmartPlaylistService.cs`
- Create: `Services/SmartPlaylist/SmartPlaylistService.cs`

**Step 1: ISmartPlaylistService.cs**

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using LocalMusicPlayer.Models;

namespace LocalMusicPlayer.Services;

public interface ISmartPlaylistService
{
    IReadOnlyList<SmartPlaylist> GetSmartPlaylists();
    Task<List<Song>> GetSongsForSmartPlaylistAsync(SmartPlaylist smartPlaylist);
}
```

**Step 2: SmartPlaylistService.cs**

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LocalMusicPlayer.Models;

namespace LocalMusicPlayer.Services;

public class SmartPlaylistService : ISmartPlaylistService
{
    private readonly IMusicLibraryService _musicLibraryService;
    private readonly IStatisticsService _statisticsService;

    public SmartPlaylistService(
        IMusicLibraryService musicLibraryService,
        IStatisticsService statisticsService)
    {
        _musicLibraryService = musicLibraryService;
        _statisticsService = statisticsService;
    }

    public IReadOnlyList<SmartPlaylist> GetSmartPlaylists()
    {
        return new List<SmartPlaylist>
        {
            new() { Name = "播放最多", Rule = SmartPlaylistRule.MostPlayed, Limit = 50 },
            new() { Name = "最近播放", Rule = SmartPlaylistRule.RecentlyPlayed, Limit = 50 },
            new() { Name = "从未播放", Rule = SmartPlaylistRule.NeverPlayed, Limit = 50 },
            new() { Name = "最近添加", Rule = SmartPlaylistRule.RecentlyAdded, Limit = 50 },
        };
    }

    public Task<List<Song>> GetSongsForSmartPlaylistAsync(SmartPlaylist smartPlaylist)
    {
        var songs = _musicLibraryService.Songs.ToList();
        IEnumerable<Song> result = smartPlaylist.Rule switch
        {
            SmartPlaylistRule.MostPlayed =>
                songs.Where(s => s.PlayCount > 0)
                     .OrderByDescending(s => s.PlayCount)
                     .Take(smartPlaylist.Limit),

            SmartPlaylistRule.RecentlyPlayed =>
                songs.Where(s => s.LastPlayedTime.HasValue)
                     .OrderByDescending(s => s.LastPlayedTime)
                     .Take(smartPlaylist.Limit),

            SmartPlaylistRule.LeastPlayed =>
                songs.OrderBy(s => s.PlayCount)
                     .Take(smartPlaylist.Limit),

            SmartPlaylistRule.RecentlyAdded =>
                songs.OrderByDescending(s => s.AddedAt)
                     .Take(smartPlaylist.Limit),

            SmartPlaylistRule.NeverPlayed =>
                songs.Where(s => s.PlayCount == 0)
                     .Take(smartPlaylist.Limit),

            _ => songs
        };

        return Task.FromResult(result.ToList());
    }
}
```

**Step 3: Commit**

```bash
git add Services/SmartPlaylist/ISmartPlaylistService.cs Services/SmartPlaylist/SmartPlaylistService.cs
git commit -m "feat: add SmartPlaylistService with rule-based song filtering"
```

---

### Task 7: Register SmartPlaylistService in DI

**Files:**
- Modify: `Helpers/ServiceCollectionExtensions.cs`

**Step 1: Add registration**

Find where other services are registered and add:
```csharp
services.AddSingleton<ISmartPlaylistService, SmartPlaylistService>();
```

**Step 2: Commit**

```bash
git add Helpers/ServiceCollectionExtensions.cs
git commit -m "feat: register SmartPlaylistService in DI"
```

---

### Task 8: Add SmartPlaylists section to sidebar

**Files:**
- Modify: `ViewModels/MainWindowViewModel.cs` — add SmartPlaylistService property and command
- Modify: `Views/Main/MainWindow.axaml` — add Smart Playlists section in sidebar

**Step 1: In MainWindowViewModel, add:**

```csharp
private readonly ISmartPlaylistService _smartPlaylistService;

public ObservableCollection<SmartPlaylist> SmartPlaylists { get; } = new();

[RelayCommand]
private void NavigateToSmartPlaylist(SmartPlaylist smartPlaylist)
{
    SmartPlaylistSongsViewModel = _viewModelFactory.CreateSmartPlaylistSongsViewModel(smartPlaylist);
    CurrentPage = SmartPlaylistSongsViewModel;
}

// Also add:
public SmartPlaylistSongsViewModel? SmartPlaylistSongsViewModel { get; private set; }
```

And in constructor:
```csharp
var smartPlaylists = _smartPlaylistService.GetSmartPlaylists();
foreach (var sp in smartPlaylists)
    SmartPlaylists.Add(sp);
```

**Step 2: In MainWindow.axaml, add sidebar section:**

After the existing sidebar navigation items (before the playlist section), add:
```xml
<!-- Smart Playlists Section -->
<StackPanel Margin="8,16,8,8">
    <TextBlock Text="智能播放列表"
               FontSize="11" FontWeight="SemiBold"
               Foreground="{StaticResource TextMutedBrush}"
               Margin="12,0,0,8" />
    <ItemsControl ItemsSource="{Binding SmartPlaylists}">
        <ItemsControl.ItemTemplate>
            <DataTemplate x:DataType="models:SmartPlaylist">
                <Button Command="{Binding $parent[Window].((vm:MainWindowViewModel)DataContext).NavigateToSmartPlaylistCommand}"
                        CommandParameter="{Binding}"
                        Background="Transparent"
                        CornerRadius="8"
                        Padding="12,8"
                        HorizontalAlignment="Stretch">
                    <StackPanel Orientation="Horizontal" Spacing="8">
                        <TextBlock Text="&#xE9D9;"
                                   FontFamily="Segoe Fluent Icons"
                                   FontSize="14"
                                   Foreground="{StaticResource AccentPurpleBrush}"
                                   VerticalAlignment="Center" />
                        <TextBlock Text="{Binding Name}"
                                   FontSize="13"
                                   Foreground="{StaticResource TextPrimaryBrush}"
                                   VerticalAlignment="Center" />
                    </StackPanel>
                </Button>
            </DataTemplate>
        </ItemsControl.ItemTemplate>
    </ItemsControl>
</StackPanel>
```

**Step 3: Commit**

```bash
git add ViewModels/MainWindowViewModel.cs Views/Main/MainWindow.axaml
git commit -m "feat: add smart playlists section to sidebar"
```

---

### Task 9: Create SmartPlaylistSongsViewModel and View

**Files:**
- Create: `ViewModels/SmartPlaylistSongsViewModel.cs`
- Create: `Views/SmartPlaylist/SmartPlaylistSongsView.axaml`
- Create: `Views/SmartPlaylist/SmartPlaylistSongsView.axaml.cs`

**Step 1: SmartPlaylistSongsViewModel.cs**

```csharp
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalMusicPlayer.Models;
using LocalMusicPlayer.Services;

namespace LocalMusicPlayer.ViewModels;

public partial class SmartPlaylistSongsViewModel : ViewModelBase
{
    private readonly ISmartPlaylistService _smartPlaylistService;
    private readonly IPlaybackStateService _playbackStateService;
    private readonly IPlaylistService _playlistService;
    private readonly IStatisticsService _statisticsService;
    private readonly IPlayHistoryService _playHistoryService;
    private readonly IUserPlaylistService _userPlaylistService;

    public SmartPlaylist SmartPlaylist { get; }
    public string PageTitle => SmartPlaylist.Name;

    public ObservableCollection<Song> Songs { get; } = new();

    public SmartPlaylistSongsViewModel(
        SmartPlaylist smartPlaylist,
        ISmartPlaylistService smartPlaylistService,
        IPlaybackStateService playbackStateService,
        IPlaylistService playlistService,
        IStatisticsService statisticsService,
        IPlayHistoryService playHistoryService,
        IUserPlaylistService userPlaylistService)
    {
        SmartPlaylist = smartPlaylist;
        _smartPlaylistService = smartPlaylistService;
        _playbackStateService = playbackStateService;
        _playlistService = playlistService;
        _statisticsService = statisticsService;
        _playHistoryService = playHistoryService;
        _userPlaylistService = userPlaylistService;
        _ = LoadSongsAsync();
    }

    private async Task LoadSongsAsync()
    {
        var songs = await _smartPlaylistService.GetSongsForSmartPlaylistAsync(SmartPlaylist);
        Songs.Clear();
        foreach (var song in songs)
            Songs.Add(song);
    }

    [RelayCommand]
    private void PlayAll()
    {
        if (Songs.Count == 0) return;
        var playlist = _playlistService.CreatePlaylist(SmartPlaylist.Name);
        _playlistService.ClearPlaylist();
        foreach (var song in Songs)
            _playlistService.AddSongToPlaylist(playlist, song);
        if (_playlistService.PlayNext() is Song song)
        {
            _statisticsService.RecordPlayStart(song);
            _playHistoryService.AddToHistory(song);
            _playbackStateService.Play(song);
        }
    }

    [RelayCommand]
    private void ShuffleAll()
    {
        if (Songs.Count == 0) return;
        var shuffled = new System.Collections.Generic.List<Song>(Songs);
        var rng = new System.Random();
        int n = shuffled.Count;
        while (n > 1) { n--; int k = rng.Next(n + 1); (shuffled[k], shuffled[n]) = (shuffled[n], shuffled[k]); }
        var playlist = _playlistService.CreatePlaylist(SmartPlaylist.Name);
        _playlistService.ClearPlaylist();
        foreach (var song in shuffled)
            _playlistService.AddSongToPlaylist(playlist, song);
        _playbackStateService.PlaybackMode = PlaybackMode.Shuffle;
        if (_playlistService.PlayNext() is Song song)
        {
            _statisticsService.RecordPlayStart(song);
            _playHistoryService.AddToHistory(song);
            _playbackStateService.Play(song);
        }
    }
}
```

**Step 2: SmartPlaylistSongsView.axaml** (reuse the song list template from PlayerView)

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:vm="using:LocalMusicPlayer.ViewModels"
             xmlns:models="using:LocalMusicPlayer.Models"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             mc:Ignorable="d" d:DesignWidth="1200" d:DesignHeight="800"
             x:Class="LocalMusicPlayer.Views.SmartPlaylist.SmartPlaylistSongsView"
             x:DataType="vm:SmartPlaylistSongsViewModel">

    <Grid RowDefinitions="Auto,*">
        <!-- Header -->
        <Grid Grid.Row="0" ColumnDefinitions="*,Auto" Margin="32,24,32,0">
            <StackPanel Grid.Column="0" Spacing="4">
                <TextBlock Text="{Binding PageTitle}"
                           FontSize="32" FontWeight="ExtraBold"
                           Foreground="{StaticResource TextPrimaryBrush}" />
                <TextBlock FontSize="14" Foreground="{StaticResource TextSecondaryBrush}">
                    <Run Text="{Binding Songs.Count, Mode=OneWay}" />
                    <Run Text=" 首歌曲" />
                </TextBlock>
            </StackPanel>
            <StackPanel Grid.Column="1" Orientation="Horizontal" Spacing="8">
                <Button Command="{Binding PlayAllCommand}" Height="38" CornerRadius="8" Padding="12,0"
                        Background="{StaticResource PurplePinkVerticalGradientBrush}">
                    <StackPanel Orientation="Horizontal" Spacing="6">
                        <TextBlock Text="&#xE768;" FontFamily="Segoe Fluent Icons" FontSize="14" Foreground="White" VerticalAlignment="Center" />
                        <TextBlock Text="播放全部" FontSize="13" FontWeight="Medium" Foreground="White" VerticalAlignment="Center" />
                    </StackPanel>
                </Button>
                <Button Command="{Binding ShuffleAllCommand}" Height="38" CornerRadius="8" Padding="12,0"
                        Background="{StaticResource BgCardBrush}">
                    <StackPanel Orientation="Horizontal" Spacing="6">
                        <TextBlock Text="&#xE8B1;" FontFamily="Segoe Fluent Icons" FontSize="14" Foreground="{StaticResource TextPrimaryBrush}" VerticalAlignment="Center" />
                        <TextBlock Text="随机播放" FontSize="13" FontWeight="Medium" Foreground="{StaticResource TextPrimaryBrush}" VerticalAlignment="Center" />
                    </StackPanel>
                </Button>
            </StackPanel>
        </Grid>

        <!-- Song List -->
        <ListBox Grid.Row="1" ItemsSource="{Binding Songs}"
                 Background="Transparent" BorderThickness="0"
                 Margin="32,24,24,24"
                 ItemContainerTheme="{StaticResource SongListItemTheme}">
            <ListBox.DataTemplates>
                <DataTemplate x:DataType="models:Song">
                    <Border CornerRadius="8" Padding="16,12" BorderThickness="1"
                            BorderBrush="{StaticResource BorderSubtleBrush}">
                        <Grid ColumnDefinitions="30,*,180,70,60">
                            <TextBlock Grid.Column="0" Text="{Binding TrackNumber}"
                                       FontSize="13" Foreground="{StaticResource TextMutedBrush}"
                                       FontFamily="Consolas" VerticalAlignment="Center" />
                            <StackPanel Grid.Column="1" Spacing="2" VerticalAlignment="Center" Margin="0,0,16,0">
                                <TextBlock Text="{Binding Title}" FontSize="14" FontWeight="Bold"
                                           Foreground="{StaticResource TextPrimaryBrush}"
                                           TextTrimming="CharacterEllipsis" />
                                <TextBlock Text="{Binding Artist}" FontSize="12"
                                           Foreground="{StaticResource TextMutedBrush}"
                                           TextTrimming="CharacterEllipsis" />
                            </StackPanel>
                            <TextBlock Grid.Column="2" Text="{Binding Album}" FontSize="13"
                                       Foreground="{StaticResource TextMutedBrush}"
                                       TextTrimming="CharacterEllipsis" VerticalAlignment="Center" />
                            <TextBlock Grid.Column="3"
                                       Text="{Binding Duration, Converter={StaticResource TimeSpanToStringConverter}}"
                                       FontSize="13" Foreground="{StaticResource TextMutedBrush}"
                                       FontFamily="Consolas" VerticalAlignment="Center"
                                       HorizontalAlignment="Right" />
                            <TextBlock Grid.Column="4"
                                       Text="{Binding PlayCount, Converter={StaticResource PlayCountConverter}}"
                                       FontSize="13" Foreground="{StaticResource TextMutedBrush}"
                                       FontFamily="Consolas" VerticalAlignment="Center"
                                       HorizontalAlignment="Right" />
                        </Grid>
                    </Border>
                </DataTemplate>
            </ListBox.DataTemplates>
        </ListBox>
    </Grid>
</UserControl>
```

**Step 3: SmartPlaylistSongsView.axaml.cs**

```csharp
using Avalonia.Controls;

namespace LocalMusicPlayer.Views.SmartPlaylist;

public partial class SmartPlaylistSongsView : UserControl
{
    public SmartPlaylistSongsView()
    {
        InitializeComponent();
    }
}
```

**Step 4: Commit**

```bash
git add ViewModels/SmartPlaylistSongsViewModel.cs Views/SmartPlaylist/SmartPlaylistSongsView.axaml Views/SmartPlaylist/SmartPlaylistSongsView.axaml.cs
git commit -m "feat: add SmartPlaylistSongsViewModel and view for smart playlist browsing"
```

---

### Task 10: Register SmartPlaylistSongsViewModel in ViewModelFactory

**Files:**
- Modify: `Helpers/ServiceCollectionExtensions.cs` or the ViewModelFactory

**Step 1: Find IViewModelFactory and add method**

Add to `IViewModelFactory`:
```csharp
SmartPlaylistSongsViewModel CreateSmartPlaylistSongsViewModel(SmartPlaylist smartPlaylist);
```

Implement in `ViewModelFactory` (find this class):
```csharp
public SmartPlaylistSongsViewModel CreateSmartPlaylistSongsViewModel(SmartPlaylist smartPlaylist)
{
    return new SmartPlaylistSongsViewModel(
        smartPlaylist,
        _serviceProvider.GetRequiredService<ISmartPlaylistService>(),
        _serviceProvider.GetRequiredService<IPlaybackStateService>(),
        _serviceProvider.GetRequiredService<IPlaylistService>(),
        _serviceProvider.GetRequiredService<IStatisticsService>(),
        _serviceProvider.GetRequiredService<IPlayHistoryService>(),
        _serviceProvider.GetRequiredService<IUserPlaylistService>());
}
```

**Step 2: Commit**

```bash
git add [path to ViewModelFactory]
git commit -m "feat: register SmartPlaylistSongsViewModel in ViewModelFactory"
```

---

## Verification

After all tasks, run:

```bash
cd LocalMusicPlayer
dotnet build
```

Expected: Build succeeds with no errors.

---

## Task Summary

| # | Feature | Task | Files |
|---|---------|------|-------|
| 1 | Mini Mode | Add IsMiniMode to MainWindowViewModel | ViewModels/MainWindowViewModel.cs |
| 2 | Mini Mode | Create MiniModeWindow | Views/MiniMode/MiniModeWindow.axaml[.cs] |
| 3 | Mini Mode | Wire up MiniModeWindow open/close | ViewModels/MainWindowViewModel.cs |
| 4 | Mini Mode | Add mini mode button to bottom bar | Views/Main/MainWindow.axaml |
| 5 | Smart Playlist | Add SmartPlaylist model | Models/SmartPlaylist.cs |
| 6 | Smart Playlist | Create SmartPlaylistService | Services/SmartPlaylist/*.cs |
| 7 | Smart Playlist | Register DI | Helpers/ServiceCollectionExtensions.cs |
| 8 | Smart Playlist | Add sidebar section | ViewModels/MainWindowViewModel.cs + Views/Main/MainWindow.axaml |
| 9 | Smart Playlist | Create SmartPlaylistSongsViewModel + View | ViewModels/SmartPlaylistSongsViewModel.cs + Views/SmartPlaylist/*.axaml |
| 10 | Smart Playlist | Register in ViewModelFactory | [ViewModelFactory path] |
