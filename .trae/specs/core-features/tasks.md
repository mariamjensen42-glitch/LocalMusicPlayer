# Tasks - LocalMusicPlayer 核心功能实现

## 依赖关系
- Task 2、3、4 可并行执行
- Task 5 依赖 Task 1、2、3
- Task 6 依赖 Task 4
- Task 7 依赖 Task 5、6

## 任务列表

- [x] Task 1: 扩展 IFileScannerService 接口
  - 添加支持进度回调和取消令牌的扫描方法重载
  - 添加 SupportedExtensions 属性
  - 创建 Services/IFileScannerService.cs

- [x] Task 2: 实现 FileScannerService
  - 使用 TagLibSharp 读取音频文件元数据
  - 实现进度报告和取消支持
  - 处理元数据读取失败的情况（使用文件名）
  - 创建 Services/FileScannerService.cs

- [x] Task 3: 扩展 IMusicPlayerService 接口
  - 添加 Next/Previous/Seek 方法
  - 添加 SetVolume/Mute 方法和 Volume/IsMuted 属性
  - 添加 PlaybackEnded/PlaybackStateChanged/PositionChanged 事件
  - 创建 Services/IMusicPlayerService.cs

- [x] Task 4: 创建播放状态相关模型
  - 创建 Models/PlayState.cs（枚举）
  - 创建 Models/PlayerStatus.cs（状态类）
  - 创建 Models/PlaybackMode.cs（枚举）
  - 创建 Models/IPlaylistService.cs（接口）

- [x] Task 5: 实现 MusicPlayerService
  - 使用 LibVLCSharp 实现音频播放
  - 实现所有 IMusicPlayerService 接口成员
  - 实现事件通知机制
  - 创建 Services/MusicPlayerService.cs

- [x] Task 6: 实现 PlaylistService
  - 实现播放列表的创建、添加、移除
  - 实现上一曲/下一曲逻辑
  - 实现随机和循环播放模式
  - 创建 Services/PlaylistService.cs

- [x] Task 7: 更新 MainWindowViewModel
  - 注入服务依赖
  - 实现播放控制命令
  - 实现播放列表绑定属性
  - 创建 ViewModels/MainWindowViewModel.cs

- [x] Task 8: 验证编译
  - dotnet build 成功
  - 无警告
