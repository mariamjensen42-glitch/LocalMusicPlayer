# LocalMusicPlayer 核心功能规格说明

## Why

为 LocalMusicPlayer 项目提供清晰的核心业务逻辑规格说明，确保音乐扫描、播放控制、播放列表管理等功能的接口定义和实现要求明确。由于用户要求先不写UI，本规格专注于后端服务和数据模型。

## What Changes

### 新增服务实现
- **FileScannerService**: 目录扫描和元数据读取服务实现
- **MusicPlayerService**: 基于 LibVLCSharp 的音频播放服务实现

### 新增数据模型
- **PlayState 枚举**: 播放状态定义（Stopped, Playing, Paused）
- **PlayerStatus 类**: 播放器当前状态（Position, Duration, IsPlaying, Volume）

### 新增服务接口
- **IPlaylistService**: 播放列表管理接口

### 修改现有接口
- **IFileScannerService**: 补充进度回调和取消令牌支持
- **IMusicPlayerService**: 扩展播放控制能力（上一曲/下一曲/跳转）、事件通知、播放模式

## Impact

- Affected specs: 音乐扫描、播放控制、播放列表管理
- Affected code:
  - Services/ 目录（新增服务实现）
  - Models/ 目录（新增枚举和状态类）
  - ViewModels/ 目录（MainWindowViewModel 将依赖新服务）

## ADDED Requirements

### Requirement: 音乐文件扫描服务

系统 SHALL 提供音乐文件扫描功能，通过 `IFileScannerService` 接口实现。

#### Scenario: 扫描包含多种格式的文件夹
- **WHEN** 调用 `ScanDirectoryAsync(path)` 扫描包含 MP3、FLAC、WAV 文件的文件夹
- **THEN** 返回包含所有音频文件元数据的 `List<Song>`，每个 Song 包含 Title、Artist、Album、FilePath、Duration

#### Scenario: 扫描过程可取消
- **WHEN** 扫描过程中调用 `CancellationToken.Cancel()`
- **THEN** 扫描立即停止，返回已扫描的部分结果

#### Scenario: 读取元数据失败时使用文件名
- **WHEN** 文件元数据读取失败（如损坏的标签）
- **THEN** 使用文件名（不含扩展名）作为 Title，其他字段使用默认值

### Requirement: 音频播放服务

系统 SHALL 提供音频播放控制功能，通过 `IMusicPlayerService` 接口实现。

#### Scenario: 播放指定歌曲
- **WHEN** 调用 `Play(song)` 并传入有效的 Song 对象
- **THEN** 开始播放歌曲，IsPlaying = true，Position 从 0 开始增长

#### Scenario: 暂停和恢复
- **WHEN** 播放过程中调用 `Pause()`
- **THEN** 播放暂停，IsPlaying = false，Position 保持不变
- **WHEN** 调用 `Resume()`
- **THEN** 从暂停位置继续播放，IsPlaying = true

#### Scenario: 停止播放
- **WHEN** 调用 `Stop()`
- **THEN** 停止播放，Position 重置为 0，IsPlaying = false

#### Scenario: 获取播放进度
- **WHEN** 播放过程中查询 `Position` 属性
- **THEN** 返回当前播放位置（TimeSpan）
- **WHEN** 查询 `Duration` 属性
- **THEN** 返回当前歌曲总时长

#### Scenario: 音量控制
- **WHEN** 调用 `SetVolume(level)` 并传入 0-100 的值
- **THEN** 音量设置为指定值
- **WHEN** 调用 `Mute()`
- **THEN** 静音，音量恢复后能还原到之前值

#### Scenario: 播放完成通知
- **WHEN** 当前歌曲播放完毕
- **THEN** 触发 `PlaybackEnded` 事件

### Requirement: 播放列表管理服务

系统 SHALL 提供播放列表管理功能，通过 `IPlaylistService` 接口实现。

#### Scenario: 创建播放列表
- **WHEN** 调用 `CreatePlaylist(name)` 并传入播放列表名称
- **THEN** 创建并返回新的 Playlist 对象

#### Scenario: 添加歌曲到播放列表
- **WHEN** 调用 `AddSongToPlaylist(playlist, song)`
- **THEN** 将 song 添加到 playlist.Songs

#### Scenario: 从播放列表移除歌曲
- **WHEN** 调用 `RemoveSongFromPlaylist(playlist, songIndex)`
- **THEN** 从 playlist.Songs 移除指定索引的歌曲

#### Scenario: 播放下一曲
- **WHEN** 调用 `PlayNext()`
- **THEN** 如果有下一首，播放它并返回 true；否则返回 false

#### Scenario: 播放上一曲
- **WHEN** 调用 `PlayPrevious()`
- **THEN** 如果有上一首，播放它并返回 true；否则返回 false

#### Scenario: 随机播放模式
- **WHEN** 设置 `PlaybackMode = Shuffle`
- **THEN** 下一曲从播放列表随机选择

#### Scenario: 列表循环模式
- **WHEN** 设置 `PlaybackMode = Loop`
- **THEN** 播放到最后一首后自动回到第一首继续播放

### Requirement: 播放器状态通知

系统 SHALL 通过事件机制通知播放器状态变化。

#### Scenario: 播放状态变化通知
- **WHEN** 播放状态发生变化（播放/暂停/停止）
- **THEN** 触发 `PlaybackStateChanged` 事件，携带新状态

#### Scenario: 播放进度变化通知
- **WHEN** 播放位置每秒钟更新
- **THEN** 触发 `PositionChanged` 事件，携带当前位置

#### Scenario: 歌曲变化通知
- **WHEN** 当前播放歌曲发生变化
- **THEN** 触发 `CurrentSongChanged` 事件，携带新歌曲

## MODIFIED Requirements

### Requirement: IFileScannerService 接口扩展

在现有 `Task<List<Song>> ScanDirectoryAsync(string path)` 方法基础上增加：

| 方法 | 说明 |
|------|------|
| `ScanDirectoryAsync(path, progress, cancellationToken)` | 支持进度报告和取消 |
| `SupportedExtensions` 属性 | 返回支持的音频文件扩展名列表 |

### Requirement: IMusicPlayerService 接口扩展

在现有接口基础上增加：

| 方法/属性 | 说明 |
|------|------|
| `Next()` | 播放下一曲 |
| `Previous()` | 播放上一曲 |
| `Seek(position)` | 跳转到指定位置 |
| `SetVolume(level)` | 设置音量 (0-100) |
| `Mute()` | 静音切换 |
| `Volume` 属性 | 当前音量 |
| `IsMuted` 属性 | 静音状态 |
| `PlaybackEnded` 事件 | 播放完成时触发 |
| `PlaybackStateChanged` 事件 | 播放状态变化时触发 |
| `PositionChanged` 事件 | 播放位置变化时触发 |

## REMOVED Requirements

无

## 技术实现约束

### 依赖注入
- 所有服务接口通过构造函数注入（Dependency Injection）
- ViewModel 依赖服务接口而非具体实现

### 线程安全
- 播放状态属性访问需要线程安全
- UI 线程通过 `ObserveOn(RxApp.MainThreadScheduler)` 更新

### 错误处理
- 文件扫描失败记录日志但不影响整体流程
- 播放失败触发 `PlaybackEnded` 事件而非抛出异常
