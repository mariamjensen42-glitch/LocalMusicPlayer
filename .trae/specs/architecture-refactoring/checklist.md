# 架构重构检查清单

## 第一阶段：播放状态服务

- [ ] `IPlaybackStateService` 接口定义完整（属性、方法、事件）
- [ ] `PlaybackStateService` 正确实现接口
- [ ] 播放状态（CurrentSong、IsPlaying、Position 等）统一管理
- [ ] 播放完成自动下一曲逻辑实现
- [ ] 服务在 DI 容器中正确注册

## 第二阶段：导航服务

- [ ] `INavigationService` 接口定义完整
- [ ] `NavigationService` 实现导航历史栈
- [ ] 页面切换正确触发 `CurrentPageChanged` 事件
- [ ] 服务在 DI 容器中正确注册

## 第三阶段：ViewModel 工厂

- [ ] `IViewModelFactory` 接口定义完整
- [ ] `ViewModelFactory` 正确创建所有 ViewModel
- [ ] 所有依赖通过构造函数注入
- [ ] 服务在 DI 容器中正确注册

## 第四阶段：MainWindowViewModel 重构

- [ ] 移除了直接的播放状态属性
- [ ] 移除了直接的播放控制命令
- [ ] 使用 `IPlaybackStateService` 获取播放状态
- [ ] 使用 `INavigationService` 处理导航
- [ ] 使用 `IViewModelFactory` 创建子 ViewModel
- [ ] 代码行数简化至约 150 行

## 第五阶段：PlayerPageViewModel 重构

- [ ] 移除了对 `MainWindowViewModel` 的依赖
- [ ] `ToggleQueuePanelCommand` 通过其他方式获取
- [ ] 使用 `IPlaybackStateService` 获取播放状态
- [ ] 使用 `INavigationService` 处理导航

## 第六阶段：其他 ViewModel 重构

- [ ] QueueViewModel 使用 `IPlaybackStateService`
- [ ] PlaylistManagementViewModel 使用 `IPlaybackStateService`
- [ ] LibraryCategoryViewModel 使用 `IPlaybackStateService`

## 第七阶段：MainWindow.axaml.cs

- [ ] 移除 Loaded 事件中的 ViewModel 创建逻辑
- [ ] 从 DI 容器获取预创建的 ViewModel

## 第八阶段：编译和功能验证

- [ ] `dotnet build` 编译成功
- [ ] 播放/暂停/停止功能正常
- [ ] 导航功能正常
- [ ] 队列功能正常
- [ ] 播放列表管理功能正常
- [ ] 原有功能未受影响

## 代码质量检查

- [ ] 所有服务接口命名符合规范（`I` 前缀）
- [ ] 服务实现类命名符合规范
- [ ] 无字符串硬编码
- [ ] 无 TODO 注释遗留
- [ ] 异常处理完整
- [ ] 符合异步编程规范（async/await）
