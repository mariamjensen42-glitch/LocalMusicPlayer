# 功能记录

> 本文档记录 LocalMusicPlayer 当前已有的功能。仅保留当前实际存在的功能。

## 一、播放核心

### 1. 音频播放
- 基于 LibVLCSharp 的音乐播放引擎
- 支持播放/暂停/停止/上一曲/下一曲
- 快进/快退（±5秒）
- 播放进度拖拽定位（SliderClickToSeekBehavior）
- 播放状态持久化（关闭时保存队列和位置，启动时恢复）

### 2. 播放模式
- PlaybackMode 枚举：Normal / Loop / SingleLoop
- Repeat 按钮三态循环：Normal → Loop → SingleLoop → Normal
- 单曲循环时图标显示 "1" 标记
- PlaylistService.PlayNext() 支持 SingleLoop 分支

### 3. 播放速度控制
- IMusicPlayerService 新增 SetPlaybackRate / PlaybackRate
- 速度范围 0.5x ~ 2.0x
- PlaybackRate 持久化到 AppSettings

### 4. 淡入淡出
- IMusicPlayerService 新增 FadeInAsync / FadeOutAsync
- 20 步渐变实现音量过渡
- IPlaybackStateService 新增 IsCrossfadeEnabled / CrossfadeDuration
- 设置页新增淡入淡出开关和时长滑块（1-10 秒）
- CrossfadeEnabled / CrossfadeDurationMs 持久化到 AppSettings

### 5. 音量归一化 (ReplayGain)
- Song 模型新增 ReplayGainTrackGain / FileSizeBytes 属性
- IMusicPlayerService 新增 SetReplayGainEnabled / ApplyReplayGain
- 播放时根据 ReplayGainTrackGain 调整音量
- ReplayGainEnabled 持久化到 AppSettings

### 6. 播放进度
- IPlaybackProgress 接口（Position, Duration, PositionSeconds, DurationSeconds）
- MainWindowViewModel 实现此接口
- MusicProgressBar 自定义控件：点击轨道跳转、250ms 定时刷新 UI、紫粉渐变填充

## 二、歌词系统

### 1. 歌词显示
- LyricsLineControl 自定义控件
  - 双层文本结构：底层半透明（Opacity=0.5）+ 顶层高亮
  - 逐字高亮动画：ClipToBounds + Width 动画实现裁剪效果
  - 翻译文本显示/隐藏
  - 依赖属性：LyricsText, TranslateText, IsTranslationVisible, LyricsFontSize, TranslateFontSize, IsCurrentLine, LineAnimateDuration, IsWFWLyrics, ActiveForeground, InactiveForeground
- LyricsAutoScrollBehavior 自动跟随滚动

### 2. 歌词显示设置
- 歌词字体大小调节（14~48px）
- 歌词行距调节（LyricLineSpacing）
- 翻译歌词显隐切换
- 翻译字号自动按原文字号 0.57 比例缩放（LyricTranslationFontSizeConverter）
- LyricFontSize / LyricLineSpacing / ShowTranslation 持久化到 AppSettings

### 3. 歌词搜索结果弹窗
- IDialogService.ShowLyricSearchResultDialogAsync
- 弹窗显示歌词预览（前15行）+ 来源信息
- 用户确认后更新歌词

### 4. 歌词相关转换器
- BoolToLyricColorConverter：布尔→歌词颜色（紫色/灰色）
- BoolToLyricFontWeightConverter：布尔→歌词字重（Bold/Normal）
- TranslationVisibilityConverter：多值翻译歌词可见性
- LyricTranslationFontSizeConverter：翻译字号缩放

## 三、音乐库

### 1. 音乐库服务 (IMusicLibraryService / MusicLibraryService)
- Songs 集合（ObservableCollection<Song>）
- FilteredSongs 过滤后的歌曲集合
- 歌曲增删操作

### 2. 文件扫描 (IFileScannerService / FileScannerService)
- 使用 TagLib 读取元数据
- 支持 16 种音频格式（含 DSD/无损）
- 带进度报告和取消令牌

### 3. 扫描编排 (IScanService / ScanService)
- 整合 FileScanner + FileWatcher
- 支持扫描/重新扫描/监视文件夹
- 文件变更时自动增删歌曲

### 4. 文件监听 (IFileWatcherService / FileWatcherService)
- 基于 FileSystemWatcher
- 自动检测音乐文件增删改
- UI 线程回调

### 5. 音乐库分类 (ILibraryCategoryService / LibraryCategoryService)
- 按艺术家/专辑/文件夹分组
- 收藏歌曲查询
- 歌曲库变更时自动刷新

### 6. 音乐库视图 (MusicLibraryViewModel / MusicLibraryView)
- TabControl 4 标签页：所有歌曲/专辑/艺术家/文件夹
- 搜索过滤
- 播放/随机/收藏命令

## 四、封面与视觉

### 1. 封面颜色提取 (IAlbumArtService / AlbumArtService)
- ExtractDominantColorAsync 缩小图片采样像素计算主色调
- AlbumCover 模型（Album, Artist, CoverPath, CachedAt, FileSizeBytes, Width, Height, MimeType, CacheKey）

### 2. AlbumArtControl 自定义控件
- 交叉淡入淡出动画：切换封面时旧图淡出新图淡入（400ms CubicEaseOut）
- 圆角遮罩 + 投影阴影（DropShadowEffect）
- 图片去重机制（ComputeFastHash 采样哈希）
- 依赖属性：ImageBytes, IsDark, CornerRadiusValue, IsShadowEnabled, IsActive
- 双 Image 控件交替显示实现平滑过渡

### 3. GradientBackgroundControl 自定义控件
- 流体渐变背景：4 个 Ellipse + BlurEffect + 位置动画模拟流体效果
- 从封面图提取 4 个主色调（自研颜色提取算法）
- 颜色平滑过渡动画（800ms CubicEaseOut）
- 亮/暗主题自适应
- 依赖属性：IsBackgroundEnable, ImageBytes, IsDark, UseImageDominantTheme
- 事件：ThemeResolved

## 五、播放列表

### 1. 播放列表服务 (IPlaylistService / PlaylistService)
- 播放队列管理
- PlayNext() 支持 Normal/Loop/SingleLoop 分支

### 2. 用户歌单 (IUserPlaylistService / UserPlaylistService)
- EF Core SQLite 存储（PlaylistEntity / PlaylistSongEntity 表）
- 收藏夹（FavoriteEntity 表）
- 播放列表 CRUD 操作
- 歌曲库变更时自动同步收藏状态
- ExportPlaylistAsync/ImportPlaylistAsync JSON 文件导入导出

### 3. 歌单列表页 (PlaylistListViewModel / PlaylistListView)
- WrapPanel 网格布局，每个歌单卡片显示图标、名称、歌曲数量
- 点击卡片导航到歌单详情
- 顶部「+ 新建播放列表」按钮

### 4. 歌单管理页 (PlaylistManagementViewModel / PlaylistManagementView)
- 歌单内歌曲列表
- 拖拽排序（DragDropSortBehavior）
- 右键菜单：播放/收藏/添加歌单/队列/属性/批量编辑

### 5. 播放队列面板 (QueueViewModel / QueueView)
- 播放队列歌曲列表
- 播放/移除/清空
- 双击播放
- 拖拽排序

## 六、播放历史

### 1. 播放历史服务 (IPlayHistoryService / PlayHistoryService)
- 最多 200 条记录
- 持久化到 AppSettings（PlayHistoryRecord: FilePath + PlayedAt）
- 启动时自动从配置加载，根据文件路径匹配音乐库歌曲
- 播放历史变更时自动保存

## 七、详情页

### 1. 专辑详情 (AlbumDetailViewModel / AlbumDetailView)
- 信息头：150×150 封面 + 专辑名 + 艺术家 + 年份·歌曲数 + 操作按钮
- 歌曲列表：封面 + 收藏 + 标题 + 艺术家 + 时长
- 双击播放 + 右键菜单
- ToggleFavoriteCommand

### 2. 艺术家详情 (ArtistDetailViewModel / ArtistDetailView)
- 复用 DetailHeaderView
- 歌曲列表 + 右键菜单
- 继承 DetailViewModelBase（含 AddToPlaylistCommand）

### 3. DetailViewModelBase 基类
- 公共属性：Songs, SongCount, TotalDuration
- 公共命令：PlayAllCommand, ShuffleAllCommand, AddToPlaylistCommand, EditSongMetadataCommand, NavigateBackCommand, PlaySongCommand
- 注入 IDialogService

## 八、元数据编辑

### 1. 元数据编辑器 (MetadataEditorViewModel / MetadataEditorView)
- 单曲标签编辑
- 使用 TagLibSharp 读写

## 九、设置

### 1. 设置服务 (IConfigurationService / ConfigurationService)
- EF Core SQLite 存储（AppSettingsEntity Key-Value 表）
- JSON 迁移逻辑：首次启动检测旧 JSON 文件，导入 SQLite 后删除
- AppSettings 包含：扫描文件夹、播放速度、淡入淡出、ReplayGain、歌词设置、播放历史等

### 2. 设置页面 (SettingsViewModel / SettingsView)
- 扫描文件夹管理
- 播放速度滑块
- 淡入淡出开关和时长
- ReplayGain 开关
- 歌词设置

## 十、导航系统

### 1. 导航服务 (INavigationService / NavigationService)
- ViewModel 级别导航
- NavigationPage 枚举：None, MusicLibrary, Favorites, Settings, Playlist

### 2. ViewModel 工厂 (IViewModelFactory / ViewModelFactory)
- 注入 IServiceProvider + ActivatorUtilities.CreateInstance
- 工厂方法：CreatePlayerPageViewModel, CreateSettingsViewModel, CreatePlaylistListViewModel, CreatePlaylistManagementViewModel, CreateMetadataEditorViewModel, CreateAlbumDetailViewModel, CreateArtistDetailViewModel, CreateQueueViewModel, CreateMusicLibraryViewModel

### 3. ViewLocator
- ViewModel → View 自动映射

### 4. 导航相关转换器
- NavPageVisibilityConverter：NavigationPage 不等比较（控制页面显隐）
- NavigationPageToBoolConverter：NavigationPage 相等比较
- NavigationPageToColorConverter：NavigationPage→颜色（选中紫色/未选灰色）

## 十一、对话框服务

### 1. IDialogService / DialogService
- ShowMessageDialogAsync：消息提示
- ShowInputDialogAsync：输入对话框
- ShowAddToPlaylistDialogAsync：添加到歌单弹窗
- ShowLyricSearchResultDialogAsync：歌词搜索结果弹窗
- ShowMetadataEditorDialogAsync：元数据编辑弹窗
- ShowBatchMetadataEditorDialogAsync：批量编辑弹窗
- ShowConfirmDialogAsync：确认对话框

## 十二、主窗口

### 1. MainWindow
- 48px 紧凑图标导航栏（参考 WinUI3 NavigationView LeftCompact 模式）
- 底部播放栏（72px）：进度条 + 播放控制 + 音量 + 播放模式
- 迷你模式（300×520 竖向歌词小窗）：同窗切换，ZIndex=300 覆盖层
- 拖放支持（DropHandlerService）
- 键盘快捷键（KeyboardShortcutService）

### 2. MainWindowViewModel
- 实现 IPlaybackProgress 接口
- 导航管理（CurrentPage, NavigationPage 枚举）
- 迷你模式切换（IsMiniMode）
- 底部播放栏条件显隐（IsBottomPlayerBarVisible）

## 十三、播放器页面

### 1. PlayerPageViewModel / PlayerPageView
- 集成 GradientBackgroundControl 流体渐变背景
- 集成 AlbumArtControl 交叉淡入淡出封面
- 集成 LyricsLineControl 逐字高亮歌词
- 歌词区 OpacityMask 上下渐变淡出效果
- ImageBytes 属性供控件使用
- IsBackgroundEnabled 控制渐变背景
- 响应式布局（封面 200-480px 自适应）

## 十四、数据模型

| 模型 | 说明 |
|------|------|
| Song | 歌曲模型（Title, Artist, Album, Genre, Year, Duration, FilePath, PlayCount, LastPlayedTime, AddedAt, ReplayGainTrackGain, FileSizeBytes 等） |
| AlbumGroup | 专辑分组（AlbumName, ArtistName, Songs, SongCount, TotalDuration, CoverArtPath） |
| AlbumGrouping | 分组容器（GroupKey + Albums 集合，按首字母 A-Z 分组） |
| ArtistGroup | 艺术家分组（ArtistName, Songs, SongCount, CoverArtPath） |
| FolderGroup | 文件夹分组（FolderPath, FolderName, DisplayName, Songs, SongCount, CoverArtPath） |
| AlbumCover | 封面缓存（Album, Artist, CoverPath, CachedAt, FileSizeBytes, Width, Height, MimeType, CacheKey） |
| UserPlaylist | 用户歌单（Name, Songs, IsFavorite） |
| Playlist | 基础播放列表（Name, Songs） |
| PlaybackMode | 播放模式枚举（Normal, Loop, SingleLoop） |
| PlayState | 播放状态枚举（Stopped, Playing, Paused） |
| PlayerStatus | 播放器状态（Position, Duration, IsPlaying, Volume, IsMuted, State, CurrentSong） |
| LibraryCategory | 库分类枚举（Songs, Artists, Albums, Folders, Favorites） |
| AppSettings | 应用设置（扫描文件夹、播放速度、淡入淡出、ReplayGain、歌词设置、播放历史等） |

## 十五、数据存储

### 1. EF Core SQLite (AppDbContext)
- AppSettingsEntity：Key-Value 设置表
- FavoriteEntity：收藏表
- PlaylistEntity / PlaylistSongEntity：播放列表表
- DatabaseService.InitializeAsync：数据库初始化和迁移

## 十六、依赖注入

### 1. ServiceCollectionExtensions
- 按功能分类注册：Navigation、Playback、Library、Media、Playlist、System、File
- AddMusicServices() 扩展方法统一注册

### 2. DI 注册的服务
- **Navigation**: INavigationService, IViewModelFactory, IDialogService, IWindowProvider, IDropHandlerService, IKeyboardShortcutService
- **Playback**: IMusicPlayerService, IPlaybackStateService
- **Library**: IMusicLibraryService, IFileScannerService, IScanService, ILibraryCategoryService
- **Media**: IAlbumArtService, ILyricsService
- **Playlist**: IPlaylistService, IUserPlaylistService, IPlayHistoryService
- **System**: IConfigurationService, DatabaseService, AppDbContext
- **File**: IFileWatcherService

## 十七、行为 (Behaviors)

| 行为 | 说明 |
|------|------|
| TapCommandBehavior | 双击/单击命令绑定 |
| LyricsAutoScrollBehavior | 歌词自动滚动到当前行 |
| SliderClickToSeekBehavior | 点击 Slider 轨道跳转 |
| WindowDragBehavior | 窗口拖拽 |
| DragDropSortBehavior | ListBox 拖拽排序（MoveCommand + ObservableCollection.Move） |
| ClickAttachedProperty | 点击次数附加属性 |

## 十八、转换器 (Converters)

| 转换器 | 说明 |
|------|------|
| AlbumArtConverter | 封面路径→ImageSource |
| IndexConverter | int 索引→1-based 字符串 |
| PlayCountConverter | 播放次数（0次显示"-"） |
| QueueIndexConverter | 多值队列索引（IList + item → 1-based 索引） |
| CountToBoolConverter | 集合计数→布尔值 |
| BoolToColorConverter | 布尔值→颜色/显隐 |
| BoolToFontWeightConverter | 布尔→字重 |
| BoolToOpacityConverter | 布尔→透明度 |
| SongIsPlayingConverter | 当前播放歌曲判断 |
| TimeSpanToStringConverter | 时间格式化 |
| BoolToFavouriteIconConverter | 布尔→收藏图标（实心心/空心心） |
| LyricTranslationFontSizeConverter | 翻译字号缩放 |
| StringNotEmptyConverter | 字符串非空→布尔 |
| StringToBrushConverter | 十六进制颜色字符串→SolidColorBrush |
| BoolToWidthConverter | 布尔→宽度（面板展开/收起） |
| BoolToLyricColorConverter | 布尔→歌词颜色（紫色/灰色） |
| BoolToLyricFontWeightConverter | 布尔→歌词字重 |
| TranslationVisibilityConverter | 多值翻译歌词可见性 |
| EnumBooleanConverter | 枚举→布尔（支持 ConvertBack） |
| MultiEnumToBoolConverter | 多值枚举相等比较 |
| PlayModeToIconConverter | 播放模式→Segoe Fluent Icons 图标 |
| PlayStatusToIconConverter | 播放状态→播放/暂停图标 |
| VolumeToIconConverter | 音量等级→音量图标 |
| FavouriteIconConverter | 收藏状态→心形图标 |
| NavPageVisibilityConverter | NavigationPage 不等比较 |
| NavigationPageToBoolConverter | NavigationPage 相等比较 |
| NavigationPageToColorConverter | NavigationPage→颜色 |

## 十九、样式系统 (Styles/Resources.axaml)

### 按钮样式
- CircularHoverButtonTheme：圆形悬停按钮
- NoHoverButtonTheme：无悬停效果按钮
- FavoriteButtonTheme：收藏按钮（悬停放大 + 按下缩小动画）
- TransparentIconButtonTheme：透明图标按钮
- PlayerControlButtonTheme：播放控制按钮（32×32 透明圆形）
- PlayButtonLargeTheme：大型播放按钮（48×48 渐变背景圆形）
- AnimatedMenuItemTheme：菜单项主题

### 滑块样式
- GhostSliderTheme：极简风格滑块
- MusicProgressSliderTheme：音乐进度滑块（紫粉渐变轨道）
- VolumeControlSliderTheme：音量控制滑块（紫色填充轨道）
- CompactProgressSliderTheme：紧凑进度滑块（8px 高轨道，隐藏 Thumb）

### 卡片/容器样式
- AlbumCardTheme：专辑卡片容器
- CardTheme / CardHoverTheme：通用卡片
- InputBorderTheme：输入框样式

### 列表样式
- SongListItemTheme：歌曲列表项
- SongListItemRowTheme / SongListActiveRowTheme：歌曲列表行/活跃行
- SongRowTheme：歌曲行
- PlayingSongRowTheme：当前播放歌曲行（紫色半透明背景 + 紫色边框）
- QueueListItemTheme：队列列表项（选中时 AccentDim 背景）

### 导航样式
- SidebarNavItemTheme：侧边栏导航项（ToggleButton，选中紫色）
- TabRadioButtonTheme：标签单选按钮
- MusicLibraryTabControlTheme / MusicLibraryTabItemTheme：音乐库标签控件

### 其他样式
- AlbumGroupHeaderTheme：分组标题（28px SemiBold）
- SongTableHeaderTheme：表头容器（圆角 12,12,0,0）
- DetailCoverTheme：详情页封面框（150×150）
- AlbumCoverCardTheme：专辑封面卡片
- ModernScrollBarTheme：现代滚动条（8px 宽，悬停 12px）
- PrimaryButtonTheme / SecondaryButtonTheme：主/次按钮

### 画刷资源
- PlayerBarBgBrush：播放栏背景色
- BgCardBrush / BgCardHoverBrush：卡片背景
- BgTableHeaderBrush：表头背景
- BgElement2Brush：元素背景
- TextPrimaryBrush / TextSecondaryBrush / TextMutedBrush：文本色
- BorderSubtleBrush：边框色
- AccentPurple / AccentDim：强调色
