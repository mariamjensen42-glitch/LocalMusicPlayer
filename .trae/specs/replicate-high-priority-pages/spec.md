# 高优先级页面复刻规格说明

## Why

当前 Avalonia 版本的音乐播放器缺少 3 个核心浏览页面（音乐浏览、文件夹浏览、歌曲列表），这些页面是用户日常使用音乐播放器的核心功能入口。复刻这些页面将使应用功能完整度从 58.7% 提升至约 75%，为用户提供完整的音乐库浏览体验。

## What Changes

### 1. 音乐浏览主页面 (MusicBrowseView)
- **功能**: 作为音乐库的主导航容器，集成标签页切换、搜索排序、底部播放控制栏
- **组件**:
  - 顶部标签导航栏 (SelectorBar): 歌曲/专辑/艺人/文件夹/收藏/播放列表
  - 搜索和排序工具栏
  - 内容区域 (Frame/ContentControl) 用于加载子页面
  - 底部播放控制栏 (120px 高度):
    - 进度条和播放时间显示
    - 当前播放信息（封面、标题、艺人、专辑）
    - 播放控制按钮（收藏、快退、上一曲、播放/暂停、下一曲、快进、停止）
    - 右侧控制区（均衡器、当前播放列表、播放模式、音量）

### 2. 文件夹浏览页面 (FolderBrowseView)
- **功能**: 以网格视图展示所有音乐文件夹，支持分组显示和语义缩放
- **组件**:
  - SemanticZoom 控件（详细视图 + 概览视图）
  - GridView 展示文件夹卡片（150x150 封面 + 文件夹名称）
  - 分组标题（按首字母或路径分组）
  - 右键菜单（播放、添加到收藏、添加到播放列表、重新扫描）
  - 设备存在状态图标

### 3. 歌曲列表页面 (SongListView)
- **功能**: 以列表视图展示所有歌曲，提供完整的歌曲信息和管理操作
- **组件**:
  - ListView 展示歌曲列表（多列布局）
  - 歌曲信息列：封面缩略图(40x40)、收藏图标、标题、艺人、专辑、音质标识、时长、添加到播放列表按钮、设备状态
  - 双击播放、右键上下文菜单
  - 多选支持（Extended SelectionMode）
  - 菜单选项：播放、收藏、添加到播放列表、音频格式转换(WAV/MP3/FLAC/OGG/OPUS)、添加到当前播放列表、重新获取歌词、打开文件位置、属性、删除

## Impact

### 受影响的现有代码
- `Views/Library/HomeView.axaml` - 需要重构为 MusicBrowseView 的子页面或调整定位
- `ViewModels/HomeViewModel.cs` - 需要创建新的 ViewModel 或扩展
- `Views/Main/MainWindow.axaml` - 需要集成新的页面导航结构
- `Services/Navigation/NavigationService.cs` - 需要注册新页面路由
- `Styles/Resources.axaml` - 可能需要添加新样式资源

### 新增文件
```
Views/
├── Library/
│   ├── MusicBrowseView.axaml          # 音乐浏览主页面
│   ├── MusicBrowseView.axaml.cs       # 代码后置
│   ├── FolderBrowseView.axaml         # 文件夹浏览页面
│   ├── FolderBrowseView.axaml.cs      # 代码后置
│   └── SongListView.axaml             # 歌曲列表页面
│   └── SongListView.axaml.cs          # 代码后置

ViewModels/
├── MusicBrowseViewModel.cs            # 音乐浏览主 ViewModel
├── FolderBrowseViewModel.cs           # 文件夹浏览 ViewModel
└── SongListViewModel.cs               # 歌曲列表 ViewModel
```

## ADDED Requirements

### Requirement: 音乐浏览主页面容器
系统应提供一个统一的音乐浏览容器页面，包含标签导航、内容区域和播放控制栏。

#### Scenario: 标签页切换
- **WHEN** 用户点击顶部标签栏的"歌曲"/"专辑"/"艺人"/"文件夹"/"收藏"/"播放列表"
- **THEN** 内容区域平滑过渡到对应的子页面，标签高亮状态更新

#### Scenario: 底部播放控制
- **WHEN** 用户在任意子页面操作时
- **THEN** 底部播放控制栏始终可见并响应播放控制命令（播放/暂停、上下曲、进度拖拽、音量调节）

#### Scenario: 全局搜索和排序
- **WHEN** 用户在搜索框输入关键词或选择排序选项
- **THEN** 当前激活的子页面数据源实时过滤/排序更新

### Requirement: 文件夹网格浏览
系统应以网格形式展示所有包含音乐的文件夹，支持分组和快速跳转。

#### Scenario: 文件夹卡片展示
- **WHEN** 系统加载完成
- **THEN** 每个文件夹以 150x150 卡片展示，包含该文件夹下第一首歌曲的封面图和文件夹名称

#### Scenario: 分组浏览
- **WHEN** 文件夹数量超过阈值
- **THEN** 自动按首字母或父目录分组，支持 SemanticZoom 快速跳转到指定分组

#### Scenario: 文件夹操作
- **WHEN** 用户右键点击文件夹卡片
- **THEN** 显示上下文菜单：播放该文件夹所有歌曲、添加到收藏、添加到播放列表、重新扫描文件夹

### Requirement: 歌曲列表详细浏览
系统应以表格形式展示所有歌曲的详细信息，支持批量操作和快捷播放。

#### Scenario: 歌曲信息展示
- **WHEN** 系统加载歌曲列表
- **THEN** 每行显示：封面缩略图、收藏状态、标题、艺人、专辑、音质标识、时长、操作按钮、设备存在状态

#### Scenario: 快捷播放
- **WHEN** 用户双击某首歌曲
- **THEN** 立即播放该歌曲，并将整个列表设置为当前播放队列

#### Scenario: 批量操作
- **WHEN** 用户通过 Ctrl+Click 或 Shift+Click 选择多首歌曲后右键
- **THEN** 菜单选项作用于选中的所有歌曲（批量收藏、批量添加到播放列表、批量转换格式、批量删除）

#### Scenario: 音频格式转换
- **WHEN** 用户选择转换格式（WAV/MP3/FLAC/OGG/OPUS）
- **THEN** 显示进度对话框，逐个转换选中歌曲为目标格式

## MODIFIED Requirements

### Requirement: HomeView 定位调整
原有的 HomeView 将作为"歌曲"标签页的默认内容嵌入到 MusicBrowseView 中，不再作为独立的主页面使用。
- **原因**: 原始 WinUI 项目中 SongListPage 是 MusicBrowsePage 的子页面
- **迁移方案**: 将 HomeView 的列表视图部分提取为 SongListView，HomeView 的统计信息移至 MusicBrowseView 顶部

### Requirement: 导航服务扩展
NavigationService 需要支持 Frame 内嵌套导航和标签页切换逻辑。
- **新增**: `NavigateToTab(string tabTag)` 方法用于标签切换
- **新增**: 页面历史栈管理，支持返回按钮在标签页间导航

## REMOVED Requirements

无

## 技术约束

1. **UI 框架兼容性**: 使用 Avalonia UI 控件替代 WinUI 控件
   - SelectorBar → 自定义 TabControl 或 Button 组
   - SemanticZoom → 可选实现（Avalonia 无原生支持，可用分组列表替代）
   - GridView → ItemsControl + WrapPanel
   - Frame → ContentControl + ViewLocator

2. **MVVM 模式**: 遵循 CommunityToolkit.Mvvm 和项目现有的 ViewModelBase 模式

3. **样式一致性**: 复用 `Styles/Resources.axaml` 中定义的设计系统资源（颜色、字体、圆角等）

4. **国际化**: 所有用户可见文本必须使用 `{DynamicResource StringXxx}` 资源键，禁止硬编码中文字符串

5. **异步编程**: 所有耗时操作使用 async/await 模式，方法名以 Async 结尾

6. **依赖注入**: 通过构造函数注入服务接口，不直接实例化具体服务类
