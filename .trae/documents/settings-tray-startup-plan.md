# 设置页面扩展计划：系统托盘/通知 + 启动行为

## 概述

为 SettingsView 新增两个设置卡片：
1. **系统托盘/通知** — 最小化到托盘开关、歌曲切换通知开关
2. **启动行为** — 开机自启开关、恢复上次播放开关

## 现状分析

| 组件 | 当前状态 |
|------|----------|
| `SystemTrayService` | 已存在但硬编码始终最小化到托盘 |
| `UpdateTrayIcon()` | 空方法，未实现 |
| `ShowNotification()` | 空方法，未实现 |
| `MainWindow.CloseButton_Click` | 始终调用 `Hide()`（无配置） |
| `AppSettings` | 无相关字段 |
| `ConfigurationService` | 无相关字段读写 |

## 实施步骤

### 步骤 1：扩展 AppSettings 模型
**文件**: `Models/AppSettings.cs`

新增字段：
```csharp
public bool MinimizeToTray { get; set; } = true;
public bool ShowSongChangeNotification { get; set; } = true;
public bool AutoStartOnBoot { get; set; }
public bool ResumeLastPlayback { get; set; } = true;
```

### 步骤 2：扩展 ConfigurationService
**文件**: `Services/System/ConfigurationService.cs`

在 `LoadSettingsAsync()` 中加载 4 个新字段
在 `SaveSettingsAsync()` 中保存 4 个新字段

### 步骤 3：创建 IAutoStartService 接口和实现
**新建文件**: `Services/System/IAutoStartService.cs`
**新建文件**: `Services/System/AutoStartService.cs`

接口方法：
- `Task SetAutoStartAsync(bool enabled)` — 写入/删除注册表 `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`
- `bool IsAutoStartEnabled()` — 检查注册表是否存在对应项

实现使用 `Microsoft.Win32.Registry` 操作 Windows 注册表。

### 步骤 4：改造 SystemTrayService
**文件**: `Services/System/SystemTrayService.cs`

改动：
- 构造函数注入 `IConfigurationService`
- `Initialize()` 中根据 `MinimizeToTray` 决定是否拦截 Closing 事件
- 实现 `UpdateTrayIcon(bool isPlaying)` — 切换图标或 ToolTipText
- 实现 `ShowNotification(string title, string message)` — 使用 Avalonia 的 TrayIcon.ShowNotification

### 步骤 5：修改 MainWindow 关闭行为
**文件**: `Views/Main/MainWindow.axaml.cs`

- 注入 `IConfigurationService`
- `CloseButton_Click` 根据 `MinimizeToTray` 决定 Hide() 还是 Close()

### 步骤 6：修改 MainWindowViewModel 通知调用
**文件**: `ViewModels/MainWindowViewModel.cs`

- `OnCurrentSongChanged` 中根据 `ShowSongChangeNotification` 决定是否调用 `_systemTrayService.ShowNotification()`

### 步骤 7：App.axaml.cs 集成恢复播放逻辑
**文件**: `App.axaml.cs`

- 在 MainWindow Loaded 回调中检查 `ResumeLastPlayback`
- 若启用则从 `AppSettings.LastSongFilePath/QueueFilePaths/LastPlaybackPosition` 恢复播放状态

### 步骤 8：注册 AutoStartService 到 DI
**文件**: `Helpers/ServiceCollectionExtensions.cs`

在 `AddSystemServices()` 中添加：
```csharp
services.AddSingleton<IAutoStartService, AutoStartService>();
```

### 步骤 9：扩展 SettingsViewModel
**文件**: `ViewModels/SettingsViewModel.cs`

新增属性：
```csharp
[ObservableProperty] private bool _minimizeToTray = true;
[ObservableProperty] private bool _showSongChangeNotification = true;
[ObservableProperty] private bool _autoStartOnBoot;
[ObservableProperty] private bool _resumeLastPlayback = true;
```

新增 OnXxxChanged partial 方法自动保存设置。

### 步骤 10：添加字符串资源
**文件**: `Styles/Strings.axaml`

新增键值对：
- `String_SystemTray` → "系统托盘"
- `String_SystemTraySettings` → "最小化和通知设置"
- `String_MinimizeToTray` → "关闭时最小化到系统托盘"
- `String_SongChangeNotification` → "歌曲切换时显示通知"
- `String_StartupBehavior` → "启动行为"
- `String_StartupBehaviorSettings` → "应用程序启动选项"
- `String_AutoStartOnBoot` → "开机时自动启动"
- `String_ResumeLastPlayback` → "启动时恢复上次播放"

### 步骤 11：扩展 SettingsView.axaml UI
**文件**: `Views/Settings/SettingsView.axaml`

在「外观」卡片之前插入两个新卡片：

#### 卡片 A：系统托盘
- 图标: &#xE790; (与外观同图标可换 &#xE962;)
- 标题: 系统托盘
- 副标题: 最小化和通知设置
- 内容:
  - ToggleSwitch: 关闭时最小化到系统托盘 (绑定 MinimizeToTray)
  - ToggleSwitch: 歌曲切换时显示通知 (绑定 ShowSongChangeNotification)

#### 卡片 B：启动行为
- 图标: &#xE8B5;
- 标题: 启动行为
- 副标题: 应用程序启动选项
- 内容:
  - ToggleSwitch: 开机时自动启动 (绑定 AutoStartOnBoot)
  - ToggleSwitch: 启动时恢复上次播放 (绑定 ResumeLastPlayback)

## 文件变更清单

| 文件 | 操作 |
|------|------|
| `Models/AppSettings.cs` | 修改 — 新增 4 个属性 |
| `Services/System/IConfigurationService.cs` | 无需改（通过 CurrentSettings 暴露） |
| `Services/System/ConfigurationService.cs` | 修改 — Load/Save 新增字段 |
| `Services/System/IAutoStartService.cs` | **新建** |
| `Services/System/AutoStartService.cs` | **新建** |
| `Services/System/ISystemTrayService.cs` | 无需改 |
| `Services/System/SystemTrayService.cs` | 修改 — 条件化托盘、实现空方法 |
| `Views/Main/MainWindow.axaml.cs` | 修改 — 关闭行为条件化 |
| `ViewModels/MainWindowViewModel.cs` | 修改 — 通知条件化 |
| `App.axaml.cs` | 修改 — 注册恢复播放逻辑 |
| `Helpers/ServiceCollectionExtensions.cs` | 修改 — 注册 AutoStartService |
| `ViewModels/SettingsViewModel.cs` | 修改 — 新增 4 个属性+Changed方法 |
| `Styles/Strings.axaml` | 修改 — 新增 8 个字符串资源 |
| `Views/Settings/SettingsView.axaml` | 修改 — 新增 2 个卡片 |

## 注意事项

- 不添加注释（遵循项目规范）
- 使用 x:DataType 编译期绑定
- 字符串资源不硬编码中文
- 异步方法以 Async 结尾，返回 Task
- 命名遵循 PascalCase/_camelCase 规范
