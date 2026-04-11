# 代码审查 Spec

## Why
项目已积累大量代码（40+ 服务、15+ ViewModel、15+ View），需要进行系统性代码审查，识别架构违规、性能问题、安全隐患和代码质量问题，确保代码库健康可维护。

## What Changes
- 审查 Services 层：依赖注入配置、接口隔离、异步模式、资源管理
- 审查 ViewModels 层：MVVM 合规性、CommunityToolkit.Mvvm 使用、响应式模式
- 审查 Views/XAML 层：编译绑定、样式规范、资源管理
- 审查 Models 层：EF Core 实体设计、数据模型合理性
- 审查 Converters/Behaviors 层：可复用性、性能
- 审查整体架构：层间依赖、服务生命周期、内存泄漏风险

## Impact
- Affected specs: 所有现有 spec 均可能受影响（审查可能发现需修改的问题）
- Affected code: Services/, ViewModels/, Views/, Models/, Converters/, Behaviors/, Data/, App.axaml.cs

## ADDED Requirements

### Requirement: Services 层审查
系统 SHALL 审查所有服务实现，检查以下方面：

#### Scenario: 依赖注入配置正确性
- **WHEN** 审查 App.axaml.cs 中的 DI 配置
- **THEN** 服务生命周期（Singleton/Transient）选择合理，无 Captive Dependency 问题

#### Scenario: 异步模式合规
- **WHEN** 审查服务中的异步方法
- **THEN** 使用 async/await 而非 .Result/.Wait()，正确使用 CancellationToken，无 async void

#### Scenario: 资源管理
- **WHEN** 审查涉及 IDisposable 的服务（如 LibVLCSharp、EF Core DbContext）
- **THEN** 资源正确释放，无泄漏风险

### Requirement: ViewModels 层审查
系统 SHALL 审查所有 ViewModel，检查以下方面：

#### Scenario: MVVM 合规性
- **WHEN** 审查 ViewModel 实现
- **THEN** 无直接引用 View 层类型，使用 CommunityToolkit.Mvvm 的 [ObservableProperty]/[RelayCommand] 而非手动实现

#### Scenario: 响应式模式
- **WHEN** 审查 ViewModel 中的数据流
- **THEN** 正确使用 Observable/Subscriber 模式，无事件订阅泄漏

### Requirement: XAML 层审查
系统 SHALL 审查所有 XAML 文件，检查以下方面：

#### Scenario: 编译绑定
- **WHEN** 审查 XAML 绑定
- **THEN** 所有绑定使用 x:DataType 编译绑定，无反射绑定

#### Scenario: 样式与资源
- **WHEN** 审查样式定义
- **THEN** 资源键遵循 PascalCase 命名规范，无硬编码字符串，样式统一管理

### Requirement: Models 层审查
系统 SHALL 审查所有数据模型，检查以下方面：

#### Scenario: EF Core 实体设计
- **WHEN** 审查数据模型和 DbContext
- **THEN** 实体关系正确，索引合理，无 N+1 查询风险

### Requirement: 整体架构审查
系统 SHALL 审查整体架构，检查以下方面：

#### Scenario: 层间依赖
- **WHEN** 审查项目依赖关系
- **THEN** View → ViewModel → Service 单向依赖，无循环依赖

#### Scenario: 异常处理
- **WHEN** 审查全局和局部异常处理
- **THEN** 异常被正确捕获和记录，无吞异常现象

## 审查优先级
1. **Critical**: 内存泄漏、资源未释放、线程安全问题
2. **Important**: MVVM 违规、异步模式错误、DI 配置问题
3. **Minor**: 命名规范、代码风格、性能优化建议
