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
