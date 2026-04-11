# Bug 修复记录

## 2026-04-12 - 代码审查关键问题修复

### 问题 #1 [严重] - 返回按钮 IsVisible 绑定类型错误
**文件**: `Views/MainWindow.axaml`, `Converters/NavPageVisibilityConverter.cs`
**问题描述**: 返回按钮的 `IsVisible` 属性使用了 `NavigationPageToColorConverter`（返回 SolidColorBrush），导致运行时类型转换异常。
**修复方案**:
- 新建 `NavPageVisibilityConverter.cs`，返回 `bool` 类型
- 修改绑定从 `NavigationPageToColorConverter` 改为 `NavPageVisibilityConverter`
- 逻辑：当前页面不是 Home 时显示返回按钮

### 问题 #2 [严重] - AlbumCardShadowBlur 资源定义顺序错误
**文件**: `Styles/Resources.axaml`
**问题描述**: 第784行引用了 `{StaticResource AlbumCardShadowBlur}`，但该颜色定义在第801行，XAML StaticResource 必须前向定义。
**修复方案**: 将 `AlbumCardShadowBlur` 定义从第801行移动到第588行（AlbumCardShadow 之后），确保在引用前已定义。

### 问题 #3 [中等] - 设置按钮未绑定命令
**文件**: `Views/MainWindow.axaml`
**问题描述**: 设置按钮缺少 Command 和 ToolTip 绑定，点击无响应。
**修复方案**: 添加 `Command="{Binding NavigateToSettingsCommand}"` 和 `ToolTip.Tip="设置"`。

### 问题 #4 [低] - 全屏按钮未绑定命令
**文件**: `Views/MainWindow.axaml`
**问题描述**: 全屏按钮缺少 Command 和 ToolTip 绑定，点击无响应。
**修复方案**: 添加 `Command="{Binding ToggleFullScreenCommand}"` 和 `ToolTip.Tip="全屏"`。

## 2026-04-12 - HomeView.axaml 编译绑定错误修复

### 问题 #1 [严重] - x:DataType 与实际 DataContext 类型不匹配
**文件**: `Views/HomeView.axaml`
**问题描述**: `x:DataType` 设置为 `MainWindowViewModel`，但 ViewLocator 通过约定将 `HomeViewModel` 映射到 `HomeView`，实际 DataContext 是 `HomeViewModel`。导致编译绑定无法解析 `PlaySongCommand`、`CurrentSong`、`IsPlaying`、`ToggleFavoriteCommand` 等属性。
**修复方案**: 将 `x:DataType` 从 `vm:MainWindowViewModel` 改为 `vm:HomeViewModel`，移除不匹配的 `Design.DataContext` 块。

### 问题 #2 [中等] - StackPanel 使用了不存在的 Padding 属性
**文件**: `Views/HomeView.axaml`
**问题描述**: Avalonia 的 `StackPanel` 没有 `Padding` 属性，导致编译错误。
**修复方案**: 将 `Padding="32"` 改为 `Margin="32"`。

### 问题 #3 [中等] - ItemVirtualizationMode 不是 Avalonia 有效属性
**文件**: `Views/HomeView.axaml`
**问题描述**: `ListBox` 上使用了 `ItemVirtualizationMode="Recycling"`，这不是 Avalonia 的有效属性（WPF 属性）。
**修复方案**: 移除 `ItemVirtualizationMode="Recycling"` 属性。

### 问题 #4 [中等] - 绑定路径与 HomeViewModel 属性不匹配
**文件**: `Views/HomeView.axaml`
**问题描述**: `Library.FilteredSongs` 绑定路径在 `HomeViewModel` 中不存在（`HomeViewModel` 没有 `Library` 属性）；`ShuffleCommand` 应为 `ShufflePlayCommand`。
**修复方案**: `{Binding Library.FilteredSongs}` → `{Binding Songs}`；`{Binding ShuffleCommand}` → `{Binding ShufflePlayCommand}`。

### 问题 #5 [低] - XML 中 & 未转义
**文件**: `Views/HomeView.axaml`
**问题描述**: `Text="标题 & 艺术家"` 中的 `&` 是非法 XML 实体引用。
**修复方案**: 改为 `Text="标题 &amp; 艺术家"`。

### 问题 #6 [低] - HomeViewModel.Songs 返回类型不匹配
**文件**: `ViewModels/HomeViewModel.cs`
**问题描述**: `Songs` 属性声明为 `ReadOnlyObservableCollection<Song>`，但 `_musicLibraryService.FilteredSongs` 返回 `ObservableCollection<Song>`，无法隐式转换。
**修复方案**: 将属性类型从 `ReadOnlyObservableCollection<Song>` 改为 `ObservableCollection<Song>`。
