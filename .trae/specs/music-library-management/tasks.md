# Tasks - 音乐库管理与元数据编辑

## 依赖关系
- Task 1 可独立执行
- Task 2、3 可并行执行
- Task 4、5 依赖 Task 1、2
- Task 6 依赖 Task 1、3
- Task 7 依赖 Task 4、5、6
- Task 8 独立验证

## 任务列表

- [x] Task 1: 创建文件监听服务 IFileWatcherService 和 FileWatcherService
  - [x] 创建 Services/IFileWatcherService.cs 接口
  - [x] 创建 Services/FileWatcherService.cs 实现
  - [x] 使用 FileSystemWatcher 监听目录变化
  - [x] 实现新增/删除/重命名文件的处理逻辑
  - [x] 实现 IDisposable 正确释放资源

- [x] Task 2: 实现拖拽文件导入功能
  - [x] 在 MainWindow.axaml.cs 中处理 DragEnter/DragOver/Drop 事件
  - [x] 区分文件和文件夹拖入
  - [x] 调用 FileScannerService 扫描并导入
  - [x] 调用 MusicLibraryService 添加到库

- [x] Task 3: 扩展 ScanService 增加文件监听管理
  - [x] 在 IScanService 接口添加 StartWatching/StopWatching/IsWatching
  - [x] 在 ScanService 实现文件监听生命周期管理
  - [x] 与 FileWatcherService 集成
  - [x] 在 App.axaml.cs 注册 IFileWatcherService

- [x] Task 4: 实现播放列表拖拽排序
  - [x] 扩展 PlaylistManagementView 的 ListBox 支持拖拽
  - [x] 在 PlaylistManagementViewModel 添加 MoveSongCommand
  - [x] 调用 UserPlaylistService 重新排序
  - [x] 更新 DragDropSortBehavior 支持 PlaylistManagementViewModel

- [x] Task 5: 实现播放列表重命名功能
  - [x] 在 PlaylistManagementViewModel 实现 RenamePlaylistCommand
  - [x] 创建 IDialogService/DialogService 显示输入对话框
  - [x] 更新 UserPlaylist.Name 并持久化
  - [x] 防止重命名 "favorites" 特殊列表

- [x] Task 6: 实现元数据编辑功能
  - [x] 创建 ViewModels/MetadataEditorViewModel.cs
  - [x] 创建 Views/MetadataEditorView.axaml 和 .cs
  - [x] 实现 Song 属性绑定（Title, Artist, Album, TrackNumber）
  - [x] 使用 TagLibSharp 保存修改到文件
  - [x] 处理专辑封面更换

- [x] Task 7: 集成测试与 UI 集成
  - [x] 在 PlaylistManagementView 添加右键菜单调用元数据编辑
  - [x] 验证拖拽导入在主窗口正常工作
  - [x] 验证文件变化监听正确触发

- [x] Task 8: 编译验证
  - [x] dotnet build 成功
  - [x] 无编译警告
