# 音乐库管理与元数据编辑功能规格说明

## Why

当前 LocalMusicPlayer 已具备基础的音乐扫描、播放列表管理和元数据读取能力，但缺少以下关键功能：

1. **拖拽导入** - 用户无法通过拖拽文件/文件夹直接导入音乐
2. **文件变化监听** - 扫描后外部文件变化无法自动感知
3. **播放列表拖拽排序** - 歌曲顺序无法通过拖拽调整
4. **播放列表重命名** - 重命名功能未实现
5. **元数据编辑** - 无法修改歌曲的标题、艺术家、专辑等元数据

## What Changes

### 新增功能
- **拖拽文件导入**：支持拖拽音频文件或文件夹到应用窗口进行导入
- **文件变化监听**：使用 FileSystemWatcher 监听扫描文件夹的文件变化，自动更新音乐库
- **播放列表拖拽排序**：在播放列表管理界面支持拖拽歌曲调整顺序
- **播放列表重命名**：支持重命名用户创建的播放列表
- **元数据编辑**：提供歌曲元数据编辑对话框，支持修改标题、艺术家、专辑、封面等

### 修改功能
- **ScanService**：增加文件变化监听管理
- **PlaylistManagementViewModel**：完善重命名命令，添加拖拽排序支持
- **FileScannerService**：支持增量扫描（添加新文件）

### 新增组件
- **MetadataEditorView / MetadataEditorViewModel**：元数据编辑对话框
- **DragDropFileBehavior**：文件拖拽导入行为
- **FileWatcherService / IFileWatcherService**：文件变化监听服务

## Impact

- Affected specs: 音乐扫描、播放列表管理、用户界面
- Affected code:
  - Services/（新增 FileWatcherService）
  - ViewModels/（新增 MetadataEditorViewModel）
  - Views/（新增 MetadataEditorView，修改 PlaylistManagementView）
  - Behaviors/（新增 DragDropFileBehavior）
  - Models/（Song 模型可能需要扩展）

## ADDED Requirements

### Requirement: 拖拽文件导入

系统 SHALL 支持通过拖拽方式导入音频文件或文件夹。

#### Scenario: 拖拽单个音频文件
- **WHEN** 用户拖拽一个 MP3/FLAC 文件到应用窗口
- **THEN** 系统解析文件元数据并添加到音乐库

#### Scenario: 拖拽文件夹
- **WHEN** 用户拖拽一个包含音频文件的文件夹到应用窗口
- **THEN** 系统递归扫描文件夹并导入所有支持的音频文件

#### Scenario: 拖拽混合内容
- **WHEN** 用户拖拽包含音频文件和非音频文件的文件夹
- **THEN** 系统仅导入支持的音频格式文件，跳过其他文件

### Requirement: 文件变化监听

系统 SHALL 监听扫描文件夹的文件变化并自动更新音乐库。

#### Scenario: 检测到新文件
- **WHEN** 监听目录下新增一个音频文件
- **THEN** 系统自动扫描该文件并添加到音乐库

#### Scenario: 检测到文件删除
- **WHEN** 音乐库中的文件被外部删除
- **THEN** 系统自动从音乐库中移除该歌曲记录

#### Scenario: 检测到文件移动/重命名
- **WHEN** 音乐库中的文件被移动或重命名
- **THEN** 系统更新该歌曲的文件路径信息

#### Scenario: 监听可控制
- **WHEN** 用户在设置中关闭文件监听
- **THEN** 系统停止 FileSystemWatcher，不再自动更新

### Requirement: 播放列表拖拽排序

系统 SHALL 支持在播放列表管理界面通过拖拽调整歌曲顺序。

#### Scenario: 拖拽歌曲到新位置
- **WHEN** 用户在播放列表中拖拽歌曲从索引 3 到索引 1
- **THEN** 歌曲移动到新位置，原索引 1-2 的歌曲顺序后移

#### Scenario: 拖拽到列表两端
- **WHEN** 用户拖拽歌曲到播放列表最顶部或最底部
- **THEN** 歌曲移动到对应端点位置

#### Scenario: 拖拽时显示视觉反馈
- **WHEN** 用户开始拖拽
- **THEN** 显示拖拽指示器，提示放置位置

### Requirement: 播放列表重命名

系统 SHALL 支持重命名用户创建的播放列表。

#### Scenario: 重命名播放列表
- **WHEN** 用户选择播放列表并点击重命名
- **THEN** 弹出输入对话框，用户输入新名称后确认
- **AND** 播放列表名称更新并持久化

#### Scenario: 特殊列表不可重命名
- **WHEN** 用户尝试重命名 "Favorites"（收藏）列表
- **THEN** 重命名按钮禁用或提示不可重命名

### Requirement: 元数据编辑

系统 SHALL 提供元数据编辑功能，允许用户查看和修改歌曲信息。

#### Scenario: 打开元数据编辑
- **WHEN** 用户在歌曲列表中右键点击歌曲并选择"编辑信息"
- **THEN** 弹出元数据编辑对话框，显示当前歌曲信息

#### Scenario: 编辑标题
- **WHEN** 用户修改歌曲标题为空字符串
- **THEN** 保存时使用文件名作为默认标题

#### Scenario: 编辑艺术家和专辑
- **WHEN** 用户修改艺术家和专辑信息
- **THEN** 更改保存到文件元数据（使用 TagLibSharp）

#### Scenario: 编辑专辑封面
- **WHEN** 用户在编辑界面点击更换封面
- **THEN** 打开文件选择对话框，选择新图片
- **AND** 封面保存到音频文件元数据

#### Scenario: 批量编辑
- **WHEN** 用户选择多首歌曲并打开编辑
- **THEN** 显示批量编辑模式，公共字段可统一修改

## MODIFIED Requirements

### Requirement: ScanService 扩展

在现有 IScanService 接口基础上增加：

| 方法 | 说明 |
|------|------|
| `StartWatching()` | 开始监听扫描文件夹的文件变化 |
| `StopWatching()` | 停止文件监听 |
| `IsWatching` 属性 | 返回当前是否正在监听 |

### Requirement: PlaylistManagementViewModel 扩展

在现有 PlaylistManagementViewModel 基础上增加：

| 功能 | 说明 |
|------|------|
| `RenamePlaylistCommand` | 实现重命名播放列表逻辑 |
| `MoveSongCommand` | 处理歌曲在播放列表中的移动 |
| 拖拽排序支持 | 集成 DragDropSortBehavior |

## REMOVED Requirements

无

## 技术实现约束

### 依赖注入
- 新服务通过接口 IFileWatcherService 注入
- ViewModel 依赖服务接口而非具体实现

### 线程安全
- FileSystemWatcher 事件在单独线程触发，需要 Dispatcher 切换到 UI 线程
- 元数据编辑保存时显示进度指示

### 错误处理
- 文件监听错误（如文件夹被删除）应优雅降级并记录日志
- 元数据保存失败时提示用户并保留更改

### UI 约束
- 使用 x:DataType 编译期绑定
- XAML 中不硬编码中文字符串（使用资源文件）
- 对话框使用 NativeDialogService 或自定义窗口
