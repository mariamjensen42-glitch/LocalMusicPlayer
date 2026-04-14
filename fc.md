# 功能变更记录 (fc.md)

## 2026-04-14

### 重构: 删除 MusicBrowseView 中的重复 tab 导航栏
- **文件**: `Views/Library/MusicBrowseView.axaml`
- **说明**: 侧边栏已有导航功能，MusicBrowseView 中的 tab 导航栏是重复的。删除 song/album/artist/folder/favourite/playlist 等 RadioButton tab，只保留工具栏（排序选项、搜索框）。

### 功能: 在侧边栏添加"我喜欢的"导航入口
- **文件**: `Views/Main/MainWindow.axaml`
- **说明**: 收藏功能是音乐播放器的核心功能，添加"我喜欢的"入口到侧边栏，方便用户快速访问收藏的歌曲。
- **修改**: 在侧边栏添加 ToggleButton，绑定 `NavigateToFavoritesCommand`，使用 `&#xE87D;`（实心爱心图标）和 `StringFavorites` 资源

### 重构: 删除 MusicBrowseView，统一使用 HomeView
- **文件**: `ViewModels/MainWindowViewModel.cs`, `Services/Navigation/IViewModelFactory.cs`, `Services/Navigation/ViewModelFactory.cs`, `ViewModels/ViewLocator.cs`
- **说明**: 简化导航结构，统一使用 HomeView 作为主内容页面，删除 MusicBrowseView 及其相关代码。
- **修改**:
  - `NavigateToLibrary()` 跳转到 `HomeViewModel` 而非 `MusicBrowseViewModel`
  - 删除 `MusicBrowseViewModel` 属性和相关初始化代码
  - 从 `ViewLocator` 字典中移除 `MusicBrowseViewModel` 映射
  - 从 `IViewModelFactory` 和 `ViewModelFactory` 中删除 `CreateMusicBrowseViewModel()` 方法
  - 初始页面改为 `HomeViewModel`

### 修复: SongListViewModel 绑定到错误的集合
- **文件**: `ViewModels/SongListViewModel.cs`
- **说明**: "所有歌曲"页面绑定了 `FilteredSongs`（筛选结果），应该直接绑定到 `Songs`（所有歌曲），因为导航栏已经有各种分类，不需要二次筛选。
- **修改**: `public ObservableCollection<Song> Songs => _musicLibraryService.Songs;`

## 2026-04-12

### 新增: HomeViewModel
- **文件**: `ViewModels/HomeViewModel.cs`
- **说明**: 创建首页（所有歌曲页面）的视图模型
- **功能**:
  - 歌曲列表展示（通过 `_musicLibraryService.FilteredSongs`）
  - 搜索/过滤歌曲（按标题、艺术家、专辑模糊匹配）
  - 统计信息显示（格式："本地: X 首"）
  - 播放全部（`PlayAllCommand`）
  - 随机播放（`ShufflePlayCommand`，打乱顺序后播放）
  - 播放指定歌曲（`PlaySongCommand`，接收文件路径参数）
  - 切换收藏状态（`ToggleFavoriteCommand`，异步操作）
  - 当前播放状态属性（`CurrentSong`, `IsPlaying`）
  - 事件订阅：PlaybackStateChanged、CurrentSongChanged、Songs.CollectionChanged
  - 通过 ViewModelBase 的 SubscribeEvent 管理事件生命周期
- **依赖服务**: IMusicLibraryService, IPlaylistService, IPlaybackStateService, IStatisticsService, IPlayHistoryService, IUserPlaylistService

### 重构: MainWindow.axaml - Particle Music 风格 UI
- **文件**: `Views/MainWindow.axaml`
- **说明**: 完全重构主窗口布局，从窄侧边栏（72px）改为 Particle Music 风格的宽侧边栏（220px）
- **主要改动**:
  1. **整体布局结构**:
     - 采用 Grid RowDefinitions="*,Auto" 双层布局
     - Row 0: 主区域（左侧边栏 + 右侧内容区）
     - Row 1: 底部播放控制栏（72px）

  2. **左侧边栏 (220px)**:
     - 顶部应用标题："Local Music Player"（FontSize 20 Bold）
     - 8个导航项：艺术家、专辑、文件夹、所有歌曲（默认选中）、听歌排行、最近播放、歌单（带+按钮）、我喜欢的
     - 使用 SidebarNavItemTheme 样式，图标 + 文字水平排列
     - 通过 EnumToBoolConverter 实现 NavigationPage 选中状态高亮

  3. **顶部工具栏 (48px)**:
     - 返回按钮（首页隐藏，其他页面显示）
     - 搜索框居中（400px宽，圆角8px）
     - 设置按钮
     - 窗口控制按钮组（全屏/窗口化、最小化、最大化/还原、关闭）

  4. **底部播放控制栏 (72px)**:
     - 左侧（280px）：专辑封面（40x40 圆角6px）+ 歌曲信息（标题 + 艺术家）
     - 中间：
       - 上部：播放控制按钮组（循环模式、上一首、播放/暂停渐变大按钮48x48、下一首、队列）
       - 下部：进度条滑块（600px宽）+ 时间文本（当前时间 / 总时长）
     - 右侧（120px）：音量图标（静音切换）+ 音量滑块

  5. **保持现有功能**:
     - 所有窗口控制事件处理（MinimizeButton_Click, MaximizeButton_Click, CloseButton_Click）
     - 队列面板（IsQueuePanelOpen 绑定，右侧滑出）
     - 所有数据绑定和命令（NavigateTo*, PlayCommand, PauseCommand 等）
     - x:Class, x:DataType, Design.DataContext 不变

  6. **使用的新样式主题**（来自 Resources.axaml）:
     - SidebarNavItemTheme: 导航按钮样式（透明背景，hover 高亮，checked 紫色）
     - PlayButtonLargeTheme: 播放大按钮（48x48 渐变背景圆形）
     - PlayerControlButtonTheme: 控制按钮（32x32 透明背景圆形）
     - MusicProgressSliderTheme: 进度条滑块（紫粉渐变填充）
     - VolumeControlSliderTheme: 音量滑块（紫色填充）

- **技术细节**:
  - 图标字体统一使用 "Segoe Fluent Icons"
  - 导航项选中状态通过 MultiBinding + EnumToBoolConverter 实现
  - 播放/暂停按钮根据 IsPlaying 属性动态切换可见性
  - 音量图标根据 IsMuted 属性在静音/非静音图标间切换
  - 进度条和时间使用 TimeSpanToStringConverter 格式化

### 修改: MainWindowViewModel.cs - 兼容 Particle Music 风格 UI
- **文件**: `ViewModels/MainWindowViewModel.cs`
- **说明**: 调整 ViewModel 以适配新的 Particle Music 风格 UI
- **主要改动**:
  1. **SidebarWidth 属性重构**:
     - 从动态计算属性（根据 IsPlayerPageVisible 返回 0 或 72）改为固定常量 `const int SidebarWidth = 220`
     - 移除 `IsPlayerPageVisible` 属性（不再需要根据页面隐藏侧边栏）
     - 移除 `[NotifyPropertyChangedFor(nameof(IsPlayerPageVisible))]` 和 `[NotifyPropertyChangedFor(nameof(SidebarWidth))]` 特性

  2. **LibraryStats 格式调整**:
     - 默认值从 `"0 songs · 0 albums · 0h 0m"` 改为 `"本地: 0 首"`
     - `UpdateLibraryStats()` 方法简化，只显示歌曲数量
     - 新格式: `"本地: {songCount} 首"`
     - 移除了专辑数和总时长的统计逻辑

  3. **新增命令**:
     - `GoBackCommand`: 返回首页（调用 NavigateToLibrary()）
     - `ToggleFullScreenCommand`: 切换窗口全屏状态（使用 IClassicDesktopStyleApplicationLifetime）

  4. **新增引用**:
     - 添加 `using Avalonia;` 和 `using Avalonia.Controls;` 以支持 WindowState 和 IClassicDesktopStyleApplicationLifetime

- **向后兼容性**:
  - 所有现有导航、播放、搜索功能保持正常工作
  - 其他页面（Settings、Statistics、Player 等）不受影响
  - 数据绑定路径保持兼容
