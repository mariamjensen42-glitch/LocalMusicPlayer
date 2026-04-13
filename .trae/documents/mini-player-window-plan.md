# 迷你模式（歌词型小窗）实现计划

## 需求概述

替换现有迷你模式（400×90 横向控制条）为**竖向歌词迷你播放器**，参考用户提供的设计图：
- 紧凑竖向布局（~300px 宽 × ~520px 高）
- 左上角小封面 + 歌曲名/歌手
- **歌词显示区域**（高亮当前行 + 中英双语）
- 渐变背景（跟随专辑色调）
- 仅展示，不含播放控制按钮

## 设计方案：方案 A — 同窗切换

在 `MainWindow.axaml` 中通过 `IsMiniMode` 属性切换两个完全不同的视觉状态。

---

## 布局设计

### 迷你模式整体结构

```
┌──────────────────────┐ 300px 宽
│ [封面64x64]  歌曲名   │ ← Row 0: 封面+信息区 (Auto)
│              歌手名   │
├──────────────────────┤
│                      │
│    词：潘伟源         │ ← Row 1: 歌词区 (*)
│    Word: Pan Weigen  │
│                      │
│  （前几行歌词，淡化）  │
│                      │
│  ─────────────────   │
│  **当前行（高亮）**   │ ← 白色/亮色, FontWeight=Bold
│  ─────────────────   │
│                      │
│  （后几行歌词，淡化）  │
│                      │
└──────────────────────┘ ~520px 高
```

### 视觉规范

| 元素 | 规格 |
|------|------|
| 窗口尺寸 | 300×520，不可调整大小 |
| 圆角 | 16px |
| 背景 | 渐变色（复用 `PlayerPageViewModel.GradientBackground`） |
| 封面 | 64×64px，圆角 8px，带阴影 |
| 歌曲名字号 | 15px, SemiBold, 白色 |
| 歌手名字号 | 12px, Secondary 色 |
| 歌词字号 | 14px（当前行 Bold + 白色，其他行 Muted 色 + 60% 透明度）|
| 译文字号 | 11px, Muted 色 |
| 歌词行间距 | 6px |
| 内边距 | 20px |

---

## 修改文件清单

### 1. `Views/Main/MainWindow.axaml`

**改动要点：**

1. 在根 `<Grid>` 内添加**迷你模式覆盖层**（置于最顶层 `ZIndex="300"`），当 `IsMiniMode=true` 时显示、同时隐藏原有全部内容

2. 迷你模式覆盖层结构：
   ```xml
   <Border Grid.RowSpan="2"
           IsVisible="{Binding IsMiniMode}"
           ZIndex="300">
       <Grid Background="{Binding PlayerPageViewModel.GradientBackground, Converter={...}}"
             CornerRadius="16">
           <Grid RowDefinitions="Auto,*" Margin="20">
               <!-- Row 0: 封面 + 信息 -->
               <Grid ColumnDefinitions="Auto,*">
                   <Border Width="64" Height="64" CornerRadius="8"> ...封面... </Border>
                   <StackPanel> ...歌名+歌手... </StackPanel>
               </Grid>
               <!-- Row 1: 歌词滚动区域 -->
               <ScrollViewer ...>
                   <ItemsControl ItemsSource="{Binding PlayerPageViewModel.Lyrics}">
                       <!-- 复用 PlayerPageView 的歌词 DataTemplate -->
                   </ItemsControl>
               </ScrollViewer>
           </Grid>
       </Grid>
   </Border>
   ```

3. 将原有主内容（侧边栏 + 内容区 + 底部栏等）的 `IsVisible` 绑定到 `{Binding !IsMiniMode}`

### 2. `Views/Main/MainWindow.axaml.cs`

**改动要点：**

修改 `OnViewModelPropertyChanged` 中 `IsMiniMode` 分支的窗口尺寸：

```csharp
// 迷你模式尺寸改为竖向
Width = 300;
Height = 520;
MinWidth = 300;
MinHeight = 520;
CanResize = false;
Topmost = true;
WindowStartupLocation = (已有位置保持不变)
```

还原时恢复原尺寸。

### 3. `Styles/Resources.axaml`（可选）

如需新增迷你模式专用样式资源可在此添加。

---

## 数据绑定来源

| 绑定属性 | 来源 | 说明 |
|----------|------|------|
| `IsMiniMode` | `MainWindowViewModel` | 已有 ✅ |
| `CurrentSong.Title` | `MainWindowViewModel.CurrentSong` | 已有 ✅ |
| `CurrentSong.Artist` | `MainWindowViewModel.CurrentSong` | 已有 ✅ |
| `CurrentSong.AlbumArtPath` | `MainWindowViewModel.CurrentSong` | 已有 ✅ |
| `PlayerPageViewModel.Lyrics` | `MainWindowViewModel.PlayerPageViewModel` | 已有 ✅ |
| `PlayerPageViewModel.GradientBackground` | `MainWindowViewModel.PlayerPageViewModel` | 已有 ✅ |
| `PlayerPageViewModel.CurrentLyricIndex` | `MainWindowViewModel.PlayerPageViewModel` | 已有 ✅ |

所有数据均已就绪，无需新建 ViewModel。

---

## 实现步骤

1. 修改 `MainWindow.axaml.cs` 的迷你模式尺寸为 300×520
2. 在 `MainWindow.axaml` 中添加迷你模式覆盖层 Border
3. 实现封面+歌曲信息区布局
4. 实现歌词滚动区域（复用 PlayerPageView 的歌词模板逻辑）
5. 为原有内容添加 `{Binding !IsMiniMode}` 可见性绑定
6. 添加 StringToBrushConverter 资源引用（渐变背景需要）
7. 测试验证：切换迷你模式 → 检查布局/歌词/背景色
