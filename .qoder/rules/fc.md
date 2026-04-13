# Feature Changes - LocalMusicPlayer

## 2026-04-13 - 代码和目录结构优化

### 概述
根据 Avalonia 最佳实践对项目进行了全面优化，包括 XAML 规范、MVVM 模式和代码质量改进。

### 主要更改

#### 1. XAML 硬编码字符串替换
- **更新的文件**:
  - `Views/Shared/DetailHeaderView.axaml` - 替换"播放全部"、"随机播放"
  - `Views/Library/AlbumsPageView.axaml` - 替换"专辑"、歌曲数量格式
  - `Views/Library/ArtistsPageView.axaml` - 替换"艺术家"、歌曲数量格式
  - `Views/Details/ArtistDetailView.axaml` - 替换表头和空状态文本
  - `Views/Details/AlbumDetailView.axaml` - 替换表头和空状态文本
  - `Views/Playlist/PlaylistManagementView.axaml` - 替换表头和空状态文本
  - `Views/Player/PlayerView.axaml` - 替换"添加到播放列表"、"移除"
  - `Views/Player/PlayerPageView.axaml` - 替换歌词搜索提示

- **新增字符串资源** (Styles/Strings.axaml):
  - `StringTitleArtist` - 标题 / 艺术家
  - `StringAlbum2` - 专辑
  - `StringDuration2` - 时长
  - `StringNoSongs` - 暂无歌曲
  - `StringNoSongsInArtist` - 该艺术家暂无歌曲
  - `StringNoSongsInAlbum` - 该专辑暂无歌曲
  - `StringNoSongsInPlaylist` - 该播放列表暂无歌曲
  - `StringSongsCount` - 歌曲
  - `StringSongCountFormat` - {0} 首
  - `StringAddToPlaylist` - 添加到播放列表
  - `StringRemove` - 移除
  - `StringSearchLyrics` - 搜索歌词
  - `StringSearching` - 搜索中...
  - `StringSecondsFormat` - {0} 秒

#### 2. 编译绑定启用
- 为所有 XAML 文件启用 `x:CompileBindings="True"`（除 HomeView.axaml 因复杂绑定需要保持 False）
- 提高了绑定表达式的编译时检查

#### 3. MainWindowViewModel 重构
- **事件订阅改进**: 使用 `SubscribeEvent` 辅助方法替代传统事件订阅模式
- **页面导航简化**: 使用字典映射替代冗长的 if-else 链
- **自动资源清理**: 事件订阅自动在 Dispose 时清理

```csharp
// 使用 SubscribeEvent 辅助方法
SubscribeEvent(
    () => {
        _playbackStateService.PlaybackStateChanged += OnPlaybackStateChanged;
        // ...
    },
    () => {
        _playbackStateService.PlaybackStateChanged -= OnPlaybackStateChanged;
        // ...
    }
);

// 使用字典映射简化页面导航
private readonly Dictionary<Type, Func<ViewModelBase?>> _pageLookup = CreatePageLookup();
```

#### 4. ViewLocator 重构
- 使用静态字典替代 switch 表达式
- 提高可维护性，新增 ViewModel 无需修改代码

```csharp
private static readonly Dictionary<Type, Func<Control>> ViewMappings = new()
{
    [typeof(SettingsViewModel)] = () => new Views.Settings.SettingsView(),
    // ...
};
```

#### 5. 异常处理改进
- `Services/Media/LyricsService.cs` - 为 catch 块添加调试日志输出

### 构建状态
- ✅ 项目成功编译
- ✅ 无警告或错误

### 符合规范检查
| 规范 | 状态 |
|-----|------|
| XAML 硬编码中文字符串 | ✅ 已修复 |
| 编译绑定启用 | ✅ 已启用 (除特殊视图) |
| MVVM 模式 | ✅ 已遵循 |
| 事件订阅清理 | ✅ 已优化 |
| 依赖注入 | ✅ 已遵循 |
| 服务层设计 | ✅ 已遵循 |
