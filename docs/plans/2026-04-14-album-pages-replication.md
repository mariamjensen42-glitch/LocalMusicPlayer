# 专辑页面复刻实现计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 将 original-sound-hq-player-win2d-navi 的专辑浏览页(AlbumPage)和专辑详情页(SongCollectionPage)完整复刻到 Avalonia 11 项目中。

**Architecture:** 两个页面独立实现——浏览页用 ItemsControl+WrapPanel 分组展示专辑卡片，详情页用信息头+完整歌曲列表表格。ViewModel 层增强命令和属性，复用已有控件（AlbumArtControl、AlbumArtConverter）和样式系统（PrimaryButtonTheme、CircularHoverButtonTheme 等）。WinUI3 的 SemanticZoom 用 ScrollViewer+手动分组替代。

**Tech Stack:** Avalonia 11, .NET 9.0, CommunityToolkit.Mvvm, MVVM + Compiled Bindings

**Design Spec:** `docs/album-pages-design.md`

---

## 文件变更总览

| 文件 | 操作 | 职责 |
|------|------|------|
| `Views/Library/AlbumsPageView.axaml` | 重写 | 分组卡片网格布局（匹配原版 AlbumPage GridView） |
| `Views/Library/AlbumsPageView.axaml.cs` | 修改 | 右键菜单事件处理 |
| `ViewModels/AlbumsPageViewModel.cs` | 增强 | 分组数据、右键菜单选项、播放/收藏/属性命令 |
| `Views/Details/AlbumDetailView.axaml` | 重写 | 原版详情布局（封面头+完整歌曲列表） |
| `Views/Details/AlbumDetailView.axaml.cs` | 修改 | 双击播放、右键菜单事件 |
| `ViewModels/AlbumDetailViewModel.cs` | 增强 | 完整功能：选中歌曲、右键菜单、艺术家/专辑跳转等 |
| `Styles/Resources.axaml` | 修改 | 新增 AlbumCardTheme、AlbumGroupHeaderTheme 等样式 |

---

### Task 1: 增强 AlbumsPageViewModel — 分组数据和命令

**Files:**
- Modify: `ViewModels/AlbumsPageViewModel.cs`
- Reference: `Models/AlbumGroup.cs`, `Services/Library/ILibraryCategoryService.cs`

- [ ] **Step 1: 添加分组模型类**

在 `Models/` 目录下新建或内联定义分组容器：

```csharp
namespace LocalMusicPlayer.Models;

public class AlbumGrouping : ObservableObject
{
    public string GroupKey { get; set; } = "";
    public ObservableCollection<AlbumGroup> Albums { get; set; } = new();
}
```

- [ ] **Step 2: 增强 AlbumsPageViewModel**

```csharp
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalMusicPlayer.Models;
using LocalMusicPlayer.Services;

namespace LocalMusicPlayer.ViewModels;

public partial class AlbumsPageViewModel : ViewModelBase
{
    private readonly ILibraryCategoryService _categoryService;

    public ObservableCollection<AlbumGroup> AlbumGroups { get; } = new();
    public ObservableCollection<AlbumGrouping> GroupedAlbums { get; } = new();

    [ObservableProperty] private AlbumGroup? _selectedItem;

    public Action<AlbumGroup>? OnNavigateToDetail { get; set; }

    public AlbumsPageViewModel(ILibraryCategoryService categoryService)
    {
        _categoryService = categoryService;
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        var groups = await Task.Run(() => _categoryService.GetAlbumGroups());
        AlbumGroups.Clear();
        foreach (var group in groups)
            AlbumGroups.Add(group);

        BuildGroupedAlbums();
    }

    private void BuildGroupedAlbums()
    {
        GroupedAlbums.Clear();
        var grouped = AlbumGroups
            .OrderBy(a => a.AlbumName)
            .GroupBy(a => GetGroupKey(a.AlbumName))
            .OrderBy(g => g.Key);

        foreach (var group in grouped)
        {
            GroupedAlbums.Add(new AlbumGrouping
            {
                GroupKey = group.Key,
                Albums = new ObservableCollection<AlbumGroup>(group.OrderBy(a => a.AlbumName))
            });
        }
    }

    private static string GetGroupKey(string name)
    {
        if (string.IsNullOrEmpty(name)) return "#";
        var c = char.ToUpper(name[0]);
        return c >= 'A' && c <= 'Z' ? c.ToString() : "#";
    }

    [RelayCommand]
    private void SelectItem(AlbumGroup albumGroup)
    {
        OnNavigateToDetail?.Invoke(albumGroup);
    }

    [RelayCommand]
    private void Play()
    {
        if (SelectedItem == null) return;
        OnNavigateToDetail?.Invoke(SelectedItem);
    }

    [RelayCommand]
    private void AddToFavourite()
    {
    }

    [RelayCommand]
    private void ShowPropertyWindow()
    {
    }
}
```

- [ ] **Step 3: 验证编译**

Run: `dotnet build`
Expected: 编译通过，无错误

---

### Task 2: 新增样式资源 — AlbumCardTheme 和分组头样式

**Files:**
- Modify: `Styles/Resources.axaml` (在 `</ResourceDictionary>` 之前添加)

- [ ] **Step 1: 添加专辑卡片样式**

在 Resources.axaml 的 `</ResourceDictionary>` 闭合标签前添加：

```xml
<!--  ==================== 专辑页面样式 ====================  -->

<!--  专辑卡片容器样式  -->
<ControlTheme x:Key="AlbumCardTheme" TargetType="Border">
    <Setter Property="Background" Value="{StaticResource BgCardBrush}" />
    <Setter Property="CornerRadius" Value="{StaticResource CardCornerRadius}" />
    <Setter Property="Padding" Value="0" />
    <Setter Property="Cursor" Value="Hand" />
    <Setter Property="MinWidth" Value="160" />
    <Setter Property="MaxWidth".Value="180" />
    <Style Selector="^:pointerover">
        <Setter Property="Background" Value="{StaticResource BgCardHoverBrush}" />
    </Style>
</ControlTheme>

<!--  专辑分组标题样式  -->
<ControlTheme x:Key="AlbumGroupHeaderTheme" TargetType="TextBlock">
    <Setter Property="FontSize" Value="28" />
    <Setter Property="FontWeight" Value="SemiBold" />
    <Setter Property="Foreground" Value="{StaticResource TextPrimaryBrush}" />
    <Setter Property="Margin" Value="16,12,8,4" />
</ControlTheme>

<!--  歌曲表头样式  -->
<ControlTheme x:Key="SongTableHeaderTheme" TargetType="Border">
    <Setter Property="Background" Value="{StaticResource BgTableHeaderBrush}" />
    <Setter Property="CornerRadius" Value="12,12,0,0" />
    <Setter Property="Padding" Value="20,10" />
</ControlTheme>

<!--  详情页封面容器  -->
<ControlTheme x:Key="DetailCoverTheme" TargetType="Border">
    <Setter Property="Width" Value="150" />
    <Setter Property="Height" Value="150" />
    <Setter Property="CornerRadius" Value="5" />
    <Setter Property="ClipToBounds" Value="True" />
    <Setter Property="Background" Value="{StaticResource BgElement2Brush}" />
</ControlTheme>
```

- [ ] **Step 2: 验证编译**

Run: `dotnet build`
Expected: 编译通过

---

### Task 3: 重写 AlbumsPageView — 分组卡片网格

**Files:**
- Modify: `Views/Library/AlbumsPageView.axaml`
- Modify: `Views/Library/AlbumsPageView.axaml.cs`

- [ ] **Step 1: 重写 XAML 布局**

完全替换 AlbumsPageView.axaml 内容：

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:vm="using:LocalMusicPlayer.ViewModels"
             xmlns:models="using:LocalMusicPlayer.Models"
             xmlns:converters="using:LocalMusicPlayer.Converters"
             xmlns:i="clr-namespace:Avalonia.Xaml.Interactivity;assembly=Avalonia.Xaml.Interactivity"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             mc:Ignorable="d" d:DesignWidth="1200" d:DesignHeight="800"
             x:Class="LocalMusicPlayer.Views.Library.AlbumsPageView"
             x:DataType="vm:AlbumsPageViewModel"
             x:CompileBindings="True">

    <UserControl.Resources>
        <converters:AlbumArtConverter x:Key="AlbumArtConverter" />
    </UserControl.Resources>

    <Grid RowDefinitions="Auto,*">
        <!-- 标题栏 -->
        <Border Grid.Row="0"
                Background="{StaticResource BgCardBrush}"
                BorderBrush="{StaticResource BorderSubtleBrush}"
                BorderThickness="0,0,0,1"
                Padding="24,16">
            <TextBlock Text="{DynamicResource StringAlbums}"
                       FontSize="24"
                       FontWeight="Bold"
                       Foreground="{StaticResource TextPrimaryBrush}" />
        </Border>

        <!-- 分组专辑列表 -->
        <ScrollViewer Grid.Row="1">
            <ItemsSource ItemsSource="{Binding GroupedAlbums}">
                <ItemsSource.DataTemplate>
                    <DataTemplate x:DataType="models:AlbumGrouping">
                        <StackPanel Spacing="4">
                            <!-- 分组标题 -->
                            <TextBlock Text="{Binding GroupKey}"
                                       Theme="{StaticResource AlbumGroupHeaderTheme}" />

                            <!-- 该组的专辑卡片 WrapPanel -->
                            <ItemsControl ItemsSource="{Binding Albums}"
                                          Margin="0,0,0,16">
                                <ItemsControl.ItemsPanel>
                                    <ItemsPanelTemplate>
                                        <WrapPanel />
                                    </ItemsPanelTemplate>
                                </ItemsControl.ItemsPanel>
                                <ItemsControl.ItemTemplate>
                                    <DataTemplate x:DataType="models:AlbumGroup">
                                        <Border Theme="{StaticResource AlbumCardTheme}"
                                                Margin="0,6,12,6"
                                                Width="170">
                                            <i:Interaction.Behaviors>
                                                <i:EventTriggerBehavior EventName="PointerPressed"
                                                    Command="{Binding $parent[ItemsControl].$parent[StackPanel].$parent[ItemsSource].DataContext.SelectItemCommand}"
                                                    CommandParameter="{Binding}" />
                                            </i:Interaction.Behaviors>
                                            <Border.ContextMenu>
                                                <ContextMenu>
                                                    <MenuItem Header="{DynamicResource StringPlay}"
                                                              Command="{Binding $parent[ItemsControl].$parent[StackPanel].$parent[ItemsSource].DataContext.PlayCommand}" />
                                                    <MenuItem Header="{DynamicResource StringAddToFavourite}"
                                                              Command="{Binding $parent[ItemsControl].$parent[StackPanel].$parent[ItemsSource].DataContext.AddToFavourCommand}" />
                                                    <Separator />
                                                    <MenuItem Header="{DynamicResource StringProperties}"
                                                              Command="{Binding $parent[ItemsControl].$parent[StackPanel].$parent[ItemsSource].DataContext.ShowPropertyWindowCommand}" />
                                                </ContextMenu>
                                            </Border.ContextMenu>
                                            <StackPanel Margin="10" Spacing="6">
                                                <!-- 封面 150x150 -->
                                                <Border Width="150"
                                                        Height="150"
                                                        CornerRadius="5"
                                                        Background="{StaticResource BgElement2Brush}"
                                                        ClipToBounds="True"
                                                        HorizontalAlignment="Center">
                                                    <Image Source="{Binding CoverArtPath, Converter={StaticResource AlbumArtConverter}}"
                                                           Stretch="UniformToFill" />
                                                </Border>

                                                <!-- 专辑名 -->
                                                <TextBlock Text="{Binding AlbumName}"
                                                           FontSize="13"
                                                           FontWeight="SemiBold"
                                                           Foreground="{StaticResource TextPrimaryBrush}"
                                                           MaxHeight="40"
                                                           TextWrapping="Wrap"
                                                           TextTrimming="CharacterEllipsis" />

                                                <!-- 底部信息行：艺术家 | 歌曲数 -->
                                                <Grid ColumnDefinitions="*,Auto">
                                                    <TextBlock Grid.Column="0"
                                                               Text="{Binding ArtistName}"
                                                               FontSize="12"
                                                               Foreground="{StaticResource TextMutedBrush}"
                                                               TextTrimming="CharacterEllipsis" />
                                                    <StackPanel Grid.Column="1"
                                                                Orientation="Horizontal"
                                                                Spacing="2">
                                                        <TextBlock Text="{Binding SongCount}"
                                                                   FontSize="12"
                                                                   Foreground="{StaticResource TextMutedBrush}" />
                                                        <TextBlock Text="{DynamicResource StringSongsUnit}"
                                                                   FontSize="12"
                                                                   Foreground="{StaticResource TextMutedBrush}" />
                                                    </StackPanel>
                                                </Grid>
                                            </StackPanel>
                                        </Border>
                                    </DataTemplate>
                                </ItemsControl.ItemTemplate>
                            </ItemsControl>
                        </StackPanel>
                    </DataTemplate>
                </ItemsSource.DataTemplate>
            </ItemsSource>
        </ScrollViewer>
    </Grid>
</UserControl>
```

- [ ] **Step 2: 更新 code-behind**

`AlbumsPageView.axaml.cs` 保持简单：

```csharp
using Avalonia.Controls;

namespace LocalMusicPlayer.Views.Library;

public partial class AlbumsPageView : UserControl
{
    public AlbumsPageView()
    {
        InitializeComponent();
    }
}
```

- [ ] **Step 3: 验证编译**

Run: `dotnet build`
Expected: 编译通过

---

### Task 4: 增强 AlbumDetailViewModel — 完整功能

**Files:**
- Modify: `ViewModels/AlbumDetailViewModel.cs`
- Reference: `ViewModels/DetailViewModelBase.cs`, `Models/Song.cs`, `Models/AlbumGroup.cs`

- [ ] **Step 1: 增强 ViewModel**

重写 AlbumDetailViewModel.cs：

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalMusicPlayer.Models;
using LocalMusicPlayer.Services;

namespace LocalMusicPlayer.ViewModels;

public partial class AlbumDetailViewModel : DetailViewModelBase
{
    public override string DetailName => AlbumName;
    public override string? CoverArtPath { get; }

    public string AlbumName { get; }
    public string ArtistName { get; }
    public override string Subtitle => $"{ArtistName} · {SongCount} 首";

    public string SecondTitle => Songs.Select(s => s.Artist).Distinct().Where(a => !string.IsNullOrEmpty(a)).JoinToString(" · ");
    public string ThirdTitle
    {
        get
        {
            var yearInfo = Songs.FirstOrDefault()?.Year > 0 ? $"{Songs.First().Year} · " : "";
            return $"{yearInfo}{SongCount} 首";
        }
    }

    [ObservableProperty] private Song? _selectedSong;
    [ObservableProperty] private List<Song> _selectedSongs = new();

    public AlbumDetailViewModel(
        AlbumGroup albumGroup,
        IMusicPlayerService musicPlayerService,
        IPlaylistService playlistService,
        IStatisticsService statisticsService,
        INavigationService navigationService,
        IDialogService dialogService)
        : base(musicPlayerService, playlistService, statisticsService, navigationService, dialogService)
    {
        AlbumName = albumGroup.AlbumName;
        ArtistName = albumGroup.ArtistName;
        CoverArtPath = albumGroup.CoverArtPath;

        LoadSongs(albumGroup.Songs);
    }

    [RelayCommand]
    private void ToggleFavorite(Song? song)
    {
        if (song != null)
        {
            song.IsFavorite = !song.IsFavorite;
        }
    }

    [RelayCommand]
    private void AddToCurrentPlaylist(Song song)
    {
    }

    [RelayCommand]
    private async Task OpenInExplorerAsync(Song song)
    {
    }

    [RelayCommand]
    private void NavigateToArtist(string artist)
    {
    }

    [RelayCommand]
    private void NavigateToAlbum(string album)
    {
    }

    partial void OnSelectedSongChanged(Song? value)
    {
        if (value != null)
        {
            SelectedSongs = new List<Song> { value };
        }
    }
}
```

注意：`JoinToString` 是一个扩展方法，如果不存在需要添加或在 VM 内联使用 `string.Join(" · ", ...)`。

- [ ] **Step 2: 验证编译**

Run: `dotnet build`
Expected: 编译通过（可能需要补充缺失的 using 或方法）

---

### Task 5: 重写 AlbumDetailView — 原版详情布局

**Files:**
- Modify: `Views/Details/AlbumDetailView.axaml`
- Modify: `Views/Details/AlbumDetailView.axaml.cs`

- [ ] **Step 1: 重写 XAML**

完全替换 AlbumDetailView.axaml 内容：

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:vm="using:LocalMusicPlayer.ViewModels"
             xmlns:models="using:LocalMusicPlayer.Models"
             xmlns:converters="using:LocalMusicPlayer.Converters"
             xmlns:i="clr-namespace:Avalonia.Xaml.Interactivity;assembly=Avalonia.Xaml.Interactivity"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             mc:Ignorable="d" d:DesignWidth="1000" d:DesignHeight="700"
             x:Class="LocalMusicPlayer.Views.Details.AlbumDetailView"
             x:DataType="vm:AlbumDetailViewModel"
             x:CompileBindings="True">

    <UserControl.Resources>
        <converters:TimeSpanToStringConverter x:Key="TimeSpanToStringConverter" />
        <converters:AlbumArtConverter x:Key="AlbumArtConverter" />
        <converters:CountToBoolConverter x:Key="CountToBoolConverter" />
    </UserControl.Resources>

    <ScrollViewer VerticalScrollBarVisibility="Auto">
        <StackPanel Spacing="24" Margin="24,20">

            <!-- ========== 信息头区域 ========== -->
            <Grid ColumnDefinitions="Auto,*">
                <!-- 左侧：封面 -->
                <Border Grid.Column="0"
                        Theme="{StaticResource DetailCoverTheme}">
                    <Image Source="{Binding CoverArtPath, Converter={StaticResource AlbumArtConverter}}"
                           Stretch="UniformToFill" />
                </Border>

                <!-- 右侧：信息 -->
                <StackPanel Grid.Column="1"
                            Margin="24,0,0,0"
                            VerticalAlignment="Center"
                            Spacing="6">
                    <!-- 专辑名 -->
                    <TextBlock Text="{Binding AlbumName}"
                               FontSize="30"
                               FontWeight="Bold"
                               Foreground="{StaticResource TextPrimaryBrush}"
                               TextTrimming="CharacterEllipsis"
                               IsTextSelectionEnabled="True" />

                    <!-- 艺术家副标题 -->
                    <TextBlock Text="{Binding SecondTitle}"
                               FontSize="20"
                               Foreground="{StaticResource TextSecondaryBrush}"
                               TextTrimming="CharacterEllipsis"
                               IsTextSelectionEnabled="True" />

                    <!-- 年份·歌曲数 -->
                    <TextBlock Text="{Binding ThirdTitle}"
                               FontSize="14"
                               Foreground="{StaticResource TextMutedBrush}"
                               TextTrimming="CharacterEllipsis"
                               IsTextSelectionEnabled="True" />

                    <!-- 操作按钮 -->
                    <StackPanel Orientation="Horizontal" Spacing="12" Margin="0,16,0,0">
                        <Button Theme="{StaticResource PrimaryButtonTheme}"
                                Command="{Binding PlayAllCommand}">
                            <StackPanel Orientation="Horizontal" Spacing="8" VerticalAlignment="Center">
                                <TextBlock Text="&#xE768;"
                                           FontFamily="Segoe Fluent Icons"
                                           FontSize="16"
                                           Foreground="White"
                                           VerticalAlignment="Center" />
                                <TextBlock Text="{DynamicResource StringPlayAll}"
                                           FontSize="13"
                                           FontWeight="Medium"
                                           Foreground="White"
                                           VerticalAlignment="Center" />
                            </StackPanel>
                        </Button>

                        <Button Theme="{StaticResource SecondaryButtonTheme}">
                            <StackPanel Orientation="Horizontal" Spacing="8" VerticalAlignment="Center">
                                <TextBlock Text="&#xE710;"
                                           FontFamily="Segoe Fluent Icons"
                                           FontSize="16"
                                           Foreground="{StaticResource TextSecondaryBrush}"
                                           VerticalAlignment="Center" />
                                <TextBlock Text="{DynamicResource StringAddTo}"
                                           FontSize="13"
                                           FontWeight="Medium"
                                           VerticalAlignment="Center" />
                            </StackPanel>
                        </Button>

                        <Button Theme="{StaticResource SecondaryButtonTheme}"
                                Command="{Binding EditSongMetadataCommand}">
                            <StackPanel Orientation="Horizontal" Spacing="8" VerticalAlignment="Center">
                                <TextBlock Text="&#xE70F;"
                                           FontFamily="Segoe Fluent Icons"
                                           FontSize="16"
                                           Foreground="{StaticResource TextSecondaryBrush}"
                                           VerticalAlignment="Center" />
                                <TextBlock Text="{DynamicResource StringEdit}"
                                           FontSize="13"
                                           FontWeight="Medium"
                                           VerticalAlignment="Center" />
                            </StackPanel>
                        </Button>
                    </StackPanel>
                </StackPanel>
            </Grid>

            <!-- ========== 歌曲列表 ========== -->
            <Border CornerRadius="12"
                    Background="{StaticResource BgCardBrush}"
                    BorderBrush="{StaticResource BorderSubtleBrush}"
                    BorderThickness="1">
                <Grid RowDefinitions="Auto,*">
                    <!-- 表头 -->
                    <Border Grid.Row="0"
                            Theme="{StaticResource SongTableHeaderTheme}">
                        <Grid ColumnDefinitions="44,Auto,3*,1.5*,3*,Auto,Auto,Auto,Auto,Auto,Auto">
                            <TextBlock Grid.Column="0" FontSize="11" FontWeight="SemiBold"
                                       Foreground="{StaticResource TextMutedBrush}" HorizontalAlignment="Center" />
                            <TextBlock Grid.Column="1" FontSize="11" FontWeight="SemiBold"
                                       Foreground="{StaticResource TextMutedBrush}" />
                            <TextBlock Grid.Column="2" Text="{DynamicResource StringTitleArtist}"
                                       FontSize="11" FontWeight="SemiBold"
                                       Foreground="{StaticResource TextMutedBrush}" />
                            <TextBlock Grid.Column="4" Text="{DynamicResource StringDuration}"
                                       FontSize="11" FontWeight="SemiBold"
                                       Foreground="{StaticResource TextMutedBrush}"
                                       HorizontalAlignment="Right" />
                        </Grid>
                    </Border>

                    <!-- 歌曲列表 -->
                    <ListBox Grid.Row="1"
                             ItemsSource="{Binding Songs}"
                             Background="Transparent"
                             BorderThickness="0"
                             ScrollViewer.HorizontalScrollBarVisibility="Disabled">
                        <ListBox.ItemContainerTheme>
                            <ControlTheme TargetType="ListBoxItem">
                                <Setter Property="Padding" Value="0" />
                                <Setter Property="Margin" Value="0" />
                                <Setter Property="Background" Value="Transparent" />
                                <Setter Property="BorderThickness" Value="0" />
                                <Style Selector="^:pointerover /template/ ContentPresenter#PART_ContentPresenter">
                                    <Setter Property="Background" Value="Transparent" />
                                </Style>
                                <Style Selector="^:selected /template/ ContentPresenter#PART_ContentPresenter">
                                    <Setter Property="Background" Value="Transparent" />
                                </Style>
                                <Setter Property="Template">
                                    <ControlTemplate>
                                        <ContentPresenter Content="{TemplateBinding Content}"
                                                          ContentTemplate="{TemplateBinding ContentTemplate}" />
                                    </ControlTemplate>
                                </Setter>
                            </ControlTheme>
                        </ListBox.ItemContainerTheme>
                        <ListBox.ItemTemplate>
                            <DataTemplate x:DataType="models:Song">
                                <Border Theme="{StaticResource SongListItemRowTheme}"
                                        Cursor="Hand">
                                    <i:Interaction.Behaviors>
                                        <i:EventTriggerBehavior EventName="DoubleTapped"
                                            Command="{Binding $parent[ListBox].DataContext.PlaySongCommand}"
                                            CommandParameter="{Binding FilePath}" />
                                    </i:Interaction.Behaviors>
                                    <Border.ContextMenu>
                                        <ContextMenu>
                                            <MenuItem Header="{DynamicResource StringPlay}"
                                                      Command="{Binding $parent[ListBox].DataContext.PlaySongCommand}"
                                                      CommandParameter="{Binding FilePath}" />
                                            <MenuItem Header="{DynamicResource StringFavorite}"
                                                      Command="{Binding $parent[ListBox].DataContext.ToggleFavoriteCommand}"
                                                      CommandParameter="{Binding}" />
                                            <Separator />
                                            <MenuItem Header="{DynamicResource StringAddToPlaylist}" />
                                            <Separator />
                                            <MenuItem Header="{DynamicResource StringAddToQueue}"
                                                      Command="{Binding $parent[ListBox].DataContext.AddToCurrentPlaylistCommand}"
                                                      CommandParameter="{Binding}" />
                                            <Separator />
                                            <MenuItem Header="{DynamicResource StringOpenLocation}"
                                                      Command="{Binding $parent[ListBox].DataContext.OpenInExplorerAsyncCommand}"
                                                      CommandParameter="{Binding}" />
                                            <MenuItem Header="{DynamicResource StringProperties}"
                                                      Command="{Binding $parent[ListBox].DataContext.EditSongMetadataCommand}"
                                                      CommandParameter="{Binding}" />
                                        </ContextMenu>
                                    </Border.ContextMenu>
                                    <Grid ColumnDefinitions="44,Auto,3*,1.5*,3*,Auto,Auto,Auto,Auto,Auto,Auto">
                                        <!-- 封面缩略图 -->
                                        <Button Grid.Column="0"
                                                Theme="{StaticResource CircularHoverButtonTheme}"
                                                Command="{Binding $parent[ListBox].DataContext.PlaySongCommand}"
                                                CommandParameter="{Binding FilePath}">
                                            <Border Width="40" Height="40"
                                                    CornerRadius="5"
                                                    ClipToBounds="True"
                                                    Background="{StaticResource BgElement2Brush}">
                                                <Image Source="{Binding AlbumArtPath, Converter={StaticResource AlbumArtConverter}}"
                                                       Stretch="UniformToFill" />
                                            </Border>
                                        </Button>

                                        <!-- 收藏按钮 -->
                                        <Button Grid.Column="1"
                                                Theme="{StaticResource NoHoverButtonTheme}"
                                                Command="{Binding $parent[ListBox].DataContext.ToggleFavoriteCommand}"
                                                CommandParameter="{Binding}"
                                                Margin="0,5,5,5"
                                                Padding="3"
                                                VerticalAlignment="Center">
                                            <TextBlock Text="{Binding IsFavorite, Converter={StaticResource FavouriteIconConverter}}"
                                                       FontFamily="Segoe Fluent Icons"
                                                       FontSize="14"
                                                       Foreground="Red" />
                                        </Button>

                                        <!-- 歌曲名 -->
                                        <TextBlock Grid.Column="2"
                                                   Text="{Binding Title}"
                                                   FontSize="14"
                                                   FontWeight="Medium"
                                                   Foreground="{StaticResource TextPrimaryBrush}"
                                                   TextTrimming="CharacterEllipsis"
                                                   VerticalAlignment="Center"
                                                   Margin="8,0" />

                                        <!-- 艺术家 -->
                                        <Button Grid.Column="3"
                                                Theme="{StaticResource NoHoverButtonTheme}"
                                                Margin="8,5,5,5"
                                                Padding="5"
                                                HorizontalAlignment="Left"
                                                VerticalAlignment="Center"
                                                Command="{Binding $parent[ListBox].DataContext.NavigateToArtistCommand}"
                                                CommandParameter="{Binding Artist}">
                                            <TextBlock Text="{Binding Artist}"
                                                       FontSize="12"
                                                       Foreground="{StaticResource TextMutedBrush}"
                                                       TextTrimming="CharacterEllipsis"
                                                       VerticalAlignment="Center" />
                                        </Button>

                                        <!-- 时长 -->
                                        <TextBlock Grid.Column="9"
                                                   Text="{Binding Duration, Converter={StaticResource TimeSpanToStringConverter}}"
                                                   FontSize="13"
                                                   Foreground="{StaticResource TextMutedBrush}"
                                                   FontFamily="{StaticResource FontFamilyMono}"
                                                   VerticalAlignment="Center"
                                                   HorizontalAlignment="Right"
                                                   Margin="0,0,8,0" />

                                        <!-- 添加到队列 -->
                                        <Button Grid.Column="10"
                                                Theme="{StaticResource NoHoverButtonTheme}"
                                                Margin="5"
                                                VerticalAlignment="Center"
                                                Command="{Binding $parent[ListBox].DataContext.AddToCurrentPlaylistCommand}"
                                                CommandParameter="{Binding}">
                                            <TextBlock Text="&#xE710;"
                                                       FontFamily="Segoe Fluent Icons"
                                                       FontSize="14"
                                                       Foreground="{StaticResource TextMutedBrush}"
                                                       VerticalAlignment="Center" />
                                        </Button>
                                    </Grid>
                                </Border>
                            </DataTemplate>
                        </ListBox.ItemTemplate>
                    </ListBox>

                    <!-- 空状态 -->
                    <Grid Grid.Row="1"
                          IsVisible="{Binding Songs.Count, Converter={StaticResource CountToBoolConverter}, ConverterParameter=invert}"
                          HorizontalAlignment="Center"
                          VerticalAlignment="Center">
                        <StackPanel Spacing="16" HorizontalAlignment="Center">
                            <TextBlock Text="&#xE93C;"
                                       FontFamily="Segoe Fluent Icons"
                                       FontSize="48"
                                       Foreground="{StaticResource TextMutedBrush}"
                                       HorizontalAlignment="Center" />
                            <TextBlock Text="{DynamicResource StringNoSongs}"
                                       FontSize="16"
                                       FontWeight="Medium"
                                       Foreground="{StaticResource TextSecondaryBrush}"
                                       HorizontalAlignment="Center" />
                        </StackPanel>
                    </Grid>
                </Grid>
            </Border>
        </StackPanel>
    </ScrollViewer>
</UserControl>
```

- [ ] **Step 2: 更新 code-behind**

保持简洁：

```csharp
using Avalonia.Controls;

namespace LocalMusicPlayer.Views.Details;

public partial class AlbumDetailView : UserControl
{
    public AlbumDetailView()
    {
        InitializeComponent();
    }
}
```

- [ ] **Step 3: 验证编译**

Run: `dotnet build`
Expected: 编译通过（可能需要补充 FavouriteIconConverter 或调整绑定路径）

---

### Task 6: 补充缺失的转换器和字符串资源

**Files:**
- 可能 Create: `Converters/UI/FavouriteIconConverter.cs`（如不存在）
- Check: 字符串资源文件中的 Key 是否齐全

- [ ] **Step 1: 检查/创建 FavouriteIconConverter**

如果不存在，创建：

```csharp
using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace LocalMusicPlayer.Converters;

public class FavouriteIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is true ? "\xE00B" : "\xE00A";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
```

- [ ] **Step 2: 检查字符串资源 Key**

确认以下 Key 存在于字符串资源文件中：
- `StringPlayAll`
- `StringAddTo`
- `StringEdit`
- `StringPlay`
- `StringFavorite`
- `StringAddToPlaylist`
- `StringAddToQueue`
- `StringOpenLocation`
- `StringProperties`
- `StringNoSongs`
- `StringSongsUnit`
- `StringTitleArtist`
- `StringDuration`
- `StringAlbums`

- [ ] **Step 3: 最终验证**

Run: `dotnet build`
Expected: 0 errors, 0 warnings

---

### Task 7: 运行测试和文档更新

- [ ] **Step 1: 运行应用验证**

Run: `dotnet run`
Expected: 应用启动成功，导航到专辑浏览页可看到分组卡片布局，点击卡片进入详情页可看到封面头+歌曲列表

- [ ] **Step 2: 更新 fc.md**

在 `docs/fc.md` 中追加记录：

```markdown
### 3. UI 大改 — 专辑页面复刻 (WinUI3 → Avalonia)

- **日期**: 2026-04-14
- **变更内容**:
  - AlbumsPageView: 从简单卡片布局改为按首字母分组的网格卡片（匹配原版 AlbumPage GridView）
  - AlbumDetailView: 从基础详情改为原版 SongCollectionPage 布局（150×150 封面头 + 完整歌曲列表表格）
  - AlbumsPageViewModel: 增加 GroupedAlbums 分组数据、右键菜单选项、播放/收藏/属性命令
  - AlbumDetailViewModel: 增加 SelectedSong/Songs、SecondTitle/ThirdTitle、ToggleFavorite、导航命令
  - Resources.axaml: 新增 AlbumCardTheme、AlbumGroupHeaderTheme、SongTableHeaderTheme、DetailCoverTheme 样式
- **参考**: original-sound-hq-player-win2d-navi View/AlbumPage.xaml, View/SongCollectionPage.xaml
```
