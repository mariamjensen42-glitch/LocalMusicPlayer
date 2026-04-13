# Checklist

## ViewModel 验证

- [ ] MusicBrowseViewModel 正确继承 ViewModelBase 并实现 IDisposable 模式
- [ ] MusicBrowseViewModel 所有属性使用 [ObservableProperty] 特性并正确触发属性更改通知
- [ ] MusicBrowseViewModel 所有命令方法标记 [RelayCommand] 特性
- [ ] MusicBrowseViewModel 通过构造函数注入所有依赖服务（不直接实例化）
- [ ] FolderBrowseViewModel.Folders 数据源正确分组并支持动态更新
- [ ] FolderBrowseViewModel 右键菜单选项完整且命令绑定正确
- [ ] SongListViewModel 支持单选和多选模式切换
- [ ] SongListViewModel.MenuOptions 包含所有 10 个菜单项及其子菜单
- [ ] SongListViewModel.ConvertAudioCommand 支持 5 种目标格式（WAV/MP3/FLAC/OGG/OPUS）
- [ ] 所有 ViewModel 的异步方法以 Async 结尾并返回 Task/Task<T>

## View UI 验证

- [ ] MusicBrowseView.axaml 使用 Avalonia 控件（不包含 WinUI 特有控件）
- [ ] MusicBrowseView 顶部标签栏包含 6 个标签项（歌曲、专辑、艺人、文件夹、收藏、播放列表）
- [ ] MusicBrowseView 底部播放控制栏高度为 120px 且布局与原始 WinUI 版本一致
- [ ] MusicBrowseView 进度条 Slider 绑定到正确的 ViewModel 属性
- [ ] MusicBrowseView 音量 Slider 范围为 0-100
- [ ] FolderBrowseView 使用 ItemsControl + WrapPanel 实现网格布局
- [ ] FolderBrowseView 文件夹卡片尺寸为 150x150 像素且圆角为 10
- [ ] FolderBrowseView 文件夹卡片显示封面图和文件夹名称
- [ ] SongListView 使用 ListView 控件且 SelectionMode="Multiple"
- [ ] SongListView 歌曲行 DataTemplate 包含 9 列信息（封面、收藏、标题、艺人、专辑、音质、时长、操作按钮、设备状态）
- [ ] SongListView 封面缩略图尺寸为 40x40 像素且圆角为 5
- [ ] 所有 View 不硬编码中文字符串，全部使用 DynamicResource 绑定
- [ ] 所有 View 应用设计系统资源（颜色、字体、圆角、间距）

## 功能完整性验证

- [ ] 标签页点击可正常切换子页面内容
- [ ] 标签切换时带有过渡动画效果（可选）
- [ ] 搜索框输入时可实时过滤当前页面数据
- [ ] 排序下拉框可选择不同排序方式并立即生效
- [ ] 文件夹卡片单击可导航到该文件夹的歌曲列表
- [ ] 文件夹右键菜单显示 4 个选项：播放、收藏、添加到播放列表、重新扫描
- [ ] 歌曲双击可立即播放并将列表设为播放队列
- [ ] 歌曲 Ctrl+Click 可多选，Shift+Click 可范围选择
- [ ] 歌曲右键菜单显示完整 10 个选项
- [ ] 收藏按钮可在红心和空心图标间切换
- [ ] 艺人/专辑名称点击可导航到对应详情页
- [ ] 播放控制栏所有按钮响应正常（播放/暂停、上下曲、快进快退、停止）
- [ ] 进度条拖拽可调整播放位置
- [ ] 音量滑块拖拽和图标点击静音功能正常
- [ ] 播放模式按钮可循环切换（单曲循环→列表循环→随机→关闭）
- [ ] 当前播放列表 Popup 显示正确并可点击跳转
- [ ] 音频格式转换显示进度对话框并在完成后关闭
- [ ] 删除操作前弹出确认对话框
- [ ] 打开文件位置可正确启动资源管理器并选中文件

## 导航集成验证

- [ ] NavigationService 已注册三个新视图路由（musicbrowse, folderbrowse, songlist）
- [ ] ViewLocator 已添加新的 ViewModel-View 映射关系
- [ ] DI 容器已注册三个新 ViewModel 为 Transient 或 Singleton
- [ ] MainWindow 主内容区成功加载 MusicBrowseView
- [ ] 标签页内部导航不影响 MainWindow 的导航历史栈
- [ ] 返回按钮在标签页导航历史中正常工作

## 国际化验证

- [ ] Strings.axaml 包含所有新增的字符串资源键（至少 30+ 个）
- [ ] 所有用户可见文本均通过 DynamicResource 或 Binding 绑定
- [ ] XAML 中不存在中文字面量字符串
- [ ] 菜单项标题、提示信息均已国际化
- [ ] 空状态提示文本已国际化

## 性能与质量验证

- [ ] 文件夹浏览页面加载 100+ 文件夹时无明显卡顿（<500ms）
- [ ] 歌曲列表滚动 1000+ 歌曲时 FPS 保持稳定（≥30fps）
- [ ] 图片加载使用异步方式，不阻塞 UI 线程
- [ ] 内存占用合理，切换标签页后释放不可见页面资源
- [ ] 无内存泄漏（事件订阅在 Dispose 中正确取消订阅）
- [ ] 编译无错误、无警告（运行 dotnet build 成功）
- [ ] 代码遵循项目命名规范（PascalCase 类名/方法名、_camelCase 私有字段）
- [ ] 异步编程规范符合要求（async/await、ConfigureAwait 使用正确）
- [ ] 日志记录规范符合要求（使用 ILogger、适当的日志级别）

## 兼容性验证

- [ ] 在 Windows 10/11 上正常运行
- [ ] 支持高 DPI 缩放（125%、150%、200%）
- [ ] 支持窗口大小调整，布局自适应
- [ ] 支持深色/浅色主题切换（如果项目支持多主题）
- [ ] 键盘快捷键可用（空格播放/暂停、方向键切歌等）

## 文档更新验证

- [ ] docs/fc.md 已记录本次新增的 3 个页面功能
- [ ] 如有修复 bug，docs/fix.md 已记录（如有）
