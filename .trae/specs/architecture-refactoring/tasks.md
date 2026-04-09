# 架构重构任务列表

## 任务 1: 创建播放状态服务

- [ ] 1.1 创建 `IPlaybackStateService` 接口
  - 定义播放状态属性（CurrentSong、IsPlaying、Position、Duration、Volume、IsMuted、PlaybackMode）
  - 定义播放控制方法（Play、Pause、Resume、Stop、Seek、Mute、PlayNext、PlayPrevious）
  - 定义事件（PlaybackEnded、PlaybackStateChanged、PositionChanged、CurrentSongChanged）

- [ ] 1.2 创建 `PlaybackStateService` 实现类
  - 实现 `IPlaybackStateService` 接口
  - 组合 `IPlaylistService` 和 `IMusicPlayerService`
  - 实现状态定时更新逻辑
  - 实现播放完成自动下一曲逻辑

- [ ] 1.3 在 `App.axaml.cs` 注册服务
  - `services.AddSingleton<IPlaybackStateService, PlaybackStateService>()`

## 任务 2: 创建导航服务

- [ ] 2.1 创建 `INavigationService` 接口
  - 定义 `CurrentPage` 属性（Type 类型）
  - 定义 `CurrentPageChanged` 事件
  - 定义 `NavigateTo<T>()` 泛型方法
  - 定义 `NavigateTo(page)` 方法
  - 定义 `CanGoBack` 和 `GoBack()` 方法

- [ ] 2.2 创建 `NavigationService` 实现类
  - 实现导航历史栈
  - 实现页面切换逻辑
  - 触发 `CurrentPageChanged` 事件

- [ ] 2.3 在 `App.axaml.cs` 注册服务
  - `services.AddSingleton<INavigationService, NavigationService>()`

## 任务 3: 创建 ViewModel 工厂

- [ ] 3.1 创建 `IViewModelFactory` 接口
  - 定义创建各 ViewModel 的方法（CreatePlayerPageViewModel、CreatePlaylistManagementViewModel、CreateSettingsViewModel、CreateStatisticsViewModel、CreateLibraryCategoryViewModel）

- [ ] 3.2 创建 `ViewModelFactory` 实现类
  - 注入所需的所有服务
  - 实现各 ViewModel 的创建逻辑

- [ ] 3.3 在 `App.axaml.cs` 注册服务
  - `services.AddSingleton<IViewModelFactory, ViewModelFactory>()`

## 任务 4: 重构 MainWindowViewModel

- [ ] 4.1 简化 MainWindowViewModel
  - 移除直接的播放状态属性（CurrentSong、Position、Volume 等）
  - 移除直接的播放控制命令
  - 移除直接创建子 ViewModel 的代码
  - 改为注入并使用服务

- [ ] 4.2 使用 IPlaybackStateService 获取播放状态
  - 替换直接的状态属性为服务属性绑定

- [ ] 4.3 使用 INavigationService 处理导航
  - 替换导航命令为服务调用

- [ ] 4.4 使用 IViewModelFactory 创建子 ViewModel
  - 替换 `new XxxViewModel()` 为 `viewModelFactory.CreateXxxViewModel()`

## 任务 5: 重构 PlayerPageViewModel

- [ ] 5.1 移除对 MainWindowViewModel 的依赖
  - 删除构造函数中的 `MainWindowViewModel` 参数
  - 替换 `ToggleQueuePanelCommand` 来源（通过服务或接口）

- [ ] 5.2 使用 IPlaybackStateService 获取播放状态
  - 重写属性绑定逻辑

- [ ] 5.3 使用 INavigationService 处理导航
  - 替换 `NavigateBackCommand` 使用导航服务

## 任务 6: 重构其他 ViewModel

- [ ] 6.1 重构 QueueViewModel
  - 改用 IPlaybackStateService 获取播放状态

- [ ] 6.2 重构 PlaylistManagementViewModel
  - 改用 IPlaybackStateService（如需要）

- [ ] 6.3 重构 LibraryCategoryViewModel
  - 改用 IPlaybackStateService（如需要）

## 任务 7: 更新 MainWindow.axaml.cs

- [ ] 7.1 移除 Loaded 事件中的 ViewModel 创建逻辑
  - 改为从 DI 容器获取预创建的 ViewModel

## 任务 8: 验证和测试

- [ ] 8.1 编译验证
  - 运行 `dotnet build` 确保无编译错误

- [ ] 8.2 功能验证
  - 播放/暂停/停止功能
  - 导航功能
  - 队列功能
  - 播放列表管理功能

- [ ] 8.3 回归测试
  - 确保原有功能未受影响

# 任务依赖关系

```
任务 1 (IPlaybackStateService) ──┐
                                 ├──> 任务 4 (MainWindowViewModel)
任务 2 (INavigationService) ────┤
                                 ├──> 任务 5 (PlayerPageViewModel)
任务 3 (IViewModelFactory) ─────┤
                                 ├──> 任务 6 (其他 ViewModel)
                                 │
任务 4 ─────────────────────────┼──> 任务 7 (MainWindow.axaml.cs)
                                 │
任务 5 ─────────────────────────┤
                                 │
任务 6 ─────────────────────────┴──> 任务 8 (验证测试)
```
