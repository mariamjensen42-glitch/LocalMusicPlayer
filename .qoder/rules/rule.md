---
trigger: always_on
---

# LocalMusicPlayer 项目规则

## 项目概述

LocalMusicPlayer 是一个基于 Avalonia UI 的跨平台本地音乐播放器，采用 MVVM 架构模式，使用 ReactiveUI 进行响应式编程。

## 技术栈

- **.NET**: 9.0
- **Avalonia**: 11.3.8
- **ReactiveUI.Avalonia**: 11.3.8
- **ReactiveUI.SourceGenerators**: 2.4.1
- **TagLibSharp**: 2.3.0
- **LibVLCSharp**: 3.8.1
- **VideoLAN.LibVLC.Windows**: 3.0.21
- **Microsoft.Extensions.DependencyInjection**: 9.0.0

## 项目结构

```
LocalMusicPlayer/
├── Models/           # 数据模型 (Song, Playlist, PlayState, etc.)
├── Services/         # 业务服务接口与实现
├── ViewModels/       # MVVM 视图模型 (继承 ReactiveObject)
├── Views/            # Avalonia 视图 (XAML)
├── Converters/       # XAML 值转换器
├── Helpers/          # 工具类
├── Styles/           # 共享样式资源 (Resources.axaml)
├── Behaviors/        # Avalonia 行为
└── Assets/           # 静态资源
```

## 架构规范

### MVVM 模式
- ViewModel 必须继承 `ReactiveObject`
- View 通过 `DataContext` 绑定到 ViewModel
- 使用 `ViewLocator` 自动映射 ViewModel 到 View

### 依赖注入
- 服务在 `App.axaml.cs` 的 `ConfigureServices` 中注册
- 优先使用 Singleton 生命周期
- ViewModel 使用 Transient 生命周期

### 服务层设计
- 所有服务必须定义接口，放在 `Services/` 目录
- 接口命名以 `I` 前缀开头 (如 `IMusicPlayerService`)
- 实现类与接口放在同一目录

## 编码规范

### 命名约定
- **类型**: PascalCase (如 `MainWindowViewModel`, `MusicPlayerService`)
- **接口**: 以大写 `I` 前缀 (如 `IMusicPlayerService`)
- **属性与字段**: PascalCase；私有字段以下划线前缀 (如 `_currentSong`)
- **命令**: 以 `Command` 结尾 (如 `PlayCommand`, `NextCommand`)
- **文件**: 与内部主类型同名

### XAML 规范
- 使用 `x:DataType` 启用编译期绑定
- 避免使用字符串路径的 `Binding`
- 共享样式置于 `Styles/Resources.axaml`，通过 `MergedDictionaries` 引用
- 不在 XAML 中硬编码中文字符串

### C# 代码规范
- 启用 `Nullable` 可空引用类型
- 使用 `var` 进行隐式类型声明
- 优先使用表达式体成员
- 保持代码简洁，避免冗余注释

## 响应式编程

### ReactiveUI 使用
- 使用 `ObservableAsPropertyHelper<T>` 创建只读响应式属性
- 使用 `ReactiveCommand` 创建命令
- 使用 `WhenAnyValue` 监听属性变化
- 在 `Dispose` 中清理订阅

### 事件命名
- 使用动词短语 (如 `PlaybackEnded`, `PositionChanged`)
- 事件处理程序使用 `On` 前缀

## 禁止事项

- 禁止在 ViewModel 中直接操作 UI 控件
- 禁止使用字符串路径的 Binding（应使用 `x:DataType` 编译绑定）
- 禁止在 XAML 中硬编码中文字符串
- 禁止在服务层直接调用 UI 相关 API

## Git 提交规范

使用 Conventional Commits 规范：

```
feat: 添加新功能
docs: 更新文档
style: 代码格式调整
refactor: 代码重构
test: 添加测试
chore: 构建/工具变更
```

## 调试与开发

- 使用 `Avalonia.LogToTrace` 输出日志
- 在命令执行链路、服务调用前后设置断点
- Release 配置自动排除 `Avalonia.Diagnostics` 包
