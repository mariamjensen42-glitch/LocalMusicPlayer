# Checklist - 音乐库管理与元数据编辑

## 接口定义

- [x] IFileWatcherService 接口定义完成（StartWatching/StopWatching/IsWatching）
- [x] IScanService 接口扩展完成（StartWatching/StopWatching/IsWatching）

## 服务实现

- [x] FileWatcherService 实现：
  - [x] FileSystemWatcher 正确初始化
  - [x] 新增文件检测并触发扫描
  - [x] 删除文件检测并从库移除
  - [x] 重命名/移动文件检测并更新路径
  - [x] Dispose 正确释放资源
  - [x] 线程安全（Dispatcher 切换到 UI 线程）

- [x] 拖拽导入实现（MainWindow 直接处理）：
  - [x] DragOver 显示正确拖拽指示
  - [x] Drop 处理文件拖入
  - [x] Drop 处理文件夹拖入
  - [x] 正确调用扫描服务导入

- [x] ScanService 扩展：
  - [x] StartWatching 启动所有扫描文件夹的监听
  - [x] StopWatching 停止所有监听
  - [x] IsWatching 属性正确反映状态

## ViewModel 实现

- [x] PlaylistManagementViewModel：
  - [x] RenamePlaylistCommand 实现重命名逻辑
  - [x] MoveSongCommand 实现歌曲移动逻辑
  - [x] EditSongMetadataCommand 打开元数据编辑对话框
  - [x] 与 UserPlaylistService 正确交互

- [x] MetadataEditorViewModel：
  - [x] Title/Artist/Album/TrackNumber 属性绑定
  - [x] SaveCommand 保存更改到文件
  - [x] CancelCommand 取消更改
  - [x] ChangeCoverCommand 更换专辑封面

## View 实现

- [x] MetadataEditorView：
  - [x] 表单布局显示歌曲信息
  - [x] 封面更换按钮功能
  - [x] 保存/取消按钮

- [x] PlaylistManagementView：
  - [x] 拖拽排序功能正常（通过 DragDropSortBehavior）
  - [x] 右键菜单包含"Edit Info"选项
  - [x] 重命名按钮功能正常

## UI 集成

- [x] 主窗口支持拖拽文件导入
- [x] 文件变化自动更新音乐库（通过 ScanService 集成）
- [x] 播放列表重命名对话框正常（通过 DialogService）
- [x] 元数据编辑对话框正常

## 代码质量

- [x] 服务接口命名符合规范
- [x] 所有类/接口使用 PascalCase
- [x] 私有字段使用 _camelCase
- [x] 无硬编码中文字符串
- [x] 使用 x:DataType 编译期绑定

## 编译验证

- [x] dotnet build 成功
- [x] 编译警告不影响功能（API 过时警告）
