# Checklist - LocalMusicPlayer 核心功能

## 接口定义

- [x] IFileScannerService 接口扩展完成（进度回调、取消支持、SupportedExtensions）
- [x] IMusicPlayerService 接口扩展完成（Next/Previous/Seek/SetVolume/Mute/事件）
- [x] IPlaylistService 接口定义完成

## 模型定义

- [x] PlayState 枚举定义完成（Stopped, Playing, Paused）
- [x] PlaybackMode 枚举定义完成（Normal, Shuffle, Loop）
- [x] PlayerStatus 类定义完成

## 服务实现

- [x] FileScannerService 实现：
  - [x] 递归扫描指定目录
  - [x] 使用 TagLibSharp 读取元数据
  - [x] 元数据读取失败时使用文件名作为标题
  - [x] 支持进度报告回调
  - [x] 支持取消令牌
  - [x] 支持音频格式：MP3, FLAC, WAV, AAC, OGG, M4A

- [x] MusicPlayerService 实现：
  - [x] Play 方法正确加载和播放歌曲
  - [x] Pause 方法暂停播放
  - [x] Resume 方法恢复播放
  - [x] Stop 方法停止播放并重置位置
  - [x] Next 方法播放下一曲
  - [x] Previous 方法播放上一曲
  - [x] Seek 方法跳转到指定位置
  - [x] SetVolume 方法设置音量
  - [x] Mute 方法切换静音状态
  - [x] Position 属性返回当前播放位置
  - [x] Duration 属性返回当前歌曲时长
  - [x] IsPlaying 属性返回播放状态
  - [x] Volume 属性返回当前音量
  - [x] IsMuted 属性返回静音状态
  - [x] PlaybackEnded 事件在播放完成时触发
  - [x] PlaybackStateChanged 事件在状态变化时触发
  - [x] PositionChanged 事件在位置变化时触发

- [x] PlaylistService 实现：
  - [x] CreatePlaylist 方法创建播放列表
  - [x] AddSongToPlaylist 方法添加歌曲
  - [x] RemoveSongFromPlaylist 方法移除歌曲
  - [x] PlayNext 方法根据播放模式选择下一曲
  - [x] PlayPrevious 方法播放上一曲
  - [x] Shuffle 模式随机选择下一曲
  - [x] Loop 模式在播完最后一曲后回到第一曲
  - [x] 当前播放索引正确维护

## ViewModel 集成

- [x] MainWindowViewModel 依赖注入服务
- [x] 播放控制命令实现（Play/Pause/Stop/Next/Previous）
- [x] 播放列表属性绑定
- [x] 搜索功能属性和命令
- [x] 播放器状态变化时 UI 更新的属性通知

## 代码质量

- [x] 所有服务接口命名符合规范
- [x] 所有类/接口使用 PascalCase
- [x] 私有字段使用 _camelCase
- [x] 无硬编码中文字符串
- [x] 使用 x:DataType 编译期绑定（代码结构支持）

## 编译验证

- [x] dotnet build 成功
- [x] 无编译警告
