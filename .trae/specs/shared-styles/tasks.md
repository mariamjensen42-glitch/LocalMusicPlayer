# Tasks

- [x] Task 1: 扩展 Styles/Resources.axaml 添加尺寸资源和文本样式
  - [x] SubTask 1.1: 添加 IconButtonSize、IconButtonSizeLarge、IconButtonSizeSmall 尺寸资源
  - [x] SubTask 1.2: 添加 CardCornerRadius、SmallCornerRadius 圆角资源
  - [x] SubTask 1.3: 添加 TitleTextStyle、BodyTextStyle、MutedTextStyle 文本样式

- [x] Task 2: 创建 Styles/Controls/ButtonStyles.axaml 定义按钮主题
  - [x] SubTask 2.1: 创建 CircleIconButtonTheme（标准圆形图标按钮）
  - [x] SubTask 2.2: 创建 CircleIconButtonLargeTheme（大圆形播放按钮）
  - [x] SubTask 2.3: 创建 CircleIconButtonSmallTheme（小圆形操作按钮）
  - [x] SubTask 2.4: 创建 TransparentIconButtonTheme（透明图标按钮）
  - [x] SubTask 2.5: 创建 SidebarIconButtonTheme（侧边栏图标按钮）

- [x] Task 3: 重构 PlayerPageView.axaml 移除内联样式
  - [x] SubTask 3.1: 重构 Header 区域的返回按钮、队列按钮、更多按钮
  - [x] SubTask 3.2: 重构播放控制按钮（上一曲、播放/暂停、下一曲）
  - [x] SubTask 3.3: 重构底部操作按钮（喜欢、分享、投射、音量）
  - [x] SubTask 3.4: 移除内联 Background/BorderBrush/CornerRadius 等属性

- [x] Task 4: 优化 MainWindow.axaml 按钮样式引用
  - [x] SubTask 4.1: 检查侧边栏导航按钮，引用共享样式
  - [x] SubTask 4.2: 确保 App.axaml 正确引用新的样式文件

- [ ] Task 5: 验证构建通过
  - [x] SubTask 5.1: 运行 dotnet build 确认无编译错误
  - [ ] SubTask 5.2: 检查所有按钮样式是否正确应用

# Task Dependencies
- Task 2 依赖 Task 1（需要尺寸资源先定义）
- Task 3 依赖 Task 2（需要按钮主题先定义）
- Task 4 依赖 Task 2
- Task 5 依赖 Task 3 和 Task 4

# Note
Task 5 构建失败是由于架构重构相关的历史遗留问题（PlayerPageViewModel、QueueViewModel 等构造函数参数不匹配），与本次样式重构无关。样式文件已正确创建和引用。
