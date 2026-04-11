# Tasks

- [x] Task 1: Services 层代码审查
  - [x] SubTask 1.1: 审查 DI 配置（App.axaml.cs），检查服务生命周期是否合理，有无 Captive Dependency
  - [x] SubTask 1.2: 审查核心播放服务（MusicPlayerService），检查 LibVLCSharp 资源管理、异步模式、线程安全
  - [x] SubTask 1.3: 审查数据访问服务（DatabaseService, AppDbContext），检查 EF Core 使用模式、异步查询
  - [x] SubTask 1.4: 审查文件系统服务（FileScannerService, FileWatcherService, FileManagerService），检查异步模式、资源释放
  - [x] SubTask 1.5: 审查其余服务，检查接口隔离、异常处理、命名规范

- [x] Task 2: ViewModels 层代码审查
  - [x] SubTask 2.1: 审查 ViewModelBase 和基类模式，检查 CommunityToolkit.Mvvm 使用是否规范
  - [x] SubTask 2.2: 审查 MainWindowViewModel，检查导航逻辑、服务依赖、响应式模式
  - [x] SubTask 2.3: 审查 PlayerPageViewModel，检查播放状态管理、事件订阅泄漏
  - [x] SubTask 2.4: 审查 LibraryBrowserViewModel 和 QueueViewModel，检查集合操作性能
  - [x] SubTask 2.5: 审查其余 ViewModel，检查 MVVM 合规性、命名规范

- [x] Task 3: Views/XAML 层代码审查
  - [x] SubTask 3.1: 审查所有 XAML 文件，检查 x:DataType 编译绑定使用情况
  - [x] SubTask 3.2: 审查 Resources.axaml 和 Strings.axaml，检查样式统一性和命名规范
  - [x] SubTask 3.3: 审查 View code-behind，检查是否有不应存在的逻辑代码

- [x] Task 4: Models 层代码审查
  - [x] SubTask 4.1: 审查 EF Core 实体模型（Song, Playlist 等），检查关系配置和索引
  - [x] SubTask 4.2: 审查 AppDbContext，检查查询模式和迁移管理

- [x] Task 5: Converters/Behaviors 层代码审查
  - [x] SubTask 5.1: 审查所有 Converters，检查 IValueConverter 实现规范性和性能
  - [x] SubTask 5.2: 审查所有 Behaviors，检查 Avalonia.Xaml.Behaviors 使用模式

- [x] Task 6: 汇总审查报告
  - [x] SubTask 6.1: 按优先级（Critical/Important/Minor）整理所有发现的问题
  - [x] SubTask 6.2: 为每个问题提供修复建议

# Task Dependencies
- [Task 6] depends on [Task 1, Task 2, Task 3, Task 4, Task 5]
- Task 1-5 可并行执行
