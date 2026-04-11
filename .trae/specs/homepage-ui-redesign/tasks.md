# Tasks

- [x] Task 1: 更新样式资源文件 - 在Resources.axaml中新增UI重构所需的样式和主题
  - [x] 1.1 新增宽侧边栏导航项样式（SidebarNavItemTheme）
  - [x] 1.2 新增歌曲列表行样式（SongListItemRowTheme）
  - [x] 1.3 新增播放控制栏相关样式
  - [x] 1.4 新增浅色主题配色资源（如需要）
  - [x] 1.5 调整现有样式以适配新UI需求

- [x] Task 2: 重构MainWindow.axaml主窗口布局
  - [x] 2.1 实现三区域Grid布局（侧边栏 | 主内容区+顶栏 | 底部播放栏）
  - [x] 2.2 实现宽侧边栏导航（带图标+文字标签的导航菜单）
  - [x] 2.3 实现顶部工具栏（返回按钮、搜索框、设置、窗口控制）
  - [x] 2.4 实现底部播放控制栏（当前歌曲信息、进度条、控制按钮、音量）
  - [x] 2.5 实现主内容区域容器（ContentControl用于页面切换）

- [x] Task 3: 实现所有歌曲页面UI（HomeView组件）
  - [x] 3.1 创建HomeView.axaml用户控件
  - [x] 3.2 实现专辑封面卡片区域
  - [x] 3.3 实现标题和操作按钮区（播放全部、随机播放）
  - [x] 3.4 实现歌曲列表表格（表头+数据行）
  - [x] 3.5 实现歌曲行DataTemplate（序号、封面、标题/艺术家、专辑、喜欢、时长）

- [x] Task 4: 创建HomeViewModel视图模型
  - [x] 4.1 创建HomeViewModel.cs文件
  - [x] 4.2 实现歌曲列表数据绑定属性
  - [x] 4.3 实现播放全部、随机播放等命令
  - [x] 4.4 实现喜欢切换命令
  - [x] 4.5 集成到MainWindowViewModel导航系统

- [x] Task 5: 适配MainWindowViewModel以支持新UI
  - [x] 5.1 修改SidebarWidth逻辑（固定显示，不再根据页面隐藏）
  - [x] 5.2 调整LibraryStats格式为"本地: X首"
  - [x] 5.3 确保所有数据绑定属性与新UI兼容
  - [x] 5.4 测试导航切换功能

- [x] Task 6: 样式微调和响应式优化
  - [x] 6.1 调整各区域间距和对齐
  - [x] 6.2 优化悬停效果和过渡动画
  - [x] 6.3 确保窗口大小调整时布局正确
  - [x] 6.4 验证深色/浅色主题兼容性

- [x] Task 7: 修复验证发现的关键问题
  - [x] 7.1 **[严重]** 修复返回按钮IsVisible绑定错误 - 创建NavPageVisibilityConverter或修改绑定逻辑
  - [x] 7.2 **[严重]** 修复AlbumCardShadowBlur资源定义顺序错误 - 移动到引用位置之前
  - [x] 7.3 **[中等]** 为设置按钮添加NavigateToSettingsCommand绑定
  - [x] 7.4 **[低]** 为全屏按钮添加ToggleFullScreenCommand绑定

# Task Dependencies
- [Task 1] 无依赖，可首先执行
- [Task 2] 依赖 [Task 1] - 需要先有样式资源
- [Task 3] 依赖 [Task 1] - 需要样式资源
- [Task 4] 可与 [Task 2][Task 3] 并行执行
- [Task 5] 依赖 [Task 2][Task 3][Task 4]
- [Task 6] 依赖 [Task 2][Task 3][Task 4][Task 5]
- [Task 7] 依赖 [Task 1-6] 全部完成后的修复任务
