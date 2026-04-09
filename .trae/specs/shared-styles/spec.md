# 共享样式规范规格说明

## Why

当前项目存在以下样式问题：

1. **内联样式泛滥** - `PlayerPageView.axaml` 中大量使用内联颜色、尺寸属性，如 `Background="#111111"`、`Width="40"`
2. **样式重复定义** - 相同的按钮样式（圆形图标按钮）在多个视图重复定义
3. **样式碎片化** - 没有统一的按钮、边框、间距等样式定义
4. **维护困难** - 修改视觉风格需要改多个文件

## What Changes

### 新增共享样式资源

#### 按钮样式
- **CircleIconButtonTheme** - 圆形图标按钮（40x40），用于导航、队列等操作按钮
- **CircleIconButtonLargeTheme** - 大圆形图标按钮（72x72），用于播放/暂停主按钮
- **CircleIconButtonSmallTheme** - 小圆形图标按钮（34x34），用于歌曲列表行操作按钮
- **TransparentIconButtonTheme** - 透明图标按钮，用于控制区域的图标按钮

#### 边框样式
- **CardBorderTheme** - 卡片边框样式，统一卡片的背景和边框
- **InputBorderTheme** - 输入框边框样式

#### 文本样式
- **TitleTextStyle** - 标题文本样式（16px, 500 weight）
- **BodyTextStyle** - 正文文本样式
- **MutedTextStyle** - 次要/静音文本样式

#### 尺寸资源
- **IconButtonSize** - 40 (标准图标按钮尺寸)
- **IconButtonSizeLarge** - 72 (大图标按钮尺寸)
- **IconButtonSizeSmall** - 34 (小图标按钮尺寸)
- **DefaultCornerRadius** - 20 (圆形按钮圆角)
- **CardCornerRadius** - 10 (卡片圆角)
- **SmallCornerRadius** - 8 (小圆角)

### 修改现有视图

#### PlayerPageView.axaml
- 移除所有内联颜色值，改用静态资源引用
- 移除重复的尺寸内联定义，改用共享样式
- 应用新的 `CircleIconButtonTheme` 和 `CircleIconButtonLargeTheme`
- 时间文本使用统一的 `MonospaceTextStyle`

#### MainWindow.axaml
- 优化侧边栏按钮样式，统一使用共享样式
- 标题栏按钮已使用主题资源，保持一致

## Impact

### 影响的规格
- `core-features` - 无影响（纯 UI 样式变更）
- `architecture-refactoring` - 无影响
- `music-library-management` - 无影响

### 影响的代码
| 文件 | 变化类型 | 说明 |
|------|---------|------|
| Styles/Resources.axaml | 扩展 | 新增尺寸资源和文本样式 |
| Styles/Controls/ButtonStyles.axaml | 新增 | 统一按钮样式定义 |
| Views/PlayerPageView.axaml | 重构 | 移除内联样式，使用共享资源 |
| Views/MainWindow.axaml | 优化 | 一致性使用共享样式 |

## ADDED Requirements

### Requirement: 圆形图标按钮样式

系统 SHALL 提供统一的圆形图标按钮样式，通过 `CircleIconButtonTheme` 实现。

#### Scenario: 标准圆形图标按钮
- **WHEN** 需要一个 40x40 的圆形图标按钮
- **THEN** 使用 `Theme="{StaticResource CircleIconButtonTheme}"`
- **AND** 背景使用 `BgCardBrush`
- **AND** 边框使用 `BorderSubtleBrush`
- **AND** 圆角为 20（完全圆形）

#### Scenario: 大圆形播放按钮
- **WHEN** 需要播放/暂停的大按钮（72x72）
- **THEN** 使用 `Theme="{StaticResource CircleIconButtonLargeTheme}"`
- **AND** 背景使用渐变 `PlayButtonGradient`

### Requirement: 透明图标按钮样式

系统 SHALL 提供透明的图标按钮样式，用于控制区域。

#### Scenario: 控制区域图标按钮
- **WHEN** 需要一个透明的图标按钮
- **THEN** 使用 `Theme="{StaticResource TransparentIconButtonTheme}"`
- **AND** 无背景色
- **AND** 光标为 Hand

### Requirement: 尺寸资源

系统 SHALL 提供统一的尺寸常量，避免硬编码。

#### Scenario: 使用尺寸资源
- **WHEN** 设置按钮宽度为 40
- **THEN** 使用 `{StaticResource IconButtonSize}` 替代硬编码

## MODIFIED Requirements

### Requirement: PlayerPageView.axaml 样式重构

**重构前**：
- 按钮使用内联样式 `Background="#111111" BorderBrush="#1A1A1A"`
- 尺寸使用硬编码 `Width="40" Height="40"`
- 文本样式重复定义

**重构后**：
- 按钮使用 `Theme="{StaticResource CircleIconButtonTheme}"`
- 颜色使用 `{StaticResource BgCardBrush}` 等静态资源
- 文本样式统一引用

### Requirement: 颜色资源一致性

**重构前**：
- 内联使用 `#111111` 等颜色值

**重构后**：
- 所有颜色通过 `{StaticResource BgCardBrush}` 等引用
- 统一在 `Resources.axaml` 中定义

## REMOVED Requirements

### Requirement: 内联样式

**移除原因**：内联样式难以维护和统一修改

**迁移**：
- 颜色值迁移到静态资源
- 尺寸值迁移到样式主题或尺寸资源
- 复杂样式封装到 ControlTheme

## 技术实现约束

### 样式定义规范

```xml
<!-- Styles/Controls/ButtonStyles.axaml -->
<ResourceDictionary>
    <!-- 标准圆形图标按钮 -->
    <ControlTheme x:Key="CircleIconButtonTheme" TargetType="Button">
        <Setter Property="Width" Value="{StaticResource IconButtonSize}" />
        <Setter Property="Height" Value="{StaticResource IconButtonSize}" />
        <Setter Property="Background" Value="{StaticResource BgCardBrush}" />
        <Setter Property="BorderBrush" Value="{StaticResource BorderSubtleBrush}" />
        <Setter Property="BorderThickness" Value="1" />
        <Setter Property="CornerRadius" Value="{StaticResource DefaultCornerRadius}" />
        <Setter Property="Cursor" Value="Hand" />
    </ControlTheme>
</ResourceDictionary>
```

### 尺寸资源定义

```xml
<!-- Styles/Resources.axaml -->
<x:Double x:Key="IconButtonSize">40</x:Double>
<x:Double x:Key="IconButtonSizeLarge">72</x:Double>
<x:Double x:Key="IconButtonSizeSmall">34</x:Double>
<CornerRadius x:Key="DefaultCornerRadius">20</CornerRadius>
<CornerRadius x:Key="CardCornerRadius">10</CornerRadius>
```

### 视图使用示例

```xml
<!-- 重构后 -->
<Button Theme="{StaticResource CircleIconButtonTheme}"
        Command="{Binding NavigateBackCommand}">
    <TextBlock Text="&#xE74A;"
               FontFamily="Segoe Fluent Icons"
               FontSize="16"
               Foreground="{StaticResource TextSecondaryBrush}" />
</Button>

<!-- 重构前 -->
<Button Width="40"
        Height="40"
        Background="#111111"
        BorderBrush="#1A1A1A"
        BorderThickness="1"
        CornerRadius="20"
        Cursor="Hand">
    <TextBlock Text="&#xE74A;"
               FontFamily="Segoe Fluent Icons"
               FontSize="16"
               Foreground="{StaticResource TextSecondaryBrush}" />
</Button>
```

## 重构步骤概要

1. 扩展 `Styles/Resources.axaml` 添加尺寸资源和文本样式
2. 创建 `Styles/Controls/ButtonStyles.axaml` 定义按钮主题
3. 重构 `PlayerPageView.axaml` 移除内联样式
4. 优化 `MainWindow.axaml` 按钮样式引用
5. 验证构建通过
