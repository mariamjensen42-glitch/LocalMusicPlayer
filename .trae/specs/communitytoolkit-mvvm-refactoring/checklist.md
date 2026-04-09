# CommunityToolkit.Mvvm 重构检查清单

## 第一阶段：项目依赖

- [x] 移除 `ReactiveUI.Avalonia` 包引用
- [x] 移除 `ReactiveUI.SourceGenerators` 包引用
- [x] 添加 `CommunityToolkit.Mvvm` 8.3.2 包引用
- [x] `dotnet restore` 成功

## 第二阶段：ViewModelBase 重构

- [x] ViewModelBase 继承 `ObservableObject`
- [x] ViewModelBase 使用 `partial` 修饰符
- [x] 移除 `using ReactiveUI;`
- [x] 所有继承 ViewModelBase 的类仍能编译

## 第三阶段：MainWindowViewModel 重构

- [x] 所有属性使用 `[ObservableProperty]` 特性
- [x] 所有命令使用 `[RelayCommand]` 特性
- [x] `Observable.Interval` 替换为 `DispatcherTimer`
- [x] `WhenAnyValue` 订阅已移除或替换
- [x] `RaisePropertyChanged` 替换为 `OnPropertyChanged`
- [x] 移除 `using ReactiveUI;`
- [x] 移除 `using System.Reactive.Linq;`
- [x] 编译无错误

## 第四阶段：PlayerPageViewModel 重构

- [x] 所有属性使用 `[ObservableProperty]` 特性
- [x] 所有命令使用 `[RelayCommand]` 特性
- [x] `Observable.Interval` 替换为 `DispatcherTimer`
- [x] `RaisePropertyChanged` 替换为 `OnPropertyChanged`
- [x] 移除 ReactiveUI 相关 using
- [x] 编译无错误

## 第五阶段：QueueViewModel 重构

- [x] 所有命令使用 `[RelayCommand]` 特性
- [x] `RaisePropertyChanged` 替换为 `OnPropertyChanged`
- [x] 移除 ReactiveUI 相关 using
- [x] 编译无错误

## 第六阶段：其他 ViewModel 重构

- [x] PlaylistManagementViewModel 使用 MVVM Toolkit
- [x] LibraryCategoryViewModel 使用 MVVM Toolkit
- [x] StatisticsViewModel 使用 MVVM Toolkit
- [x] SettingsViewModel 使用 MVVM Toolkit
- [x] MetadataEditorViewModel 使用 MVVM Toolkit
- [x] 所有 ViewModel 编译无错误

## 第七阶段：编译和功能验证

- [x] `dotnet build` 编译成功
- [ ] 播放/暂停/停止功能正常
- [ ] 导航功能正常
- [ ] 队列功能正常
- [ ] 播放列表管理功能正常
- [ ] 原有功能未受影响

## 代码质量检查

- [x] 所有 ViewModel 使用 `partial class`
- [x] 私有字段使用 `_camelCase` 命名
- [x] 公共属性由 Source Generator 生成
- [x] 无 `using ReactiveUI;` 引用
- [x] 无 `RaiseAndSetIfChanged` 调用
- [x] 无 `ReactiveCommand` 使用
- [ ] 异常处理完整
- [ ] 符合异步编程规范（async/await）
