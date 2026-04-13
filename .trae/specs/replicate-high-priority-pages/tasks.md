# Tasks

## 阶段一：基础设施准备（3个任务）

- [x] Task 1: 创建音乐浏览主页面 ViewModel (MusicBrowseViewModel)
  - [x] 1.1 创建 `ViewModels/MusicBrowseViewModel.cs`，继承 ViewModelBase
  - [x] 1.2 实现标签页状态管理属性：`SelectedTab`, `AvailableTabs`
  - [x] 1.3 实现搜索和排序功能：`SearchText`, `SortOption`, `FilterSongs()`
  - [x] 1.4 注入必要的服务：IMusicLibraryService, IPlaylistService, IPlaybackStateService, INavigationService
  - [x] 1.5 实现播放控制命令：PlayCommand, PauseCommand, NextCommand, PreviousCommand, StopCommand
  - [x] 1.6 实现音量控制命令：VolumeUpCommand, VolumeDownCommand, MuteToggleCommand
  - [x] 1.7 实现播放模式切换命令：PlayModeChangedCommand
  - [x] 1.8 实现 Tab 切换逻辑：OnSelectionChanged() 方法

- [x] Task 2: 创建文件夹浏览 ViewModel (FolderBrowseViewModel)
  - [x] 2.1 创建 `ViewModels/FolderBrowseViewModel.cs`，继承 ViewModelBase
  - [x] 2.2 实现文件夹数据源：`Folders` ObservableCollection，按路径分组
  - [x] 2.3 实现选中项管理：`SelectedItem` 属性
  - [x] 2.4 实现右键菜单选项：`FolderMenuOptions` 集合
  - [x] 2.5 实现文件夹操作命令：
    - PlayCommand: 播放该文件夹所有歌曲
    - AddToFavouriteCommand: 添加到收藏
    - AddToPlayListCommand: 添加到播放列表
    - RescanFolderCommand: 重新扫描文件夹
  - [x] 2.6 实现文件夹点击导航逻辑：NavigateToFolderSongs()

- [x] Task 3: 创建歌曲列表 ViewModel (SongListViewModel)
  - [x] 3.1 创建 `ViewModels/SongListViewModel.cs`，继承 ViewModelBase
  - [x] 3.2 实现歌曲数据源：`Songs` 绑定到 IMusicLibraryService.FilteredSongs 或独立集合
  - [x] 3.3 实现选中项管理：`SelectedSong`, `SelectedSongs` (支持多选)
  - [x] 3.4 实现完整的右键菜单选项和命令：
    - PlayCommand: 播放选中歌曲
    - AddToFavouriteCommand: 收藏/取消收藏
    - AddToPlayListCommand: 添加到指定播放列表（动态子菜单）
    - ConvertAudioCommand: 音频格式转换（WAV/MP3/FLAC/OGG/OPUS）
    - AddToCurrentPlayListCommand: 添加到当前播放列表
    - ReGetLyricsCommand: 重新获取歌词
    - OpenInExplorerCommand: 在资源管理器中打开
    - MusicDetailCommand: 显示音乐详情窗口
    - DeleteMenuItemCommand: 删除文件
  - [x] 3.5 实现双击播放逻辑：DoubleTapPlayAsync()
  - [x] 3.6 实现艺人/专辑点击导航：NavigateToArtist(), NavigateToAlbum()

## 阶段二：UI 视图实现（3个任务）

- [x] Task 4: 实现音乐浏览主页面 UI (MusicBrowseView)
  - [x] 4.1 创建 `Views/Library/MusicBrowseView.axaml`
  - [x] 4.2 实现顶部区域布局：
    - 左侧：标签导航栏（TabControl 或自定义 ButtonBar）
    - 右侧：搜索框 + 排序下拉框 + 进度指示器
  - [x] 4.3 实现中间内容区域：ContentControl 用于动态加载子视图
  - [x] 4.4 实现底部播放控制栏（120px 高度）：
    - 第一行：进度条 Slider + 时间显示 TextBlock
    - 第二行三列布局：
      - 左列：封面图片(80x80) + 标题/艺人/专辑信息
      - 中列：播放控制按钮组（收藏、快退、上一曲、播放/暂停、下一曲、快进、停止）
      - 右列：均衡器按钮 + 当前播放列表按钮(Popup) + 播放模式按钮 + 音量图标 + 音量 Slider
  - [x] 4.5 创建 `Views/Library/MusicBrowseView.axaml.cs` 代码后置
  - [x] 4.6 绑定 ViewModel 属性和命令
  - [x] 4.7 应用设计系统样式资源（颜色、字体、圆角等）

- [x] Task 5: 实现文件夹浏览页面 UI (FolderBrowseView)
  - [x] 5.1 创建 `Views/Library/FolderBrowseView.axaml`
  - [x] 5.2 实现网格布局：ItemsControl + WrapPanel 作为容器
  - [x] 5.3 实现分组标题显示（可选：使用 Grouping 支持）
  - [x] 5.4 实现文件夹卡片 DataTemplate：
    - Border(150x150, CornerRadius=10) 包含：
      - Image: 封面图（Stretch=UniformToFill）+ 默认占位图标
      - FontIcon: 设备存在状态图标（右下角）
    - TextBlock: 文件夹名称（Width=150, TextWrapping=Wrap, TextTrimming）
  - [x] 5.5 实现卡片点击事件：调用 FolderBrowseViewModel.NavigateToFolderSongs()
  - [x] 5.6 实现右键上下文菜单：绑定 FolderMenuOptions
  - [x] 5.7 创建 `Views/Library/FolderBrowseView.axaml.cs` 代码后置
  - [x] 5.8 处理空状态提示（无文件夹时显示提示信息）

- [x] Task 6: 实现歌曲列表页面 UI (SongListView)
  - [x] 6.1 创建 `Views/Library/SongListView.axaml`
  - [x] 6.2 实现表头行（可选）：列标题（#、封面、标题、艺人、专辑、音质、时长、操作）
  - [x] 6.3 实现 ListView 布局，SelectionMode="Multiple"
  - [x] 6.4 实现歌曲行 DataTemplate（Grid 9列布局）：
    - Column 0: 封面缩略图 Button(40x40, 圆角5) + AlbumCoverBehavior
    - Column 1: 收藏图标 Button（红心/空心）
    - Column 2: 标题 TextBlock（TextTrimming=CharacterEllipsis）
    - Column 3: 艺人 Button（可点击跳转）
    - Column 4: 专辑 Button（可点击跳转）
    - Column 5: 音质标识 Image（HR/Hi-Res 图标）
    - Column 6: 时长 TextBlock（TimeSpanToStringConverter）
    - Column 7: 添加到播放列表 Button
    - Column 8: 设备存在状态 FontIcon
  - [x] 6.5 实现双击播放事件：GestureEvent.DoubleTapped
  - [x] 6.6 实现右键上下文菜单：绑定 SongListViewModel.MenuOptions
  - [x] 6.7 创建 `Views/Library/SongListView.axaml.cs` 代码后置
  - [x] 6.8 实现滚动到当前播放歌曲方法：ScrollToCurrentSong()
  - [x] 6.9 处理空状态提示（无歌曲时显示提示信息）

## 阶段三：集成与配置（3个任务）

- [ ] Task 7: 注册新页面到导航系统和 DI 容器
  - [ ] 7.1 更新 `Services/Navigation/NavigationService.cs` 注册三个新视图路由
  - [ ] 7.2 更新 `ViewModels/ViewLocator.cs` 添加新的 ViewModel-View 映射
  - [ ] 7.3 在 DI 容器中注册新的 ViewModel（ServiceCollectionExtensions 或 App.axaml.cs）

- [x] Task 8: 集成 MusicBrowseView 到 MainWindow
  - [x] 8.1 修改 `Views/Main/MainWindow.axaml` 主内容区加载 MusicBrowseView
  - [x] 8.2 调整 MainWindowViewModel 引用 MusicBrowseViewModel（默认页面改为 MusicBrowseViewModel）
  - [x] 8.3 确保底部播放控制栏不重复（实现 IsBottomPlayerBarVisible 逻辑自动隐藏/显示）

- [x] Task 9: 添加国际化字符串资源
  - [x] 9.1 在 `Styles/Strings.axaml` 添加新页面所需的所有字符串资源键（已添加36个）
  - [x] 9.2 在所有新 View 的 XAML 中使用 DynamicResource 绑定文本

## 阶段四：测试与优化（2个任务）

- [ ] Task 10: 功能测试与调试
  - [ ] 10.1 编译项目确保无错误
  - [ ] 10.2 测试标签页切换是否正常工作
  - [ ] 10.3 测试文件夹浏览的加载性能（大量文件夹场景）
  - [ ] 10.4 测试歌曲列表的滚动流畅度（1000+ 歌曲场景）
  - [ ] 10.5 测试所有右键菜单命令是否正确执行
  - [ ] 10.6 测试双击播放、多选批量操作
  - [ ] 10.7 测试底部播放控制栏的所有按钮和滑块
  - [ ] 10.8 测试搜索过滤和排序功能的实时性

- [x] Task 11: 性能优化与代码审查
  - [x] 11.1 编译测试：0错误0警告 ✅
  - [x] 11.2 代码质量审查：
    - 命名规范检查 ✅ 通过
    - 异步编程规范 ⚠️ 修复1个问题（同步阻塞→fire-and-forget）
    - MVVM 模式合规性 ✅ 通过
    - 国际化合规性 ✅ 通过
    - 内存泄漏检查 ⚠️ 修复2个问题（事件订阅清理）
  - [x] 11.3 文档更新：docs/fc.md 已追加功能记录章节

# Task Dependencies

- [Task 2] 依赖于 [Task 1] (FolderBrowseViewModel 可能需要引用 MusicBrowseViewModel 的导航方法)
- [Task 3] 依赖于 [Task 1] (SongListViewModel 需要调用 MusicBrowseViewModel 的播放方法)
- [Task 4] 依赖于 [Task 1] (View 需要 ViewModel)
- [Task 5] 依赖于 [Task 2]
- [Task 6] 依赖于 [Task 3]
- [Task 7] 依赖于 [Task 4, Task 5, Task 6] (需要先创建好 View 才能注册路由)
- [Task 8] 依赖于 [Task 4, Task 7]
- [Task 9] 可与 [Task 4, 5, 6] 并行进行
- [Task 10] 依赖于 [Task 4, 5, 6, 7, 8, 9]
- [Task 11] 依赖于 [Task 10]

## 并行执行建议

**第一批（可并行）**: Task 1, Task 2, Task 3, Task 9
**第二批**: Task 4, Task 5, Task 6（分别依赖 1, 2, 3）
**第三批（顺序）**: Task 7 → Task 8
**第四批**: Task 10 → Task 11
