# LocalMusicPlayer CommunityToolkit.Mvvm 重构规格说明

## Why

当前 LocalMusicPlayer 使用 ReactiveUI.Avalonia 作为 MVVM 框架，存在以下问题：

1. **学习曲线陡峭** - ReactiveUI 的 Reactive Extensions 概念复杂
2. **代码冗长** - 需要手动调用 `RaiseAndSetIfChanged`、创建命令等
3. **编译速度慢** - ReactiveUI.SourceGenerators 生成大量代码
4. **调试困难** - 生成的代码难以追踪
5. **CommunityToolkit.Mvvm 官方推荐** - .NET 官方推荐的 MVVM 框架，更现代、更轻量

## What Changes

### 依赖变更

**移除**：
- `ReactiveUI.Avalonia` (11.3.8)
- `ReactiveUI.SourceGenerators` (2.4.1)

**新增**：
- `CommunityToolkit.Mvvm` (8.3.2) - 官方 MVVM 工具包

### 框架核心变更

| 变更项 | ReactiveUI | CommunityToolkit.Mvvm |
|--------|-----------|----------------------|
| 基类 | `ReactiveObject` | `ObservableObject` |
| 属性 | 手动 `RaiseAndSetIfChanged` | `[ObservableProperty]` 自动生成 |
| 命令 | `ReactiveCommand` | `[RelayCommand]` 自动生成 |
| 集合 | `ObservableCollection` | `ObservableCollection` (不变) |
| 事件 | `WhenAnyValue` + `Subscribe` | `[NotifyPropertyChangedFor]` 或直接绑定 |
| 定时器 | `Observable.Interval` | `DispatcherTimer` 或 `Task.Delay` |

### 重构现有组件

#### ViewModelBase

**重构前**：
```csharp
public class ViewModelBase : ReactiveObject
{
}
```

**重构后**：
```csharp
public abstract partial class ViewModelBase : ObservableObject
{
}
```

#### MainWindowViewModel

**重构前**：约 325 行，使用 ReactiveUI 命令和属性

**重构后**：
- 使用 `[ObservableProperty]` 自动生成属性
- 使用 `[RelayCommand]` 自动生成命令
- 使用 `[NotifyPropertyChangedFor]` 自动通知依赖属性
- 约 200 行

#### PlayerPageViewModel

**重构前**：使用 `WhenAnyValue` 订阅状态变化

**重构后**：
- 使用 `[NotifyPropertyChangedFor]` 声明依赖属性
- 使用 `DispatcherTimer` 替代 `Observable.Interval`

#### QueueViewModel

**重构前**：使用 `ReactiveCommand` 和事件同步

**重构后**：
- 使用 `[RelayCommand]` 生成命令
- 使用 `partial class` + Source Generator

### 新增接口/类

无新增接口，纯框架迁移。

### 移除接口/类

无移除接口，纯实现变更。

## Impact

### 影响的规格

- `architecture-refactoring` - 服务层架构不变，仅 ViewModel 层迁移
- `core-features` - 功能不变，仅实现方式变更
- `music-library-management` - 功能不变

### 影响的代码

| 文件 | 变化类型 | 说明 |
|------|---------|------|
| ViewModels/ViewModelBase.cs | 重构 | 基类从 ReactiveObject 改为 ObservableObject |
| ViewModels/MainWindowViewModel.cs | 重构 | 使用 MVVM Toolkit 特性 |
| ViewModels/PlayerPageViewModel.cs | 重构 | 使用 MVVM Toolkit 特性 |
| ViewModels/QueueViewModel.cs | 重构 | 使用 MVVM Toolkit 特性 |
| ViewModels/PlaylistManagementViewModel.cs | 重构 | 使用 MVVM Toolkit 特性 |
| ViewModels/LibraryCategoryViewModel.cs | 重构 | 使用 MVVM Toolkit 特性 |
| ViewModels/StatisticsViewModel.cs | 重构 | 使用 MVVM Toolkit 特性 |
| ViewModels/SettingsViewModel.cs | 重构 | 使用 MVVM Toolkit 特性 |
| ViewModels/MetadataEditorViewModel.cs | 重构 | 使用 MVVM Toolkit 特性 |
| LocalMusicPlayer.csproj | 修改 | 替换包依赖 |
| Services/*.cs | 无变化 | 服务层保持不变 |

## ADDED Requirements

### Requirement: CommunityToolkit.Mvvm 依赖

系统 SHALL 使用 CommunityToolkit.Mvvm 8.3.2 作为唯一的 MVVM 框架。

#### Scenario: 添加新包
- **WHEN** 需要添加 CommunityToolkit.Mvvm
- **THEN** 使用 `dotnet add package CommunityToolkit.Mvvm --version 8.3.2`

### Requirement: ViewModel 基类

系统 SHALL 使用 `ObservableObject` 作为所有 ViewModel 的基类。

#### Scenario: 创建 ViewModel
- **WHEN** 定义新的 ViewModel 类
- **THEN** 继承 `ObservableObject`
- **AND** 使用 `partial class` 修饰符

### Requirement: ObservableProperty 特性

系统 SHALL 使用 `[ObservableProperty]` 特性自动生成属性代码。

#### Scenario: 定义可观察属性
- **WHEN** 需要定义 `string Name` 属性
- **THEN** 声明 `private string _name;`
- **AND** 添加 `[ObservableProperty]` 特性
- **AND** 属性通过 `Name` 自动生成

### Requirement: RelayCommand 特性

系统 SHALL 使用 `[RelayCommand]` 特性自动生成命令。

#### Scenario: 定义命令
- **WHEN** 需要定义播放命令
- **THEN** 声明 `public async Task PlayAsync()`
- **AND** 添加 `[RelayCommand]` 特性
- **AND** `PlayCommand` 自动生成

### Requirement: NotifyPropertyChangedFor 特性

系统 SHALL 使用 `[NotifyPropertyChangedFor]` 声明属性依赖关系。

#### Scenario: 定义计算属性
- **WHEN** `FullName` 依赖 `FirstName` 和 `LastName`
- **THEN** 在 `FullName` 上添加 `[NotifyPropertyChangedFor(nameof(FirstName), nameof(LastName))]`

## MODIFIED Requirements

### Requirement: 定时器实现

**重构前**：
```csharp
Observable.Interval(TimeSpan.FromMilliseconds(250))
    .ObserveOn(RxApp.MainThreadScheduler)
    .Subscribe(_ => { /* 更新逻辑 */ });
```

**重构后**：
```csharp
private DispatcherTimer _positionTimer;

private void StartPositionTimer()
{
    _positionTimer = new DispatcherTimer
    {
        Interval = TimeSpan.FromMilliseconds(250)
    };
    _positionTimer.Tick += (_, _) => { /* 更新逻辑 */ };
    _positionTimer.Start();
}
```

### Requirement: 异步命令执行

**重构前**：
```csharp
PlayCommand = ReactiveCommand.CreateFromTask(PlayAsync);
```

**重构后**：
```csharp
[RelayCommand]
private async Task PlayAsync()
{
    await _playbackStateService.PlayAsync();
}
```

### Requirement: 属性变更通知

**重构前**：
```csharp
_playbackStateService.PlaybackStateChanged += (_, _) =>
{
    this.RaisePropertyChanged(nameof(IsPlaying));
};
```

**重构后**：
```csharp
_playbackStateService.PlaybackStateChanged += (_, _) =>
{
    OnPropertyChanged(nameof(IsPlaying));
};
```

## REMOVED Requirements

### Requirement: ReactiveUI 依赖

**移除原因**：迁移到 CommunityToolkit.Mvvm

**迁移**：
- 删除 `ReactiveUI.Avalonia` 包引用
- 删除 `ReactiveUI.SourceGenerators` 包引用
- 重写所有 ViewModel

### Requirement: WhenAnyValue 模式

**移除原因**：CommunityToolkit.Mvvm 使用不同的属性依赖声明方式

**迁移**：
- 使用 `[NotifyPropertyChangedFor]` 替代 `WhenAnyValue` 订阅
- 直接绑定替代复杂的 `WhenAnyValue` 链

### Requirement: RaiseAndSetIfChanged 模式

**移除原因**：Source Generator 自动生成

**迁移**：
- 使用 `[ObservableProperty]` 自动生成 `RaiseAndSetIfChanged` 调用

## 技术实现约束

### 命名约束

1. **私有字段** - 使用 `_camelCase` 下划线前缀
2. **公共属性** - 使用 `PascalCase`，Source Generator 生成
3. **命令属性** - 使用 `PascalCase` + `Command` 后缀

### 特性使用约束

1. **partial class** - 所有 ViewModel 必须使用 partial 修饰符
2. **[ObservableProperty]** - 只用于私有字段，不用于属性
3. **[RelayCommand]** - 方法必须 private/ internal，返回 Task 或 void
4. **[NotifyPropertyChangedFor]** - 依赖属性必须是生成的属性名

### 线程处理

1. **UI 线程** - 使用 `DispatcherTimer` 确保主线程执行
2. **异步操作** - 使用 `async/await`，避免 `.Wait()` 或 `.Result()`

### 服务注册

无需变更，服务层保持不变。

## 重构步骤概要

1. 更新 `LocalMusicPlayer.csproj` 依赖
2. 重构 `ViewModelBase` 基类
3. 重构 `MainWindowViewModel`
4. 重构 `PlayerPageViewModel`
5. 重构 `QueueViewModel`
6. 重构其他 ViewModel（PlaylistManagementViewModel、SettingsViewModel 等）
7. 验证编译和功能
