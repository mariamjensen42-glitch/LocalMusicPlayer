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
