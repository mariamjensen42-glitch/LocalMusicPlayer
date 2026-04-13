# 新功能记录

## 2026-04-10

### 1. 播放次数显示
- 歌曲列表 Plays 列绑定 Song.PlayCount，0 次显示 "-"
- 新增 PlayCountConverter 转换器

### 2. 播放模式增强（单曲循环）
- PlaybackMode 枚举新增 SingleLoop
- Repeat 按钮改为三态循环：Normal → Loop → SingleLoop → Normal
- 单曲循环时 Repeat 图标显示 "1" 标记
- PlaylistService.PlayNext() 支持 SingleLoop 分支

### 3. 播放速度控制
- IMusicPlayerService 新增 SetPlaybackRate / PlaybackRate
- MusicPlayerService 通过 LibVLCSharp MediaPlayer.SetRate 实现
- PlayerPageView 新增速度选择弹窗（0.5x ~ 2.0x）
- 设置页新增播放速度滑块
- PlaybackRate 持久化到 AppSettings

### 4. 淡入淡出
- IMusicPlayerService 新增 FadeInAsync / FadeOutAsync
- MusicPlayerService 使用 20 步渐变实现音量过渡
- IPlaybackStateService 新增 IsCrossfadeEnabled / CrossfadeDuration
- PlaybackStateService.OnPlaybackEnded 支持淡入淡出切换
- 设置页新增淡入淡出开关和时长滑块（1-10 秒）
- CrossfadeEnabled / CrossfadeDurationMs 持久化到 AppSettings

### 5. 歌词页面渐变背景色
- IAlbumArtService 新增 ExtractDominantColorAsync
- AlbumArtService 缩小图片采样像素计算主色调
- PlayerPageViewModel 新增 GradientBackground 属性
- PlayerPageView 背景绑定渐变画刷（主色 → 暗色 → 默认背景色）
- 切歌时自动更新背景色

### 6. 播放历史
- 新增 PlayHistoryEntry 模型（Song + PlayedAt + FormattedPlayedAt）
- 新增 IPlayHistoryService / PlayHistoryService（最多 200 条记录）
- 新增 PlayHistoryViewModel / PlayHistoryView
- 侧边栏新增历史导航图标（时钟图标）
- 播放歌曲时自动记录到历史
- 支持清空历史、播放历史歌曲

### 7. 歌词显示设置
- 歌词字体大小调节（14~48px），歌词区顶部 +/- 按钮
- 歌词行距调节，通过 LyricLineSpacing 属性控制
- 翻译歌词显隐切换，点击翻译图标按钮
- 翻译字号自动按原文字号 0.57 比例缩放
- 新增 LyricTranslationFontSizeConverter 转换器
- LyricFontSize / LyricLineSpacing / ShowTranslation 持久化到 AppSettings

### 8. 音乐库去重
- 新增 IDedupService / DedupService
- DuplicateGroup 模型：按 Title + Artist + Duration + FileSizeBytes 匹配
- FindDuplicates() 返回重复歌曲分组列表
- RemoveDuplicate() 标记保留歌曲（TODO: 实际删除）

### 9. 音量归一化 (ReplayGain)
- Song 模型新增 ReplayGainTrackGain / FileSizeBytes 属性
- IMusicPlayerService 新增 SetReplayGainEnabled / ApplyReplayGain
- MusicPlayerService 播放时根据 ReplayGainTrackGain 调整音量
- IPlaybackStateService 新增 IsReplayGainEnabled 属性
- 设置页新增 ReplayGain 开关（设置 → 播放效果 → 音量归一化）
- ReplayGainEnabled 持久化到 AppSettings
- 注意：TagLibSharp 暂未自动读取音轨增益标签，需手动配置

### 10. 播放历史持久化
- 新增 PlayHistoryRecord 模型（FilePath + PlayedAt）用于 JSON 序列化
- AppSettings 新增 PlayHistory 列表属性
- PlayHistoryService 构造函数注入 IConfigurationService 和 IMusicLibraryService
- 启动时自动从配置加载历史记录，根据文件路径匹配音乐库中的歌曲
- 播放历史变更时自动保存到配置文件
- 最多保存 200 条记录

### 11. 数据统计和报告
- 新增 StatisticsReportViewModel
  - 使用 CommunityToolkit.Mvvm 实现
  - 总览统计属性（总歌曲数、总时长、总播放次数）
  - 排行数据集合（TopArtists, TopAlbums, TopSongs）
  - 流派分布数据集合
  - 收听历史数据集合
  - 刷新命令和加载命令
  - 时间范围筛选（7天/30天/90天/1年）
- 新增 StatisticsReportView.axaml
  - 总览统计卡片布局（4个统计卡片）
  - Top艺术家/专辑/歌曲列表
  - 流派分布显示（带进度条）
  - 收听历史列表
  - 刷新按钮和时间范围筛选按钮
  - 使用 x:DataType 编译绑定
- 新增 StatisticsReportView.axaml.cs
- PlaybackStateService 集成统计触发
  - 注入 IStatisticsService
  - 跟踪歌曲播放进度
  - 播放超过50%时长时调用 TrackPlayAsync
  - 在歌曲切换、播放结束、暂停时正确统计

### 12. 音乐库管理增强功能
- **多维分类浏览**
  - 新增 LibraryBrowserViewModel / LibraryBrowserView
  - 支持按歌曲/艺术家/专辑/流派/文件夹浏览
  - 左侧 Sidebar 切换分类，右侧显示对应列表
  - 专辑网格视图显示封面
  - 文件夹树形结构浏览
  - 扩展 IMusicLibraryService（GetArtists, GetAlbums, GetGenres 等）
- **批量标签编辑**
  - 新增 BatchMetadataEditorViewModel / BatchMetadataEditorView
  - 支持多选歌曲（Ctrl/Shift+点击）
  - 自动检测共同字段，混合值显示 "<Mixed Values>"
  - 批量保存带进度条
  - PlaylistManagementView 添加右键菜单"批量编辑"选项
- **文件管理**
  - 新增 IFileManagerService / FileManagerService
  - 支持文件重命名（命名模板如 "{Artist} - {Title}.mp3"）
  - 支持文件移动、复制、删除
  - 批量操作带进度报告（IProgress<FileOperationProgress>）
  - 后台线程执行，线程安全
- **封面管理**
  - 新增 ICoverManagerService / CoverManagerService
  - 使用 TagLibSharp 读取/写入内嵌封面
  - 本地缓存机制（%AppData%/LocalMusicPlayer/covercache/）
  - 支持从本地文件设置封面
  - 支持将封面嵌入音频文件
- **数据统计增强**
  - Song 模型扩展（PlayCount, LastPlayedAt, AddedAt）
  - IStatisticsService 扩展（TrackPlay, GetReport, GetHistory, GetTopArtists 等）
  - 数据持久化到 JSON 文件
  - 收听报告包含总览、排行、趋势、流派分布
- **UI 集成**
  - 主窗口添加"音乐库"和"统计报告"导航入口
  - 集成到 MainWindowViewModel 导航系统
  - 播放统计自动触发（超过50%时长）

## 2026-04-11

### 端到端测试验证

**构建状态**: ✅ 成功（1 个警告：CoverManagerService._jpegQuality 未使用）

**启动测试**: ✅ 应用程序成功启动，无崩溃

**核心功能代码审查**:
- ✅ EF Core SQLite 数据库初始化正常（DatabaseService.InitializeAsync）
- ✅ ConfigurationService 从 JSON 迁移到 SQLite 逻辑完整
- ✅ UserPlaylistService 收藏夹和播放列表 CRUD 操作正常
- ✅ MusicPlayerService 使用 LibVLCSharp 播放音频
- ✅ 依赖注入配置完整（AppDbContext 注册为 Singleton）
- ✅ 播放状态持久化（Closing 事件保存队列和位置）

**遗留警告**: CoverManagerService._jpegQuality 字段已赋值但未使用

### 1. ConfigurationService 重写为 EF Core SQLite 存储
- 注入 AppDbContext 替代 JSON 文件读写
- CurrentSettings 从 AppSettingsEntity（Key-Value 表）加载
- LoadSettingsAsync 从数据库读取所有设置项，反序列化为 AppSettings
- SaveSettingsAsync 将每个属性序列化为 JSON 写入数据库
- JSON 迁移逻辑：首次启动检测 %AppData%/LocalMusicPlayer/settings.json，导入 SQLite 后删除 JSON 文件
- 通过 _MigratedFromJson 标记防止重复迁移
- GetScanFolders / AddScanFolderAsync / RemoveScanFolderAsync 操作数据库
- IConfigurationService 接口保持不变

### 2. UserPlaylistService 重写为 EF Core SQLite 存储
- 注入 AppDbContext 和 IMusicLibraryService 替代 IConfigurationService
- AddToFavorites/RemoveFromFavorites 操作 FavoriteEntity 表，立即 SaveChanges
- GetFavoriteSongs 从数据库查询 FavoriteEntity，再匹配 IMusicLibraryService.Songs
- IsFavorite 从数据库查询
- 播放列表 CRUD 操作 PlaylistEntity 和 PlaylistSongEntity 表
- 歌曲库变更时通过 Songs.CollectionChanged 事件同步收藏状态和清理无效引用
- LoadPlaylistsAsync 从数据库加载播放列表到内存 ObservableCollection
- SavePlaylistsAsync 将内存播放列表同步到数据库（增删改全量对比）
- ExportPlaylistAsync/ImportPlaylistAsync 保持 JSON 文件导入导出
- IUserPlaylistService 接口保持不变
- App.axaml.cs 注册 AppDbContext 为 Singleton 服务

### 13. 首页"所有歌曲"视图（HomeView）
- 新增 HomeView.axaml / HomeView.axaml.cs
- **顶部区域**：
  - 左侧：180x180 专辑封面卡片（AlbumCoverCardTheme）
  - 右侧：标题 "所有歌曲"、统计信息（LibraryStats）、操作按钮组
  - 播放全部按钮（PrimaryButtonTheme，渐变背景 + 播放图标）
  - 随机播放按钮（SecondaryButtonTheme + 随机图标）
- **歌曲列表表格**：
  - 表头：6列布局（序号、封面、标题&艺术家、专辑、喜欢、时长）
  - 使用 ListBox + VirtualizationMode.Recycling 实现虚拟化
  - 自定义 ItemContainerTheme 移除默认样式
  - DataTemplate 绑定 Song 模型
- **歌曲行功能**：
  - 序号列：默认显示 TrackNumber，当前播放时显示播放图标
  - 封面列：40x40 圆角缩略图，使用 AlbumArtConverter
  - 标题列：当前播放行高亮为 AccentPurple 色
  - 喜欢按钮：空心心/实心心切换，绑定 ToggleFavoriteCommand
  - 时长列：TimeSpanToStringConverter 格式化显示
  - 双击播放：TapCommandBehavior (TapCount=2) → PlaySongCommand
- **样式应用**：
  - 使用 SongListItemRowTheme 实现悬停效果
  - 使用 SongIsPlayingConverter 判断当前播放状态
  - 使用 BoolToColorConverter 控制喜欢图标显隐
- **编译绑定**：x:CompileBindings="True"，x:DataType="vm:MainWindowViewModel"
- **DataContext**：MainWindowViewModel（通过父级传递）

## 2026-04-12

### 1. 依赖注入规范化重构
- **新增 ServiceCollectionExtensions**（Helpers/ServiceCollectionExtensions.cs）
  - 按功能分类组织服务注册：Navigation、Playback、Library、Media、Playlist、System、Statistics、File
  - `AddMusicServices()` 扩展方法统一注册所有服务和 ViewModel
- **App.axaml.cs 重构**
  - `IServiceProvider` 从 `public` 改为 `private`，消除全局服务定位器入口
  - 使用 `AddMusicServices()` 替代内联 `ConfigureServices()` 方法
  - Composition Root 中通过构造函数注入将服务传递给 MainWindow
- **MainWindow.axaml.cs 重构**
  - 消除 `(App.Current as App)?.Services?.GetService<T>()` 服务定位器反模式
  - `IDropHandlerService`、`IKeyboardShortcutService` 改为构造函数注入
  - 移除 `Microsoft.Extensions.DependencyInjection` 引用
- **ViewModelFactory 重构**
  - 从手动传递 15 个依赖改为注入 `IServiceProvider` + `ActivatorUtilities.CreateInstance`
  - 新增 `CreateMetadataEditorViewModel`、`CreateBatchMetadataEditorViewModel` 工厂方法
  - 消除每次新增 ViewModel 参数时需同步修改工厂的维护负担
- **DialogService 重构**
  - 注入 `IViewModelFactory` 替代直接 `new MetadataEditorViewModel` / `new BatchMetadataEditorViewModel`
  - ViewModel 创建统一通过工厂，符合 DI 规范

### 2. 全局「添加到歌单」右键菜单
- **IDialogService 扩展**
  - 新增 `ShowAddToPlaylistDialogAsync(Song song)` 方法
- **DialogService 实现**
  - 弹窗显示所有用户歌单列表（排除 Favorites）
  - 点击歌单项执行添加并关闭弹窗
  - 无歌单时提示用户先创建
  - 注入 `IUserPlaylistService` 实现添加逻辑
- **ViewModel 层**
  - DetailViewModelBase 新增 `AddToPlaylistCommand`（AlbumDetail/ArtistDetail 自动继承）
  - HomeViewModel 新增 `AddToPlaylistCommand`
  - PlaylistManagementViewModel 新增 `AddToPlaylistCommand`
- **View 层**
  - HomeView 歌曲行新增 ContextMenu「添加到歌单」
  - AlbumDetailView 歌曲行新增 ContextMenu「添加到歌单」
  - ArtistDetailView 歌曲行新增 ContextMenu「添加到歌单」
  - PlaylistManagementView 右键菜单追加「添加到歌单」选项
- **构造函数更新**
  - DialogService 构造函数新增 `IUserPlaylistService` 参数
  - DetailViewModelBase 构造函数新增 `IDialogService` 参数
  - AlbumDetailViewModel / ArtistDetailViewModel 透传新参数
  - HomeViewModel 新增 `IDialogService` 参数

### 3. 歌单列表页 + 导航流程
- **新增 PlaylistListViewModel** (ViewModels/PlaylistListViewModel.cs)
  - 展示所有 UserPlaylists 卡片网格
  - CreatePlaylistCommand：弹窗输入创建新歌单
  - SelectPlaylist：选中歌单触发 OnPlaylistSelected 回调
- **新增 PlaylistListView** (Views/Playlist/PlaylistListView.axaml)
  - WrapPanel 网格布局，每个歌单卡片显示图标、名称、歌曲数量
  - 点击卡片 → SelectPlaylistCommand → 导航到歌单详情
  - 顶部「+ 新建播放列表」按钮
- **MainWindowViewModel 更新**
  - NavigateToPlaylist 改为导航到 PlaylistListViewModel（歌单列表页）
  - 新增 NavigateToPlaylistDetail 方法，接收 UserPlaylist 参数，导航到 PlaylistManagementViewModel
  - 通过 OnPlaylistSelected 回调实现列表→详情的导航跳转
- **PlaylistManagementViewModel**
  - 新增 SetSelectedPlaylist 公开方法，供外部设置初始选中歌单
- **IViewModelFactory / ViewModelFactory / ViewLocator**
  - 注册 CreatePlaylistListViewModel
  - 映射 PlaylistListViewModel → PlaylistListView
  - MainWindowViewModel 添加 PlaylistListViewModel 页面类型检查

### 4. 设置页面扩展 — 系统托盘/通知 + 启动行为
- **AppSettings 新增 4 个属性**
  - `MinimizeToTray`（默认 true）：关闭时最小化到系统托盘
  - `ShowSongChangeNotification`（默认 true）：歌曲切换时显示通知
  - `AutoStartOnBoot`（默认 false）：开机时自动启动
  - `ResumeLastPlayback`（默认 true）：启动时恢复上次播放
- **ConfigurationService 扩展**：LoadSettingsAsync / SaveSettingsAsync 新增 4 个字段读写
- **新增 IAutoStartService / AutoStartService**
  - 通过 Windows 注册表 `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` 管理开机自启
  - `SetAutoStartAsync(bool)` 写入/删除注册表项
  - `IsAutoStartEnabled()` 检查注册表状态
- **SystemTrayService 改造**
  - 构造函数注入 IConfigurationService
  - Initialize() 根据 MinimizeToTray 决定是否拦截 Closing 事件（Hide 而非 Close）
  - UpdateTrayIcon(bool) 实现为更新 ToolTipText（显示播放状态）
  - ShowNotification(string, string) 实现为更新 ToolTipText（Avalonia TrayIcon 无原生通知 API）
- **MainWindow.axaml.cs 改造**
  - 构造函数注入 IConfigurationService
  - CloseButton_Click 根据 MinimizeToTray 决定 Hide() 或 Close()
- **MainWindowViewModel 改造**
  - OnCurrentSongChanged 根据 ShowSongChangeNotification 决定是否调用 ShowNotification
- **App.axaml.cs 集成恢复播放逻辑**
  - MainWindow Loaded 回调中检查 ResumeLastPlayback
  - 启用时从 LastSongFilePath/QueueFilePaths/LastPlaybackPosition 恢复播放状态
  - 通过 IMusicLibraryService 匹配文件路径、IPlaylistService 恢复队列和播放、IPlaybackStateService Seek 到上次位置
- **DI 注册**：ServiceCollectionExtensions.AddSystemServices() 注册 IAutoStartService/AutoStartService
- **SettingsViewModel 扩展**
  - 注入 IAutoStartService
  - 新增 4 个 ObservableProperty + 4 个 OnXxxChanged partial 方法自动保存设置
  - AutoStartOnBoot 变更时调用 _autoStartService.SetAutoStartAsync 写入注册表
- **Strings.axaml 新增 8 个字符串资源**：系统托盘相关 4 个 + 启动行为相关 4 个
- **SettingsView.axaml 新增 2 个卡片**
  - 系统托盘卡片：MinimizeToTray ToggleSwitch + ShowSongChangeNotification ToggleSwitch
  - 启动行为卡片：AutoStartOnBoot ToggleSwitch + ResumeLastPlayback ToggleSwitch

### 5. 歌词搜索结果弹窗
- **IDialogService 扩展**
  - 新增 `ShowLyricSearchResultDialogAsync(Song song, OnlineLyricResult? result)` 方法声明
- **DialogService 实现**
  - 弹窗显示歌曲信息（标题 + 艺术家）
  - 找到歌词时：显示歌词预览（前15行）+ 来源信息 + "Use These Lyrics" / "Skip" 按钮
  - 未找到歌词时：显示提示信息 + "Close" 按钮
  - 返回 `OnlineLyricResult?` 让用户选择是否使用搜索到的歌词
- **PlayerPageViewModel 更新**
  - 构造函数新增 `IDialogService` 参数注入
  - `SearchOnlineLyricsAsync` 方法改造为先搜索，弹窗显示结果，用户确认后更新歌词
  - 错误时通过 `ShowMessageDialogAsync` 显示错误信息
- **交互流程**
  - 点击搜索歌词按钮 → 搜索中状态
  - 搜索完成 → 弹窗预览歌词
  - 用户选择"使用这些歌词" → 更新歌词显示
  - 用户选择"跳过" → 保持原有歌词状态

## 2026-04-14

### 1. 迷你模式（歌词型小窗）
- **替换现有迷你模式**：从 400×90 横向控制条改为 300×520 竖向歌词小窗
- **布局结构**：
  - 顶部区域（Auto）：64×64 圆角专辑封面（带阴影）+ 歌曲名（15px SemiBold 白色） + 歌手名（12px Secondary 色）
  - 底部区域（*）：歌词滚动显示区，支持自动滚动到当前行
- **歌词展示**：
  - 复用 PlayerPageViewModel.Lyrics 数据源
  - 当前行高亮（Bold + 紫色），非当前行淡化（Normal + Muted 灰色）
  - 支持翻译歌词显示（11px Muted 色，70% 透明度）
  - 使用 LyricsAutoScrollBehavior 实现自动跟随滚动
  - 无歌词时显示提示文本
- **背景**：复用 PlayerPageViewModel.GradientBackground 渐变色（跟随专辑主色调）
- **交互**：
  - 整个窗口可拖拽（WindowDragBehavior）
  - 置顶显示（Topmost=true）、不可调整大小（CanResize=false）
  - 点击底部播放栏迷你模式按钮切换进入/退出
- **实现方式**：方案 A — 同窗切换，在 MainWindow.axaml 根 Grid 中添加 ZIndex=300 的覆盖层 Border
  - 原有主内容（侧边栏、内容区、底部栏）通过 `IsVisible="{Binding !IsMiniMode}"` 隐藏
  - 迷你模式覆盖层通过 `IsVisible="{Binding IsMiniMode}"` 显示
- **修改文件**：
  - `Views/Main/MainWindow.axaml`：新增迷你模式覆盖层（~110 行 XAML）、添加 services 命名空间和转换器资源引用、原有内容绑定 !IsMiniMode 隐藏
  - `Views/Main/MainWindow.axaml.cs`：迷你模式尺寸从 400×90 改为 300×520

### 2. UI 大改 — WinUI3 控件转换为 Avalonia

参考 original-sound-hq-player-win2d-navi (WinUI3) 项目，将核心控件和样式转换为 Avalonia 版本。

**新增控件（Controls/ 目录）**：

- **AlbumArtControl**（AlbumArtControl.axaml/.cs）
  - 交叉淡入淡出动画：切换封面时旧图淡出新图淡入（400ms CubicEaseOut）
  - 圆角遮罩 + 投影阴影（DropShadowEffect）
  - 图片去重机制（ComputeFastHash 采样哈希）
  - 依赖属性：ImageBytes, IsDark, CornerRadiusValue, IsShadowEnabled, IsActive
  - 双 Image 控件交替显示实现平滑过渡

- **GradientBackgroundControl**（GradientBackgroundControl.axaml/.cs）
  - 流体渐变背景：4 个 Ellipse + BlurEffect + 位置动画模拟流体效果
  - 从封面图提取 4 个主色调（自研颜色提取算法，无需 ColorThief 依赖）
  - 颜色平滑过渡动画（800ms CubicEaseOut）
  - 亮/暗主题自适应（IsImageDark 亮度检测 + ScalePaletteLuminance 亮度调整）
  - 依赖属性：IsBackgroundEnable, ImageBytes, IsDark, UseImageDominantTheme
  - 事件：ThemeResolved

- **LyricsLineControl**（LyricsLineControl.axaml/.cs）
  - 双层文本结构：底层半透明（Opacity=0.5）+ 顶层高亮
  - 逐字高亮动画：通过 ClipToBounds + Width 动画实现裁剪效果
  - 翻译文本显示/隐藏
  - 依赖属性：LyricsText, TranslateText, IsTranslationVisible, LyricsFontSize, TranslateFontSize, IsCurrentLine, LineAnimateDuration, IsWFWLyrics, ActiveForeground, InactiveForeground

**样式系统升级（Styles/Resources.axaml）**：

- **CircularHoverButtonTheme**：圆形悬停按钮，透明背景 + 椭圆遮罩悬停效果（参考 WinUI3 CircularHoverButtonStyle）
- **NoHoverButtonTheme**：无悬停效果按钮
- **GhostSliderTheme**：极简风格滑块，小 Thumb（10x6 圆角矩形），细轨道
- **AnimatedMenuItemTheme**：菜单项主题（预留悬停动画）

**PlayerPageView 重构**：

- 集成 GradientBackgroundControl 替代简单渐变背景
- 集成 AlbumArtControl 替代 Image + Border 封面显示
- 集成 LyricsLineControl 替代原有歌词模板
- 歌词区添加 OpacityMask 上下渐变淡出效果（LinearGradientBrush）
- 所有按钮升级为 CircularHoverButtonTheme

**MainWindow 布局调整**：

- 侧边栏从 220px 文字导航改为 48px 紧凑图标导航栏（参考 WinUI3 NavigationView LeftCompact 模式）
- 每个导航项仅显示图标 + ToolTip
- 底部设置按钮移至侧边栏底部
- 底部播放栏按钮升级为 CircularHoverButtonTheme

**PlayerPageViewModel 扩展**：

- 新增 ImageBytes 属性（byte[]?）：从封面缓存文件读取字节数据，供 AlbumArtControl 和 GradientBackgroundControl 使用
- 新增 IsBackgroundEnabled 属性（bool）：控制渐变背景是否启用
- UpdateGradientBackgroundAsync 方法同时更新 ImageBytes

### 3. UI 大改 — 专辑页面复刻 (WinUI3 → Avalonia)

- **日期**: 2026-04-14
- **参考**: original-sound-hq-player-win2d-navi View/AlbumPage.xaml, View/SongCollectionPage.xaml
- **新增文件**:
  - `Models/AlbumGrouping.cs` — 分组容器模型（GroupKey + Albums 集合）
  - `Converters/UI/BoolToFavouriteIconConverter.cs` — 布尔→收藏图标转换器（实心心/空心心）
- **修改文件**:
  - `ViewModels/AlbumsPageViewModel.cs`:
    - 新增 GroupedAlbums 属性（ObservableCollection<AlbumGrouping>，按首字母 A-Z/# 分组）
    - 新增 BuildGroupedAlbums() 分组方法（GetGroupKey 按首字母分类）
    - 新增 SelectItemCommand / PlayCommand / AddToFavourCommand / ShowPropertyWindowCommand
    - 新增 OnNavigateToDetail 回调（点击卡片导航到详情页）
  - `Views/Library/AlbumsPageView.axaml`:
    - 从简单 UniformGrid 卡片布局改为分组卡片网格
    - 外层 ItemsControl 绑定 GroupedAlbums，每组显示首字母标题 + WrapPanel 卡片网格
    - 每张卡片：150×150 封面 + 专辑名(13px SemiBold) + 艺术家名 | 歌曲数"首"
    - 使用 AlbumCardTheme 样式（hover 变色）、AlbumGroupHeaderTheme（28px 粗体分组头）
    - TapCommandBehavior 点击导航，RelativeSource AncestorType 绑定解决嵌套层级问题
  - `ViewModels/AlbumDetailViewModel.cs`:
    - 新增 SecondTitle 属性（所有艺术家 join " · "）
    - 新增 ThirdTitle 属性（"年份 · N 首"格式）
    - 新增 SelectedSong / SelectedSongs 属性
    - 新增 ToggleFavoriteCommand / AddToCurrentPlaylistCommand / OpenInExplorerCommand / NavigateToArtistCommand / NavigateToAlbumCommand
  - `Views/Details/AlbumDetailView.axaml`:
    - 从基础详情改为原版 SongCollectionPage 布局
    - **信息头区域**：150×150 封面(DetailCoverTheme) + 30px Bold 专辑名 + 20px 艺术家副标题 + 14px 年份·歌曲数 + 操作按钮行（播放全部 PrimaryButton / 添加到 SecondaryButton / 编辑 SecondaryButton）
    - **歌曲列表**：11 列表格布局（封面44px + 收藏Auto + 标题3* + 艺术家1.5* + 时长0.5* + 队列0.5*）
    - 表头使用 SongTableHeaderTheme（圆角12,12,0,0）
    - 歌曲行：封面缩略图(40×40) + 收藏按钮(BoolToFavouriteIconConverter) + 标题(14px Medium) + 艺术家(可点击导航) + 时长(TimeSpanToStringConverter) + 添加队列按钮
    - 双击播放(TapCommandBehavior) + 右键菜单(ContextMenu: 播放/收藏/添加歌单/队列/属性)
    - ListBoxItem 自定义 Theme 移除默认选中样式
    - 空状态提示（无歌曲时显示图标+文字）
- **样式系统** (`Styles/Resources.axaml`):
  - AlbumCardTheme：专辑卡片容器（BgCardBrush 背景 + CardCornerRadius 圆角 + hover BgCardHoverBrush）
  - AlbumGroupHeaderTheme：分组标题（28px SemiBold TextPrimaryBrush）
  - SongTableHeaderTheme：表头容器（BgTableHeaderBrush + CornerRadius 12,12,0,0）
  - DetailCoverTheme：详情页封面框（150×150 + CornerRadius=5 + ClipToBounds）
- **字符串资源** (`Styles/Strings.axaml`):
  - 新增 StringAddToFavourite、StringFavorite、StringProperties、StringSongsUnit、StringAddTo、StringOpenLocation

### 4. 歌曲列表页 ViewModel（SongListViewModel）

- **新增文件**: `ViewModels/SongListViewModel.cs`
- **继承**: ViewModelBase，使用 CommunityToolkit.Mvvm 的 [ObservableProperty] 和 [RelayCommand]
- **构造函数注入**:
  - IMusicLibraryService — 歌曲库服务
  - IPlaylistService — 播放列表服务
  - IPlaybackStateService — 播放状态服务
  - INavigationService — 导航服务
  - IDialogService — 对话框服务
  - IUserPlaylistService — 用户歌单服务
  - IFileManagerService — 文件管理服务
  - ILogger<SongListViewModel> — 日志服务
  - IOnlineLyricsService? (可选) — 在线歌词服务
- **属性**:
  - Songs → 绑定到 IMusicLibraryService.FilteredSongs
  - SelectedSong (Song?) — 单选当前选中歌曲
  - SelectedSongs (IList<Song>) — 多选选中歌曲列表
  - MenuOptions (ObservableCollection<MenuOption>) — 右键菜单选项集合
- **MenuOption 模型** (同文件定义):
  - Title: 菜单标题
  - Tag: 标识符（用于命令路由）
  - Children: 子菜单项集合
- **右键菜单（10 个主菜单项）**:
  1. Play → PlayCommand
  2. AddToFavourite → AddToFavouriteCommand
  3. AddToPlayList → AddToPlayListCommand（动态子菜单，从 IUserPlaylistService.UserPlaylists 加载）
  4. ConvertAudio → ConvertAudioCommand（子菜单: WAV/MP3/FLAC/OGG/OPUS）
  5. AddToCurrentPlayList → AddToCurrentPlayListCommand
  6. ReGetLyrics → ReGetLyricsCommand
  7. OpenInExplorer → OpenInExplorerCommand
  8. Properties → MusicDetailCommand
  9. Delete → DeleteMenuItemCommand
- **核心命令**:
  - DoubleTapPlayAsync(): 双击播放并将整个列表设为播放队列
  - PlayAsync(): 支持单选/多选播放
  - AddToFavouriteAsync(): 批量收藏/取消收藏
  - AddToPlayListAsync(object? tag): 添加到指定用户歌单
  - ConvertAudioAsync(string? format): 音频格式转换（显示进度对话框）
  - AddToCurrentPlayListAsync(): 批量添加到当前播放列表
  - ReGetLyricsAsync(): 重新获取在线歌词（需 IOnlineLyricsService）
  - OpenInExplorer(): 启动资源管理器并选中文件
  - MusicDetailAsync(): 显示歌曲属性编辑器
  - DeleteMenuItemAsync(): 确认后从磁盘删除文件并从音乐库移除
  - NavigateToArtist(string?): 点击艺人名导航
  - NavigateToAlbum(string?): 点击专辑名导航
- **多选支持**:
  - GetEffectiveSelectedSongs() 方法统一处理：优先使用 SelectedSongs，回退到 SelectedSong
  - 所有操作命令均支持批量处理
- **事件通知**:
  - ScrollToCurrentSongRequested 事件：通知 View 滚动到当前播放歌曲
  - ScrollToCurrentSong() 方法：自动定位并触发滚动
- **生命周期**:
  - 订阅 CurrentSongChanged 自动跟踪播放位置
  - 订阅 PlaylistsChanged 动态更新歌单子菜单
  - DisposeCore 正确释放所有订阅

### 5. 音乐浏览主页面（MusicBrowseView）

- **日期**: 2026-04-14
- **参考**: original-sound-hq-player-win2d-navi View/MusicBrowsePage/MusicBrowsePage.xaml
- **新增文件**:
  - `Views/Library/MusicBrowseView.axaml` — 音乐浏览主页面 XAML 视图
  - `Views/Library/MusicBrowseView.axaml.cs` — 代码后置文件
- **布局结构** (Grid RowDefinitions="Auto,*,Auto"):

  **顶部区域 (Row=0, Margin="48,0,0,0")**:
  - **标签导航栏**: Horizontal StackPanel 包含 6 个 RadioButton
    - song/album/artist/folder/favourite/playlist 标签页
    - 使用 TabRadioButtonTheme 样式（选中时 AccentPurple 背景 + 白色文字）
    - 绑定 SelectTabCommand + CommandParameter 切换标签
    - 国际化文本：StringTabSong/StringTabAlbum/StringTabArtist/StringTabFolder/StringTabFavourite/StringTabPlaylist
  - **工具栏**: Right-aligned StackPanel
    - ProgressRing: 绑定 IsLoading 显示加载状态
    - ComboBox Width=130: 排序选项下拉框，绑定 SortOptions/SelectedSortOption
    - TextBox Width=200: 搜索框，绑定 SearchText，水印 StringSearchMusic

  **中间内容区域 (Row=1)**:
  - ContentControl: 绑定 CurrentContent 属性，用于动态加载子视图（歌曲列表、专辑列表等）
  - Margin="10" 保持间距

  **底部播放控制栏 (Row=2, Height=120, Margin="10,0,10,10")**:
  - Border 容器：PlayerBarBgBrush 背景 + CornerRadius=12 + BorderSubtleBrush 边框
  - **进度条区域 (Row=0)**:
    - Grid ColumnDefinitions="*,Auto"
    - Slider: MusicProgressSliderTheme 样式，绑定 DurationSeconds/PositionSeconds
    - TextBlock: 显示 "当前时间 / 总时长"（TimeSpanToStringConverter 格式化）
  - **控制区域 (Row=1)**: Grid ColumnDefinitions="*,Auto,*"

    **左列 — 当前播放信息**:
    - StackPanel Orientation=Horizontal, Spacing=12
    - 封面按钮 (80×80, CornerRadius=5): AlbumArtConverter 转换封面路径 → Image
      - 命令: ShowCurrentSongDetailCommand（点击打开详情）
    - 歌曲信息 Grid (MaxWidth=300):
      - Row0: 歌曲标题 TextBlock (18px Bold, CharacterEllipsis)
      - Row1: 专辑名 Button (12px, 可点击导航到专辑)
      - Row2: 艺术家名 Button (12px, 可点击导航到艺术家)
      - Row3: 音频信息 TextBlock (12px Muted 色)

    **中列 — 播放控制按钮组** (Grid 7列 Auto):
    - 收藏按钮: PlayerControlButtonTheme + BoolToColorConverter 切换心形图标
    - 快退按钮 (-5秒): FastBackwardCommand
    - 上一曲按钮: PreviousCommand
    - **播放/暂停按钮**: PlayButtonLargeTheme (48×48 渐变圆形) + PlayStatusToIconConverter
    - 下一曲按钮: NextCommand
    - 快进按钮 (+5秒): FastForwardCommand
    - 停止按钮: StopCommand

    **右列 — 其他控制**:
    - StackPanel Orientation=Horizontal, Spacing=8
    - 均衡器按钮: ShowEqualizerCommand
    - 播放列表按钮: ShowPlaylistCommand
    - 播放模式按钮: PlayModeChangedCommand + PlayModeToIconConverter
    - 音量图标按钮: MuteToggleCommand + VolumeToIconConverter
    - 音量滑块 Slider (Width=100): VolumeControlSliderTheme, Min=0, Max=100

- **样式应用**:
  - TabRadioButtonTheme: 标签导航单选按钮样式
  - TransparentIconButtonTheme: 透明图标按钮
  - NoHoverButtonTheme: 无悬停效果按钮（用于可点击文本）
  - PlayerControlButtonTheme: 播放控制按钮 (32×32 透明圆形)
  - PlayButtonLargeTheme: 大型播放按钮 (48×48 渐变背景圆形)
  - MusicProgressSliderTheme: 音乐进度滑块（紫粉渐变轨道）
  - VolumeControlSliderTheme: 音量控制滑块（紫色填充轨道）
  - PlayerBarBgBrush: 播放栏背景色 (#FF0F0F0F)

- **转换器引用**:
  - TimeSpanToStringConverter: 时间格式化
  - AlbumArtConverter: 封面路径→ImageSource
  - BoolToColorConverter: 布尔值→颜色/显隐
  - PlayModeToIconConverter: 播放模式→Segoe Fluent Icons 字符
  - VolumeToIconConverter: 音量等级→音量图标字符
  - FavouriteIconConverter: 收藏状态→心形图标字符
  - PlayStatusToIconConverter: 播放状态→播放/暂停图标字符

- **ViewModel 绑定**:
  - x:DataType="vm:MusicBrowseViewModel"
  - x:CompileBindings="False" (使用 DynamicResource 国际化)
  - 数据源: MusicBrowseViewModel (继承 ViewModelBase)
  - 关键属性: SelectedTab, SearchText, CurrentContent, CurrentSong, IsPlaying,
    Position, Duration, PositionSeconds, DurationSeconds, Volume, PlaybackMode, IsLoading

- **设计规范遵循**:
  - ✅ 使用设计系统资源（BgCardBrush, TextPrimaryBrush, BorderSubtleBrush 等）
  - ✅ 所有文本使用 DynamicResource 国际化字符串
  - ✅ 按钮使用预定义 Theme 样式（TransparentIconButtonTheme, PlayerControlButtonTheme 等）
  - ✅ 图标使用 Segoe Fluent Icons 字体图标
  - ✅ 符合 MVVM 模式，无代码隐藏逻辑
  - ✅ 响应式布局，Grid 自适应不同窗口尺寸

### 6. 文件夹浏览页面（FolderBrowseView）

- **日期**: 2026-04-14
- **参考**: original-sound-hq-player-win2d-navi View/FolderBrowsePage.xaml
- **新增文件**:
  - `Views/Library/FolderBrowseView.axaml` — 文件夹网格浏览视图
  - `Views/Library/FolderBrowseView.axaml.cs` — 代码后置文件
- **布局结构** (Grid Margin="10,0,0,0"):

  **主内容区域 — ScrollViewer + ItemsControl**:
  - ItemsControl.ItemsPanel: WrapPanel（自适应换行网格）
  - ItemsSource: 绑定 FolderBrowseViewModel.Folders (ObservableCollection<FolderGroup>)
  - ItemTemplate DataTemplate x:DataType="models:FolderGroup":

    **每个文件夹卡片 (Border Width=180)**:
    - 样式: BgCardBrush 背景 + CornerRadius=12 + BorderSubtleBrush 边框 + Margin="0,0,16,16"
    - 点击交互: TapCommandBehavior → NavigateToFolderSongsCommand（传递当前 FolderGroup）
    - 右键菜单 ContextMenu（3 个菜单项）:
      - 播放 (StringPlay) → PlayCommand
      - 添加到收藏 (StringAddToFavourite) → AddToFavouriteCommand
      - 重新扫描 (StringMenuRescan) → RescanFolderCommand

    **内部 StackPanel Spacing=8 Margin=12**:

    *封面区域* (Border 156×156):
    - CornerRadius=8 + BgElement2Brush 背景 + ClipToBounds=True
    - 底层: 文件夹图标 TextBlock "" (Segoe Fluent Icons, 50px, TextMutedBrush)
    - 上层: Image Stretch=UniformToFill（AlbumArtConverter 转换 CoverArtPath）
    - 当无封面时显示文件夹图标，有封面时图片覆盖图标

    *文字信息*:
    - 文件夹名称 TextBlock: 14px SemiBold + TextPrimaryBrush + CharacterEllipsis + Wrap + MaxLines=2
    - 歌曲数量 StackPanel Orientation=Horizontal Spacing=2:
      - 数字 TextBlock: 绑定 SongCount (11px TextMutedBrush)
      - 单位 TextBlock: DynamicResource StringSongsUnit (11px TextMutedBrush)

  **空状态提示 Grid**:
  - IsVisible: Folders.Count 通过 CountToBoolConverter (ConverterParameter=invert) 控制
  - 居中 StackPanel Spacing=16:
    - 文件夹图标 "" (48px TextMutedBrush)
    - 提示文本 StringNoFolders (16px Medium TextSecondaryBrush)
    - 操作提示 StringAddMusicFolder (13px TextMutedBrush)

- **ViewModel 绑定**:
  - x:DataType="vm:FolderBrowseViewModel"
  - x:CompileBindings="False" (使用 DynamicResource 国际化)
  - 数据源: FolderBrowseViewModel (继承 ViewModelBase)
  - 关键属性: Folders, SelectedItem, FolderMenuOptions
  - 关键命令: NavigateToFolderSongsCommand, PlayCommand, AddToFavouriteCommand, RescanFolderCommand

- **转换器引用**:
  - CountToBoolConverter: 集合计数→布尔值（空状态显隐控制）
  - AlbumArtConverter: 封面路径→Image Source

- **设计规范遵循**:
  - ✅ 完全复用设计系统资源（BgCardBrush, BorderSubtleBrush, BgElement2Brush, TextPrimaryBrush 等）
  - ✅ 所有用户可见文本使用 DynamicResource 国际化字符串（无硬编码中文）
  - ✅ 使用 TapCommandBehavior 进行 MVVM 命令绑定（无代码隐藏逻辑）
  - ✅ 右键菜单直接绑定 ViewModel 命令（符合现有 HomeView 模式）
  - ✅ 响应式 WrapPanel 布局自适应窗口宽度
  - ✅ 卡片样式与 HomeView 网格视图保持一致（180px 宽度、12px 圆角、16px 间距）
  - ✅ 空状态提示与其他页面（HomeView、AlbumsPageView）风格统一
  - ✅ 参考原版 WinUI FolderBrowsePage 的 GridView 布局，改用 Avalonia ItemsControl+WrapPanel 实现

### 7. MusicBrowseView 集成为主窗口默认页面

- **日期**: 2026-04-14
- **目标**: 将 MusicBrowseView 设为应用主界面，替代原来的 HomeView
- **修改文件**:
  - `Services/Navigation/IViewModelFactory.cs`: 新增 `CreateMusicBrowseViewModel()` 接口方法
  - `Services/Navigation/ViewModelFactory.cs`: 实现工厂方法（ActivatorUtilities.CreateInstance）
  - `ViewModels/MainWindowViewModel.cs`:
    - 新增 `MusicBrowseViewModel` 属性（只读，构造函数初始化）
    - 新增 `_isPlayerBarVisible` 可观察属性 + `IsBottomPlayerBarVisible` 计算属性
    - 默认页面从 HomeViewModel 改为 MusicBrowseViewModel
    - NavigateToLibrary() 改为导航到 MusicBrowseViewModel
    - _pageLookup 字典新增 MusicBrowseViewModel 映射
    - OnCurrentPageChanged() 根据页面类型控制底部播放栏显隐
  - `Views/Main/MainWindow.axaml`: 底部播放栏 IsVisible 绑定改为 `{Binding IsBottomPlayerBarVisible}`
  - `Converters/PlayModeToIconConverter.cs`: 新增（PlaybackMode → Segoe Fluent Icons 图标）
  - `Converters/VolumeToIconConverter.cs`: 新增（音量值 → 音量等级图标）
  - `Converters/FavouriteIconConverter.cs`: 新增（收藏状态 → 心形图标）
  - `Converters/PlayStatusToIconConverter.cs`: 新增（播放状态 → 播放/暂停图标）
  - `Views/Library/MusicBrowseView.axaml`: 修复 ProgressRing→Border、WatermarkContent→Watermark

- **核心设计决策**:
  - MusicBrowseView 自带完整播放控制栏（120px 高），与 MainWindow 原有底部播放栏（72px）功能重叠
  - 采用**条件显隐方案**: 当 CurrentPage 为 MusicBrowseViewModel 时隐藏 MainWindow 底部栏，显示其他页面时恢复
  - 通过 `IsPlayerBarVisible` 属性 + `IsBottomPlayerBarVisible` 计算属性实现（结合 !IsMiniMode 条件）
  - 最小改动原则：不修改 MusicBrowseView 内部结构，仅调整外层容器的可见性绑定

- **导航流程变化**:
  - 应用启动 → 默认显示 MusicBrowseView（标签页: 歌曲）
  - 点击侧边栏"所有歌曲"图标 → NavigateToLibrary() → MusicBrowseViewModel
  - 切换到设置/统计等其他页面 → 自动恢复 MainWindow 底部播放栏
  - 返回音乐浏览 → 再次隐藏底部播放栏

### 8. 高优先级页面复刻 — 代码质量审查与修复

- **日期**: 2026-04-14
- **审查范围**: 新增的 3 个页面（MusicBrowseView、FolderBrowseView、SongListView）及相关 ViewModel 和 Converter

#### 审查结果汇总

| 检查项 | 状态 | 说明 |
|--------|------|------|
| 命名规范 (PascalCase/_camelCase) | ✅ 通过 | 所有类、方法、属性、字段符合规范 |
| 异步编程规范 | ⚠️ 已修复 | 1 个同步阻塞调用问题 |
| MVVM 模式合规性 | ✅ 通过 | 正确使用 [ObservableProperty]/[RelayCommand] |
| 国际化合规性 | ✅ 通过 | 无硬编码中文，全部使用 DynamicResource |
| 资源释放/内存泄漏 | ⚠️ 已修复 | 2 个事件管理问题 |

#### 发现并修复的问题

**问题 1 (严重) — MusicBrowseViewModel 同步阻塞异步调用**
- **位置**: [ViewModels/MusicBrowseViewModel.cs#L52](ViewModels/MusicBrowseViewModel.cs#L52)
- **原代码**: `OnSelectionChangedAsync().ConfigureAwait(false).GetAwaiter().GetResult()`
- **问题**: 在 partial void 同步方法中同步阻塞等待异步方法，违反"不要在同步方法中调用异步方法"规范，可能导致 UI 线程死锁
- **修复**: 改为 fire-and-forget 模式 `_ = OnSelectionChangedAsync()`
- **原因分析**: OnSelectionChangedAsync 内部主要是同步导航操作，末尾 `await Task.CompletedTask` 无实际异步工作

**问题 2 (严重) — SongListView.axaml.cs 事件内存泄漏**
- **位置**: [Views/Library/SongListView.axaml.cs#L20,L30](Views/Library/SongListView.axaml.cs#L20)
- **原代码**: InitializeEvents() 和 OnDataContextChanged() 中重复订阅 ScrollToCurrentSongRequested 且从未取消订阅
- **问题**: 每次 DataContext 变更都会添加新的事件处理器而不移除旧的，导致事件处理器链不断增长
- **修复**: 
  - 新增 `_viewModel` 字段保存当前 ViewModel 引用
  - 提取 `SubscribeToViewModel()` 统一管理订阅
  - 订阅前先取消旧订阅 (`-=`)，再订阅新实例 (`+=`)

**问题 3 (中) — FolderBrowseViewModel 手动事件管理不一致**
- **位置**: [ViewModels/FolderBrowseViewModel.cs#L65](ViewModels/FolderBrowseViewModel.cs#L65)
- **原代码**: 手动 `_userPlaylistService.PlaylistsChanged +=` 和 DisposeCore 中手动 `-=` 
- **问题**: 与项目中 SubscribeEvent 统一管理模式不一致，增加遗漏风险
- **修复**: 改用 `SubscribeEvent()` 自动管理生命周期，移除 DisposeCore 中的手动取消代码

#### 审查通过的规范项

**命名规范详情**:
- ViewModels: `MusicBrowseViewModel`, `FolderBrowseViewModel`, `SongListViewModel` — PascalCase ✅
- Views: `MusicBrowseView`, `FolderBrowseView`, `SongListView` — PascalCase ✅
- Converters: `PlayModeToIconConverter`, `VolumeToIconConverter`, `FavouriteIconConverter`, `PlayStatusToIconConverter` — PascalCase ✅
- 私有字段: `_musicLibraryService`, `_playlistService`, `_playbackStateService` 等 — _camelCase ✅
- 常量: `FavouriteIcon`, `NotFavouriteIcon` (FavouriteIconConverter) — PascalCase ✅
- XAML 控件: `SongList` (x:Name) — PascalCase ✅

**MVVM 模式合规性详情**:
- 所有 View 的 DataContext 通过 x:DataType 绑定到对应 ViewModel ✅
- 使用 CommunityToolkit.Mvvm 的 [ObservableProperty] 特性声明可观察属性 ✅
- 使用 [RelayCommand] 特性声明命令 ✅
- Code-behind 文件仅包含 InitializeComponent() 和必要的事件转发逻辑 ✅
- XAML 中无内联业务逻辑或代码隐藏 ✅

**国际化合规性详情**:
- MusicBrowseView.axaml: 标签文本使用 StringTabSong/StringTabAlbum 等 DynamicResource ✅
- FolderBrowseView.axaml: 菜单使用 StringPlay/StringAddToFavourite/StringMenuRescan ✅
- SongListView.axaml: 表头使用 StringTitle/StringArtist/StringAlbum/StringDuration ✅
- 所有空状态提示使用 StringNoFolders/StringNoSongs/StringAddMusicFolder ✅
- 零硬编码中文字符串 ✅

#### 功能完整度统计

基于 original-sound-hq-player-win2d-navi 项目（WinUI3）的 29 个核心页面：

| 时间节点 | 完成页面数 | 完成率 |
|----------|-----------|--------|
| 2026-04-14 前 | 17/29 | 58.7% |
| 2026-04-14 (本次) | 20/29 | ~69% |

**本次新增 3 个页面**:
1. MusicBrowseView — 音乐浏览主页面（6标签 + 播放控制栏）
2. FolderBrowseView — 文件夹网格浏览页面
3. SongListView — 歌曲列表表格页面（8列 + 右键菜单）

**配套新增**:
- 4 个 Converter（PlayMode/Volume/Favourite/PlayStatus 图标转换）
- MusicBrowseViewModel 集成为 MainWindow 默认页面
- DI 容器注册和 ViewLocator 映射更新
- Strings.axaml 新增 36 个国际化字符串
