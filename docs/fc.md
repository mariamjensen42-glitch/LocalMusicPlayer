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
