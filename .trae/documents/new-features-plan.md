# LocalMusicPlayer 新功能实施计划

## 概述

为 LocalMusicPlayer 添加 6 项新功能，提升音乐播放体验。

---

## 功能一：播放模式增强（单曲循环）

### 现状
当前 `PlaybackMode` 枚举只有 `Normal`/`Shuffle`/`Loop` 三种模式。`Loop` 是列表循环，缺少单曲循环。

### 修改内容

1. **Models/PlaybackMode.cs** — 枚举添加 `SingleLoop`
2. **Services/PlaylistService.cs** — `PlayNext()` 方法中增加 `SingleLoop` 分支，单曲循环时 `_currentIndex` 不变，直接返回 `true`
3. **ViewModels/PlayerPageViewModel.cs** — `RepeatCommand` 改为三态循环：`Normal → Loop → SingleLoop → Normal`；添加 `IsSingleLoop` 属性；`IsRepeat` 在 `Loop` 和 `SingleLoop` 时都为 `true`
4. **ViewModels/MainWindowViewModel.cs** — 同步修改 `RepeatCommand` 为三态循环
5. **Views/PlayerPageView.axaml** — 单曲循环时 Repeat 图标显示 `&#xE8ED;` + 小 "1" 标记（叠加 TextBlock "1"）
6. **Views/PlayerView.axaml** — 底部播放栏 Repeat 按钮同步修改
7. **Services/IConfigurationService.cs / ConfigurationService.cs** — 保存/恢复 `SingleLoop` 模式

---

## 功能二：歌词页面渐变背景色（封面主色提取）

### 现状
`PlayerPageView` 背景是固定的 `BgPrimaryBrush`，没有根据封面动态变化。

### 修改内容

1. **Services/IAlbumArtService.cs** — 添加 `Task<Color?> ExtractDominantColorAsync(string? albumArtPath)` 方法
2. **Services/AlbumArtService.cs** — 实现主色提取：从磁盘缓存图片加载为 Bitmap，采样像素计算主色调（简化算法：将图片缩小到小尺寸，统计最频繁色相区间）
3. **ViewModels/PlayerPageViewModel.cs** — 添加 `[ObservableProperty] private Color _dominantColor`；在 `CurrentSongChanged` 时调用 `ExtractDominantColorAsync`；添加 `[ObservableProperty] private IBrush _gradientBackground`
4. **Views/PlayerPageView.axaml** — 根 Grid 的 Background 绑定到 `GradientBackground`，使用 `LinearGradientBrush` 从主色（低透明度）渐变到背景色
5. **Converters/** — 添加 `ColorToGradientBrushConverter`，将主色转为渐变画刷

---

## 功能三：播放历史

### 现状
`StatisticsService` 有 `GetRecentlyPlayedSongs` 但仅返回最近播放的 Top N，没有完整的播放历史记录功能，也没有独立的播放历史页面。

### 修改内容

1. **Models/PlayHistoryEntry.cs** — 新建模型：`Song` + `PlayedAt`（DateTime）
2. **Services/IPlayHistoryService.cs** — 新建接口：`AddToHistory(Song)` / `GetHistory()` / `ClearHistory()` / `HistoryChanged` 事件，最大保留 200 条
3. **Services/PlayHistoryService.cs** — 实现：维护 `ObservableCollection<PlayHistoryEntry>`，在 `PlaybackStateService.PlaybackEnded` 或 `CurrentSongChanged` 时记录
4. **ViewModels/PlayHistoryViewModel.cs** — 新建：展示播放历史列表，支持清空、播放选中、添加到播放列表
5. **Views/PlayHistoryView.axaml** — 新建：播放历史页面 UI
6. **ViewModels/MainWindowViewModel.cs** — 添加侧边栏"历史"导航项和导航命令
7. **Views/MainWindow.axaml** — 侧边栏添加历史图标按钮
8. **Services/IViewModelFactory.cs / ViewModelFactory.cs** — 添加 `CreatePlayHistoryViewModel` 方法
9. **Services/INavigationService.cs** — 注册 `PlayHistoryViewModel` 导航

---

## 功能四：播放次数显示

### 现状
歌曲列表的 "Plays" 列显示固定文本 "-"，没有绑定实际播放次数。`Song` 模型已有 `PlayCount` 属性，`StatisticsService` 已记录播放次数。

### 修改内容

1. **ViewModels/MainWindowViewModel.cs** — 在 `InitializeAsync` 中从 `StatisticsService` 加载播放次数到 `Song.PlayCount`；监听 `StatisticsChanged` 事件刷新
2. **Views/PlayerView.axaml** — Plays 列的 TextBlock 绑定 `{Binding PlayCount}`，PlayCount 为 0 时显示 "-"
3. **Converters/PlayCountConverter.cs** — 新建：PlayCount 为 0 显示 "-"，否则显示数字
4. **Services/StatisticsService.cs** — 在 `RecordPlayStart` 时同步更新 `Song.PlayCount` 属性

---

## 功能五：淡入淡出

### 现状
歌曲切换时直接开始播放，没有音量渐变过渡。

### 修改内容

1. **Services/IMusicPlayerService.cs** — 添加 `Task FadeInAsync(int targetVolume, TimeSpan duration)` / `Task FadeOutAsync(TimeSpan duration)` 方法
2. **Services/MusicPlayerService.cs** — 实现淡入淡出：使用 `DispatcherTimer` 分步调整 `_mediaPlayer.Volume`，每步约 50ms
3. **Services/IPlaybackStateService.cs** — 添加 `bool IsCrossfadeEnabled` 属性 / `TimeSpan CrossfadeDuration` 属性
4. **Services/PlaybackStateService.cs** — 在 `OnPlaybackEnded` 中：先对当前歌曲 `FadeOutAsync`，完成后播放下一首并 `FadeInAsync`
5. **Models/AppSettings.cs** — 添加 `bool CrossfadeEnabled` / `int CrossfadeDurationMs` 配置项
6. **ViewModels/SettingsViewModel.cs** — 添加淡入淡出开关和时长设置 UI 绑定属性
7. **Views/SettingsView.axaml** — 添加淡入淡出设置区域（开关 + 时长滑块 1-10 秒）

---

## 功能六：播放速度控制

### 现状
播放速度固定为 1.0x，无法调整。

### 修改内容

1. **Services/IMusicPlayerService.cs** — 添加 `float PlaybackRate { get; set; }` 属性
2. **Services/MusicPlayerService.cs** — 实现播放速率：通过 `_mediaPlayer.SetRate(rate)` 设置，范围 0.5x ~ 2.0x
3. **Services/IPlaybackStateService.cs** — 添加 `float PlaybackRate { get; set; }` 属性
4. **Services/PlaybackStateService.cs** — 透传 `PlaybackRate` 到 `MusicPlayerService`
5. **ViewModels/PlayerPageViewModel.cs** — 添加 `[ObservableProperty] private float _playbackRate = 1.0f`；`ChangePlaybackRateCommand`；预设速率：0.5x / 0.75x / 1.0x / 1.25x / 1.5x / 2.0x
6. **Views/PlayerPageView.axaml** — 在控制区域添加速率按钮，点击弹出速率选择弹窗
7. **Models/AppSettings.cs** — 添加 `float PlaybackRate` 配置项
8. **ViewModels/SettingsViewModel.cs** — 添加播放速率设置
9. **Views/SettingsView.axaml** — 添加播放速率设置区域

---

## 实施顺序

| 顺序 | 功能 | 依赖 | 复杂度 |
|------|------|------|--------|
| 1 | 播放次数显示 | 无 | 低 |
| 2 | 播放模式增强 | 无 | 低 |
| 3 | 播放速度控制 | 无 | 中 |
| 4 | 淡入淡出 | 播放模式增强 | 中 |
| 5 | 歌词页面渐变背景色 | 无 | 中 |
| 6 | 播放历史 | 无 | 高 |

---

## 通用修改

- **Services/ServiceCollectionExtensions.cs** 或 **App.axaml.cs** — 注册新服务 `IPlayHistoryService`
- **fc.md** — 记录新增功能
- 字符串资源 — 添加新功能相关的本地化字符串
