# 项目简化计划

## 目标

简化项目功能，将多个浏览页面合并为单一"音乐库"视图，删除非核心功能。

## 需要保留的MVP功能

- ✅ 音乐扫描 + 元数据读取
- ✅ 播放控制（播放/暂停/进度/音量/播放模式）
- ✅ 用户播放列表
- ✅ 歌曲搜索
- ✅ 本地LRC歌词显示
- ✅ 专辑封面显示
- ✅ 收藏功能

---

## 阶段一：删除非核心功能模块

### 1.1 删除智能播放列表 (SmartPlaylist)

**删除文件：**
- `Services/SmartPlaylist/` (整个目录)
- `Models/SmartPlaylist.cs`
- `ViewModels/SmartPlaylistEditorViewModel.cs`
- `ViewModels/SmartPlaylistSongsViewModel.cs`
- `Views/SmartPlaylist/` (整个目录)
- `Views/Editors/SmartPlaylistEditorView.axaml` (如存在)

**删除引用：**
- `ViewModels/MainWindowViewModel.cs` - 移除 SmartPlaylist 相关命令和属性
- `Helpers/ServiceCollectionExtensions.cs` - 移除 SmartPlaylistService 注册
- `Views/Main/MainWindow.axaml` - 移除智能播放列表菜单项

### 1.2 删除在线歌词搜索 (OnlineLyrics)

**删除文件：**
- `Services/OnlineLyrics/` (整个目录)
- `Services/IOnlineLyricsService.cs`
- `Services/OnlineLyricsService.cs`
- `Helpers/ChineseHelper.cs` (如仅用于歌词搜索)
- `Helpers/StringHelper.cs` (如仅用于歌词搜索)

**删除引用：**
- `Helpers/ServiceCollectionExtensions.cs` - 移除 OnlineLyricsService 注册
- `ViewModels/PlayerPageViewModel.cs` - 移除"搜索歌词"命令

### 1.3 删除播放统计系统 (Statistics)

**删除文件：**
- `Services/Statistics/` (整个目录)
- `Models/StatisticsReport.cs`
- `Models/LibraryStatistics.cs`
- `Models/SongStatistics.cs`
- `Models/SongPlayRecord.cs`
- `Models/PlayHistoryEntry.cs`
- `Models/PlayHistoryRecord.cs`
- `ViewModels/StatisticsViewModel.cs`
- `ViewModels/StatisticsReportViewModel.cs`
- `ViewModels/PlayHistoryViewModel.cs`
- `Views/Statistics/` (整个目录)

**删除引用：**
- `Services/Playlist/PlayHistoryService.cs` - 简化或删除
- `Helpers/ServiceCollectionExtensions.cs` - 移除 StatisticsService 注册
- `Views/Main/MainWindow.axaml` - 移除统计菜单项

### 1.4 删除批量元数据编辑

**删除文件：**
- `ViewModels/BatchMetadataEditorViewModel.cs`
- `Views/Editors/BatchMetadataEditorView.axaml`
- `Views/Editors/BatchMetadataEditorView.axaml.cs`
- `Models/BatchEditField.cs`
- `Converters/Metadata/` (整个目录)

**删除引用：**
- `ViewModels/MainWindowViewModel.cs` - 移除批量编辑命令

### 1.5 删除系统托盘和自动启动

**删除文件：**
- `Services/System/AutoStartService.cs`
- `Services/System/ISystemTrayService.cs`
- `Services/System/SystemTrayService.cs`

**删除引用：**
- `Helpers/ServiceCollectionExtensions.cs` - 移除托盘服务注册
- `App.axaml.cs` - 移除托盘初始化

### 1.6 删除其他未使用的模型

**检查并删除：**
- `Models/AlbumCover.cs` - 如仅用于在线歌词封面
- `Models/AlbumGroup.cs` - 如有替代方案
- `Models/ArtistGroup.cs`
- `Models/FolderGroup.cs`
- `Models/FolderNode.cs`
- `Models/GenreInfo.cs`
- `Models/BrowserCategory.cs`
- `Models/LibraryCategory.cs`

---

## 阶段二：创建统一的音乐库视图

### 2.1 创建 MusicLibraryView (统一视图)

**新建文件：**
- `Views/Library/MusicLibraryView.axaml`
- `Views/Library/MusicLibraryView.axaml.cs`
- `ViewModels/MusicLibraryViewModel.cs`

**布局结构：**
```
MusicLibraryView
├── 顶部工具栏 (搜索框 + 视图切换Tab)
├── 内容区域 (TabControl)
│   ├── 全部歌曲 (SongsTab)
│   ├── 专辑 (AlbumsTab)
│   ├── 艺术家 (ArtistsTab)
│   └── 分类 (FoldersTab)
└── 底部播放栏 (保留现有PlayerBar)
```

### 2.2 创建 MusicLibraryViewModel

**核心属性：**
- `Songs` - 所有歌曲列表
- `Albums` - 专辑分组
- `Artists` - 艺术家分组
- `Folders` - 文件夹分组
- `SearchQuery` - 搜索关键字
- `FilteredSongs` - 过滤后的歌曲
- `SelectedTabIndex` - 当前Tab索引

**核心命令：**
- `SearchCommand` - 搜索歌曲
- `PlaySongCommand` - 播放歌曲
- `AddToPlaylistCommand` - 添加到播放列表
- `ToggleFavoriteCommand` - 切换收藏

### 2.3 迁移现有功能

**从 HomeViewModel 迁移：**
- 歌曲列表显示
- 搜索功能
- 快捷播放

**从 AlbumsPageViewModel 迁移：**
- 专辑分组逻辑

**从 ArtistsPageViewModel 迁移：**
- 艺术家分组逻辑

**从 LibraryBrowserViewModel / LibraryCategoryViewModel 迁移：**
- 文件夹浏览逻辑

---

## 阶段三：更新主窗口导航

### 3.1 修改 MainWindow.axaml

**移除的导航项：**
- 首页 (Home)
- 专辑页 (Albums)
- 艺术家页 (Artists)
- 音乐库浏览器 (Library Browser)
- 音乐库分类 (Library Category)
- 智能播放列表
- 统计
- 批量编辑

**保留的导航项：**
- 音乐库 (MusicLibrary) - 新增
- 播放列表 (Playlists)
- 播放器 (Player) - 详情页
- 设置 (Settings)

### 3.2 修改 MainWindowViewModel

**更新导航：**
- 移除旧的页面导航属性
- 添加 MusicLibrary 导航命令
- 简化页面切换逻辑

---

## 阶段四：数据模型清理

### 4.1 保留的核心模型

- `Song.cs` - 歌曲
- `Playlist.cs` - 播放列表
- `UserPlaylist.cs` - 用户播放列表
- `AlbumInfo.cs` - 专辑信息
- `ArtistInfo.cs` - 艺术家信息
- `AppSettings.cs` - 应用设置
- `PlaybackMode.cs` - 播放模式
- `PlayState.cs` - 播放状态
- `PlayerStatus.cs` - 播放器状态
- `FileOperationProgress.cs` - 文件操作进度

### 4.2 简化 AlbumGroup 和 ArtistGroup

如果 AlbumInfo/ArtistInfo 已包含所需信息，可删除这些分组模型。

---

## 阶段五：服务层清理

### 5.1 保留的核心服务

- `IMusicPlayerService` / `MusicPlayerService`
- `IPlaylistService` / `PlaylistService`
- `IUserPlaylistService` / `UserPlaylistService`
- `IPlayHistoryService` / `PlayHistoryService` (简化)
- `IMusicLibraryService` / `MusicLibraryService`
- `IScanService` / `ScanService`
- `ILyricsService` / `LyricsService`
- `IAlbumArtService` / `AlbumArtService`
- `IConfigurationService` / `ConfigurationService`
- `IFileScannerService` / `FileScannerService`
- `INavigationService` / `NavigationService`
- `IViewModelFactory` / `ViewModelFactory`
- `IWindowProvider` / `WindowProvider`
- `IDialogService` / `DialogService`
- `IDropHandlerService` / `DropHandlerService`
- `IKeyboardShortcutService` / `KeyboardShortcutService`
- `IDedupService` / `DedupService`
- `ICoverManagerService` / `CoverManagerService`
- `IFileManagerService` / `FileManagerService`
- `IFileWatcherService` / `FileWatcherService`
- `ILibraryCategoryService` / `LibraryCategoryService`
- `DatabaseService`

### 5.2 删除的服务

- `SmartPlaylistService`
- `OnlineLyricsService`
- `StatisticsService`
- `AutoStartService`
- `SystemTrayService`

---

## 阶段六：数据库迁移

### 6.1 创建删除迁移

```bash
dotnet ef migrations add RemoveDeprecatedFeatures
```

**删除的表：**
- SmartPlaylists
- Statistics
- PlayHistoryRecords (可选保留简化的历史)

---

## 阶段七：清理 XAML 资源和转换器

### 7.1 检查并删除未使用的转换器

**可能保留：**
- `TimeSpanToStringConverter`
- `PlayStatusToIconConverter`
- `PlayModeToIconConverter`
- `VolumeToIconConverter`
- `BoolToOpacityConverter`
- `BoolToFontWeightConverter`
- `IndexConverter`
- `AlbumArtConverter`

**删除未使用的转换器**

### 7.2 清理 Controls

**保留：**
- `AlbumArtControl`
- `GradientBackgroundControl`
- `LyricsLineControl`

**检查并清理未使用的 Controls**

---

## 实施顺序

1. **阶段一** - 删除非核心模块（最安全，先做）
2. **阶段六** - 数据库迁移
3. **阶段五** - 服务层清理
4. **阶段四** - 数据模型清理
5. **阶段七** - 资源和转换器清理
6. **阶段二** - 创建统一音乐库视图
7. **阶段三** - 更新主窗口导航
8. **编译测试** - 确保无编译错误
9. **功能测试** - 验证核心功能正常

---

## 预计删除的代码量

- ViewModels: ~5 个
- Views: ~15 个
- Services: ~5 个
- Models: ~10 个
- 总计: 约 35+ 个文件

---

## 风险评估

| 风险 | 级别 | 缓解措施 |
|------|------|----------|
| 删除依赖导致编译错误 | 中 | 按阶段执行，每阶段编译测试 |
| 导航逻辑耦合紧密 | 中 | 逐步解耦，保持主窗口稳定 |
| 数据库迁移丢失数据 | 低 | 先备份数据库 |
| UI测试失败 | 中 | 更新对应的单元测试 |
