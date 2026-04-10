# Bug 修复记录

## 2026-04-10: 首页内容完全空白

**问题**: 首页（PlayerView）完全空白，不显示任何内容（标题、歌曲列表、播放控制栏均不可见）

**根因**: `PlayerView.axaml` 引用了 `{StaticResource CountToBoolConverter}`，但该转换器未在 `App.axaml` 全局资源中注册，导致 Avalonia XAML 解析失败，整个 PlayerView 无法渲染。

**修复**:
1. **[App.axaml](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/App.axaml)**: 在全局资源中注册 `CountToBoolConverter`
2. **[MainWindowViewModel.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/ViewModels/MainWindowViewModel.cs#L425)**: 在 `Library.Songs.CollectionChanged` 事件中添加 `FilterSongs()` 调用，确保扫描完成后歌曲列表正确填充

## 2026-04-10: 系统托盘图标不可见

**问题**: 关闭窗口后系统托盘看不到应用图标，无法恢复窗口

**根因**: `SystemTrayService` 只创建了 `WindowIcon`（图标数据），但没有创建 `TrayIcon` 控件（实际的系统托盘图标），所以托盘区域没有显示任何东西

**修复**:
1. **[SystemTrayService.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/Services/SystemTrayService.cs)**: 创建 `TrayIcon` 控件并设置 Icon、ToolTipText、Menu 属性
2. 添加 `NativeMenuItem` 右键菜单（显示主窗口、退出）
3. 添加 `TrayIcon.Clicked` 事件，单击托盘图标恢复窗口
4. 修复退出逻辑，确保点"退出"时能正常关闭应用
