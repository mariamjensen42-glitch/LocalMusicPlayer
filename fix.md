# Bug 修复记录

## 2026-04-10: 首页内容完全空白

**问题**: 首页（PlayerView）完全空白，不显示任何内容（标题、歌曲列表、播放控制栏均不可见）

**根因**: `PlayerView.axaml` 引用了 `{StaticResource CountToBoolConverter}`，但该转换器未在 `App.axaml` 全局资源中注册，导致 Avalonia XAML 解析失败，整个 PlayerView 无法渲染。

**修复**:
1. **[App.axaml](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/App.axaml)**: 在全局资源中注册 `CountToBoolConverter`
2. **[MainWindowViewModel.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/ViewModels/MainWindowViewModel.cs#L425)**: 在 `Library.Songs.CollectionChanged` 事件中添加 `FilterSongs()` 调用，确保扫描完成后歌曲列表正确填充

## 2026-04-10: 系统托盘图标不可见

**问题**: 关闭窗口后系统托盘看不到应用图标，无法恢复窗口

**根因**: `SystemTrayService` 只创建了 `WindowIcon`（图标数据），但没有创建 `TrayIcon` 控件（实际的系统托盘图标），所以托盘区域没有显示任何东西

**修复**:
1. **[SystemTrayService.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/Services/SystemTrayService.cs)**: 创建 `TrayIcon` 控件并设置 Icon、ToolTipText、Menu 属性
2. 添加 `NativeMenuItem` 右键菜单（显示主窗口、退出）
3. 添加 `TrayIcon.Clicked` 事件，单击托盘图标恢复窗口
4. 修复退出逻辑，确保点"退出"时能正常关闭应用

## 2026-04-11: PlayHistoryService 重写为 EF Core SQLite 存储

**问题**: PlayHistoryService 使用内存+JSON 存储播放历史，需要迁移到 EF Core SQLite

**修复**:
1. **[PlayHistoryService.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/Services/PlayHistoryService.cs)**: 重写为使用 AppDbContext 操作 PlayHistoryEntity 表
2. 构造函数注入 AppDbContext 和 IMusicLibraryService，移除 IConfigurationService 依赖
3. AddToHistory 使用 Task.Run 包装数据库写入，立即 SaveChanges，超过 200 条时删除最旧记录
4. GetHistory 从数据库查询并按 PlayedAt 降序，通过 FilePath 匹配歌曲库中的 Song 对象，找不到则跳过
5. ClearHistory 使用 Task.Run 包装数据库清空操作

## 2026-04-11: ViewModel 层代码审查问题全面修复

**问题**: ViewModel 层存在17项代码审查问题，包括 Bug、架构违规、内存泄漏、性能问题等

**修复**:

### C8: BatchMetadataEditorViewModel YearField Bug
- [BatchMetadataEditorViewModel.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/ViewModels/BatchMetadataEditorViewModel.cs): YearField 映射从 TrackNumber 改为 Year；catch 块移除未使用的 ex 变量

### C3: PlaylistManagementViewModel 直接 new View
- [IDialogService.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/Services/IDialogService.cs): 添加 ShowMetadataEditorDialogAsync 和 ShowBatchMetadataEditorDialogAsync 方法
- [DialogService.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/Services/DialogService.cs): 实现对话框方法，添加 ShowFolderPickerAsync
- [PlaylistManagementViewModel.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/ViewModels/PlaylistManagementViewModel.cs): 移除 Views 引用，改为通过 IDialogService 打开对话框

### C4: ViewModel 引用 Avalonia UI 类型
- [PlayerPageViewModel.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/ViewModels/PlayerPageViewModel.cs): GradientBackground 从 IBrush 改为 string(hex)，移除 Avalonia UI 引用
- [MetadataEditorViewModel.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/ViewModels/MetadataEditorViewModel.cs): AlbumArtBitmap 改为 AlbumArtPath(string)，移除 Avalonia.Media.Imaging
- [StringToBrushConverter.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/Converters/StringToBrushConverter.cs): 新增 Converter 将颜色字符串转为 Brush
- [PlayerPageView.axaml](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/Views/PlayerPageView.axaml): 使用 StringToBrushConverter 绑定

### C5: 全局事件订阅泄漏
- [ViewModelBase.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/ViewModels/ViewModelBase.cs): 添加 IDisposable、AddDisposable、SubscribeEvent、DisposeCore
- 所有 ViewModel 改为方法引用订阅事件，在 DisposeCore 中取消订阅

### I1: DispatcherTimer 改为事件驱动
- [IPlaybackStateService.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/Services/IPlaybackStateService.cs): 添加 PositionChanged 事件
- PlayerPageViewModel 和 MainWindowViewModel 订阅 PositionChanged 替代 DispatcherTimer

### I2: 直接 new ViewModel 改为 IViewModelFactory
- [IViewModelFactory.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/Services/IViewModelFactory.cs): 添加 CreateArtistDetailViewModel 和 CreateAlbumDetailViewModel
- [ViewModelFactory.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/Services/ViewModelFactory.cs): 实现新方法
- MainWindowViewModel 改为通过 _viewModelFactory 创建

### I3: Action 委托改为 INavigationService
- [INavigationService.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/Services/INavigationService.cs): 添加 NavigateBack 方法
- 各 ViewModel 中 Action 委托改为调用 INavigationService

### I4: 手动 INPC 改为 [ObservableProperty]
- PlaylistManagementViewModel 和 LibraryCategoryViewModel 的手动 INPC 改为 [ObservableProperty] + partial OnChanged

### I5: SettingsViewModel 引用 Avalonia.Storage
- IDialogService 添加 ShowFolderPickerAsync，SettingsViewModel 改为调用 _dialogService

### I6: async void / fire-and-forget 修复
- InitializeAsync 改为 async Task，调用处用 ContinueWith 处理异常
- 所有 _ = SaveSettingsAsync() 添加 ContinueWith 错误处理
- LoadDataAsync 等改为 ContinueWith 模式

### I8: 频繁创建新 ObservableCollection
- LibraryBrowserViewModel 所有方法改为 Clear + foreach Add 模式

### I9: QueueViewModel PlaySong O(n)
- [IPlaylistService.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/Services/IPlaylistService.cs): 添加 PlayAtIndex(int index)
- [PlaylistService.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/Services/PlaylistService.cs): 实现 PlayAtIndex
- QueueViewModel 改为调用 _playlistService.PlayAtIndex(index)

### I10: 导航状态6布尔值改枚举
- MainWindowViewModel 添加 NavigationPage 枚举，6个布尔属性改为 CurrentNavPage
- [NavigationPageToColorConverter.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/Converters/NavigationPageToColorConverter.cs): 新增 Converter
- [MainWindow.axaml](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/Views/MainWindow.axaml): 侧边栏绑定改为 CurrentNavPage + Converter

### I10 Bug: StatisticsReportViewModel FormattedTotalDuration 赋值错误
- TotalDuration 使用 report.Overview.TotalPlayTime，FormattedTotalDuration 使用 TotalDuration 格式化

### Minor: BrowserCategory 枚举移至 Models
- [BrowserCategory.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/Models/BrowserCategory.cs): 枚举移至 Models 命名空间
- LibraryBrowserView.axaml 引用改为 models:BrowserCategory

### Minor: AlbumDetailViewModel/ArtistDetailViewModel 提取基类
- [DetailViewModelBase.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/ViewModels/DetailViewModelBase.cs): 提取公共逻辑到基类
- 子类仅定义 DetailName、CoverArtPath 属性

### Minor: 硬编码字符串
- DetailViewModelBase 中播放列表名使用 DetailName 属性动态生成

## 2026-04-11: Services/Models/Converters/Behaviors 层代码审查问题修复

### C2: AppDbContext Singleton 线程安全
- [AppDbContext.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/Data/AppDbContext.cs): 添加 _syncLock 字段和 SyncLock 属性
- [DatabaseService.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/Services/DatabaseService.cs): 所有数据库操作使用 lock(_dbContext.SyncLock) 包裹

### C6: LibVLCSharp 资源释放
- [App.axaml.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/App.axaml.cs): MainWindow.Closing 事件中释放 MusicPlayerService

### C7: 同步阻塞异步操作
- [IUserPlaylistService.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/Services/IUserPlaylistService.cs): 所有同步方法改为异步（CreatePlaylistAsync、DeletePlaylistAsync 等）
- [UserPlaylistService.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/Services/UserPlaylistService.cs): 移除所有 .Wait() 和 .GetAwaiter().GetResult()，改为 async/await
- [ILibraryCategoryService.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/Services/ILibraryCategoryService.cs): GetFavoriteSongs 改为 GetFavoriteSongsAsync
- [LibraryCategoryService.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/Services/LibraryCategoryService.cs): 实现异步方法
- [PlaylistManagementViewModel.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/ViewModels/PlaylistManagementViewModel.cs): 调用方改为 await
- [MainWindowViewModel.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/ViewModels/MainWindowViewModel.cs): ToggleFavorite 改为异步
- [SettingsViewModel.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/ViewModels/SettingsViewModel.cs): ShowFolderPickerAsync 返回值改为 IReadOnlyList

### I7: AlbumArtConverter 无缓存
- [AlbumArtConverter.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/Converters/AlbumArtConverter.cs): 添加 ConcurrentDictionary + WeakReference 缓存、具体异常捕获、Instance 单例

### I15: Converter 问题修复
- [EnumToBoolConverter.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/Converters/EnumToBoolConverter.cs): ConvertBack 使用 Enum.Parse 返回枚举值；添加 Instance 单例
- [EnumBooleanConverter.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/Converters/EnumBooleanConverter.cs): 继承 EnumToBoolConverter，功能合并
- [BoolToWidthConverter.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/Converters/BoolToWidthConverter.cs): double.Parse 改为 double.TryParse
- [CountToBoolConverter.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/Converters/CountToBoolConverter.cs): int.Parse 改为 int.TryParse；ConvertBack 统一 NotSupportedException
- [SongIsPlayingConverter.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/Converters/SongIsPlayingConverter.cs): 颜色值提取为常量
- [BoolToColorConverter.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/Converters/BoolToColorConverter.cs): 缓存 Brush；颜色提取为常量；ConvertBack 统一 NotSupportedException
- [BoolToFontWeightConverter.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/Converters/BoolToFontWeightConverter.cs): 显式处理 null；ConvertBack 统一 NotSupportedException
- [BoolToLyricColorConverter.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/Converters/BoolToLyricColorConverter.cs): 显式处理 null；颜色提取为常量；ConvertBack 统一 NotSupportedException
- [BoolToOpacityConverter.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/Converters/BoolToOpacityConverter.cs): 支持 parameter 自定义透明度值
- [LyricTranslationFontSizeConverter.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/Converters/LyricTranslationFontSizeConverter.cs): 魔法数字 0.57 提取为 TranslationScaleFactor 常量
- [QueueIndexConverter.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/Converters/QueueIndexConverter.cs): 添加 Instance 单例
- [StringNotEmptyConverter.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/Converters/StringNotEmptyConverter.cs): ConvertBack 统一 NotSupportedException
- [IndexConverter.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/Converters/IndexConverter.cs): ConvertBack 统一 NotSupportedException

### I11: DragDropSortBehavior 依赖具体 VM
- [DragDropSortBehavior.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/Behaviors/DragDropSortBehavior.cs): 添加 MoveCommand StyledProperty，移除对 QueueViewModel/PlaylistManagementViewModel 的直接依赖

### I16: ClickBehavior 重命名
- [ClickAttachedProperty.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/Behaviors/ClickAttachedProperty.cs): 重命名为 ClickAttachedProperty

### I17: EF Core 缺少索引
- [AppDbContext.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/Data/AppDbContext.cs): SongEntity 添加 Artist、Album、AlbumArtPath 索引；PlayHistoryEntity 添加 FilePath 索引

### I9: IPlaylistService 添加 PlayAtIndex
- [IPlaylistService.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/Services/IPlaylistService.cs): 添加 PlayAtIndex(int index)
- [PlaylistService.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/Services/PlaylistService.cs): 实现 PlayAtIndex

### I5: IDialogService 添加方法
- [IDialogService.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/Services/IDialogService.cs): 添加 ShowMetadataEditorDialogAsync、ShowBatchMetadataEditorDialogAsync、ShowFolderPickerAsync
- [DialogService.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/Services/DialogService.cs): ShowFolderPickerAsync 返回 IReadOnlyList<string>?

### I3: INavigationService 添加 NavigateBack
- 已在之前的修复中完成

### Minor: MetadataEditorViewModel ByteVector Bug
- [MetadataEditorViewModel.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/ViewModels/MetadataEditorViewModel.cs): new ByteVector(imagePath) 改为 new ByteVector(File.ReadAllBytes(imagePath))

## 2026-04-11: 上一次播放和队列功能修复

**问题**: 应用重启后无法恢复上次的播放状态和队列

**根因**: 缺少播放状态持久化机制

**修复**:
1. **[AppSettings.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/Models/AppSettings.cs)**: 新增 `LastSongFilePath`、`QueueFilePaths`、`LastPlaybackPosition` 属性
2. **[IConfigurationService.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/Services/IConfigurationService.cs)**: 新增 `SavePlaybackStateAsync` 方法
3. **[ConfigurationService.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/Services/ConfigurationService.cs)**: 实现 `SavePlaybackStateAsync`，LoadSettingsAsync/SaveSettingsAsync 中添加新属性的序列化
4. **[App.axaml.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/App.axaml.cs)**: Closing 事件中保存当前播放队列和位置
5. **[MainWindowViewModel.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/ViewModels/MainWindowViewModel.cs)**: InitializeAsync 结束时调用 `RestorePlaybackStateAsync` 恢复播放状态
