# LocalMusicPlayer 架构重构规格说明

## Why

当前 LocalMusicPlayer 项目存在以下架构问题：

1. **ViewModel 耦合严重** - `PlayerPageViewModel` 直接依赖 `MainWindowViewModel`，违反 MVVM 原则
2. **ViewModel 职责过多** - `MainWindowViewModel` 约 500 行，包含导航、播放、搜索、统计等多种职责
3. **违反依赖注入** - `MainWindowViewModel` 内部直接 `new` 创建其他 ViewModel
4. **状态重复定义** - `CurrentSong`、`Position`、`Volume` 等状态在多个 ViewModel 中重复定义
5. **状态同步复杂** - 通过 `PropertyChanged` 事件手动同步状态，容易出错

## What Changes

### 新增服务

#### IPlaybackStateService - 播放状态服务

统一管理所有播放相关状态，消除状态重复定义：

| 属性/方法 | 类型 | 说明 |
|-----------|------|------|
| `CurrentSong` | `Song?` | 当前播放歌曲 |
| `IsPlaying` | `bool` | 是否正在播放 |
| `Position` | `TimeSpan` | 当前播放位置 |
| `Duration` | `TimeSpan` | 当前歌曲时长 |
| `Volume` | `int` | 音量 (0-100) |
| `IsMuted` | `bool` | 是否静音 |
| `PlaybackMode` | `PlaybackMode` | 播放模式 |
| `PlaybackEnded` | `event` | 播放完成事件 |
| `Play(Song)` | `void` | 播放指定歌曲 |
| `Pause()` | `void` | 暂停 |
| `Resume()` | `void` | 继续播放 |
| `Stop()` | `void` | 停止 |
| `Seek(TimeSpan)` | `void` | 跳转到位置 |
| `SetVolume(int)` | `void` | 设置音量 |
| `Mute()` | `void` | 静音切换 |
| `PlayNext()` | `bool` | 播放下一曲 |
| `PlayPrevious()` | `bool` | 播放上一曲 |

#### INavigationService - 导航服务

统一管理页面导航，解耦 ViewModel 之间的导航依赖：

| 属性/方法 | 类型 | 说明 |
|-----------|------|------|
| `CurrentPage` | `Type` | 当前页面类型 |
| `CurrentPageChanged` | `event` | 页面变化事件 |
| `NavigateTo<T>()` | `void` | 导航到指定页面 |
| `NavigateTo(page)` | `void` | 导航到指定页面实例 |
| `CanGoBack` | `bool` | 是否可以返回 |
| `GoBack()` | `void` | 返回上一页 |

### 新增接口

#### IViewModelFactory - ViewModel 工厂接口

解决 `MainWindowViewModel` 内部直接 `new` 创建 ViewModel 的问题：

| 方法 | 说明 |
|------|------|
| `CreatePlayerPageViewModel()` | 创建 PlayerPageViewModel |
| `CreatePlaylistManagementViewModel()` | 创建 PlaylistManagementViewModel |
| `CreateSettingsViewModel()` | 创建 SettingsViewModel |
| `CreateStatisticsViewModel()` | 创建 StatisticsViewModel |
| `CreateLibraryCategoryViewModel()` | 创建 LibraryCategoryViewModel |

### 重构现有组件

#### MainWindowViewModel 简化

**重构前**：约 500 行，包含导航、播放、搜索、统计、ViewModel 创建等职责

**重构后**：约 150 行，职责：
- 管理布局状态（侧边栏宽度、页面可见性）
- 持有导航服务
- 持有播放器状态服务
- 协调子 ViewModel 的生命周期

#### PlayerPageViewModel 解耦

**重构前**：依赖 `MainWindowViewModel` 获取 `ToggleQueuePanelCommand`

**重构后**：
- 通过 `IPlaybackStateService` 获取播放状态
- 通过 `INavigationService` 处理导航
- 无需依赖 `MainWindowViewModel`

#### QueueViewModel 状态同步

**重构前**：通过事件与 MainWindowViewModel 同步

**重构后**：通过 `IPlaybackStateService` 共享状态

## Impact

### 影响的规格

- `core-features` - 音频播放服务接口需扩展状态管理能力
- `music-library-management` - 播放列表管理依赖新的状态服务

### 影响的代码

| 文件 | 变化类型 | 说明 |
|------|---------|------|
| Services/ | 新增 | IPlaybackStateService、INavigationService、IViewModelFactory |
| ViewModels/MainWindowViewModel.cs | 重构 | 简化至约 150 行 |
| ViewModels/PlayerPageViewModel.cs | 重构 | 解耦，移除对 MainWindowViewModel 的依赖 |
| ViewModels/QueueViewModel.cs | 重构 | 通过服务共享状态 |
| App.axaml.cs | 修改 | 注册新服务 |

## ADDED Requirements

### Requirement: 播放状态服务

系统 SHALL 通过 `IPlaybackStateService` 提供统一的播放状态管理。

#### Scenario: 播放歌曲
- **WHEN** 调用 `IPlaybackStateService.Play(song)`
- **THEN** `CurrentSong` 属性更新为指定歌曲
- **AND** `IsPlaying` 属性变为 `true`
- **AND** `Position` 从头开始

#### Scenario: 多客户端状态同步
- **WHEN** `IPlaybackStateService` 的任意状态发生变化
- **THEN** 所有订阅 `WhenAnyValue` 的消费者同时收到更新
- **AND** 无需手动通过 `PropertyChanged` 事件同步

#### Scenario: 播放完成自动下一曲
- **WHEN** 当前歌曲播放完毕
- **THEN** `PlaybackEnded` 事件被触发
- **AND** 服务内部自动调用 `PlayNext()`
- **AND** `CurrentSong` 更新为下一首歌曲

### Requirement: 导航服务

系统 SHALL 通过 `INavigationService` 提供统一的页面导航。

#### Scenario: 导航到播放器页面
- **WHEN** 调用 `INavigationService.NavigateTo<PlayerPageViewModel>()`
- **THEN** `CurrentPage` 属性更新
- **AND** `CurrentPageChanged` 事件被触发
- **AND** UI 自动切换到对应页面

#### Scenario: ViewModel 无法直接导航
- **WHEN** 任意 ViewModel 需要执行导航
- **THEN** 该 ViewModel 注入 `INavigationService`
- **AND** 不再直接操作其他 ViewModel 或其属性

### Requirement: ViewModel 工厂

系统 SHALL 通过 `IViewModelFactory` 统一管理 ViewModel 的创建。

#### Scenario: 创建 PlayerPageViewModel
- **WHEN** 调用 `IViewModelFactory.CreatePlayerPageViewModel()`
- **THEN** 返回正确配置的 `PlayerPageViewModel` 实例
- **AND** 所有依赖通过构造函数注入

## MODIFIED Requirements

### Requirement: MainWindowViewModel 简化

**重构前**：
- 直接创建所有子 ViewModel（SettingsViewModel、PlayerPageViewModel 等）
- 持有大量播放状态属性
- 处理播放命令逻辑

**重构后**：
- 通过 `IViewModelFactory` 创建子 ViewModel
- 通过 `IPlaybackStateService` 访问播放状态
- 通过 `INavigationService` 处理导航
- 只负责布局和页面切换

### Requirement: PlayerPageViewModel 解耦

**重构前**：
- 直接依赖 `MainWindowViewModel` 获取 `ToggleQueuePanelCommand`
- 直接依赖 `MainWindowViewModel` 的属性

**重构后**：
- 独立于 `MainWindowViewModel`
- 所有状态来自 `IPlaybackStateService`
- 导航通过 `INavigationService`
- 命令通过工厂或服务获取

### Requirement: PlaylistService 集成

**重构后**：
- 实现 `IPlaybackStateService` 接口
- 内部委托给 `IPlaylistService` 和 `IMusicPlayerService`
- 统一封装播放状态和播放控制

## REMOVED Requirements

### Requirement: ViewModel 间直接依赖

**移除原因**：`PlayerPageViewModel` 不应直接依赖 `MainWindowViewModel`

**迁移**：
- 使用 `IPlaybackStateService` 替代直接属性访问
- 使用 `INavigationService` 替代命令调用

### Requirement: MainWindowViewModel 中的播放逻辑

**移除原因**：播放逻辑应集中在 `IPlaybackStateService`

**迁移**：
- 播放控制命令移到服务层
- 定时更新状态逻辑移到服务层
- 事件处理逻辑移到服务层

## 技术实现约束

### 依赖注入

所有新增服务通过接口注入：
- `IPlaybackStateService` 注册为 Singleton
- `INavigationService` 注册为 Singleton
- `IViewModelFactory` 注册为 Singleton

### 线程安全

- 播放状态更新在 UI 线程执行
- 使用 `ObserveOn(RxApp.MainThreadScheduler)` 确保线程安全

### 服务注册

```csharp
// App.axaml.cs
private static void ConfigureServices(IServiceCollection services)
{
    // 现有服务...
    services.AddSingleton<IPlaybackStateService, PlaybackStateService>();
    services.AddSingleton<INavigationService, NavigationService>();
    services.AddSingleton<IViewModelFactory, ViewModelFactory>();
}
```

### ViewModel 构造函数

```csharp
// PlayerPageViewModel 新的构造函数
public PlayerPageViewModel(
    IPlaybackStateService playbackState,
    INavigationService navigation,
    IViewModelFactory viewModelFactory)
{
    // 所有依赖通过服务获取
}
```

## 重构步骤概要

1. 创建 `IPlaybackStateService` 接口和实现
2. 创建 `INavigationService` 接口和实现
3. 创建 `IViewModelFactory` 接口和实现
4. 重构 `PlaylistService` 实现 `IPlaybackStateService`
5. 重构 `MainWindowViewModel`，简化并使用新服务
6. 重构 `PlayerPageViewModel`，移除对 `MainWindowViewModel` 的依赖
7. 重构其他依赖 `MainWindowViewModel` 的 ViewModel
8. 更新 `App.axaml.cs` 注册新服务
9. 验证所有功能正常工作
