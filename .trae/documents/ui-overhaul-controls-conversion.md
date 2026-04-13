# UI 大改计划：WinUI3 → Avalonia 控件转换

## 概述

将 original-sound-hq-player-win2d-navi (WinUI3) 的 UI 设计转换为 LocalMusicPlayer (Avalonia) 项目，从控件开始逐步改造。

## 核心挑战

| WinUI3 技术 | Avalonia 替代方案 |
|-------------|------------------|
| Win2D CanvasControl (GPU渲染) | WriteableBitmap + Skia 渲染 |
| PixelShader (HLSL流体渐变) | Skia Shader 或 CSS 渐变动画 |
| Composition API (逐字裁剪动画) | Avalonia Animation + Clip |
| NavigationView | 自定义导航控件 |
| x:Load 条件加载 | IsVisible 绑定 |
| DesktopAcrylicBackdrop | Avalonia AcrylicBlur |

---

## 阶段一：自定义控件转换（优先级最高）

### 1.1 AlbumArtControl — 专辑封面控件

**WinUI3 原始功能**：
- Win2D GPU 渲染专辑封面
- 交叉淡入淡出动画（切换封面时旧图淡出新图淡入）
- 圆角遮罩 + 投影阴影
- RenderTarget 烘焙优化
- 图片去重机制

**Avalonia 实现方案**：
- 创建 `Controls/AlbumArtControl.axaml(.cs)`
- 使用 `Border` + `ClipToBounds` + `CornerRadius` 实现圆角
- 使用 `DropShadowEffect` 实现阴影
- 使用 `DoubleAnimation` 实现 Opacity 交叉淡入淡出
- 使用 `WriteableBitmap` 或直接 `Image` 控件显示封面
- 依赖属性：`ImageBytes`, `IsDark`, `CornerRadius`, `IsShadowEnabled`, `IsActive`

**关键文件**：
- 新建：`Controls/AlbumArtControl.axaml` + `Controls/AlbumArtControl.axaml.cs`
- 参考：`original-sound-hq-player-win2d-navi/Controls/AlbumArtControl.xaml(.cs)`

### 1.2 GradientBackgroundControl — 渐变背景控件

**WinUI3 原始功能**：
- PixelShader (HLSL) 流体渐变背景动画
- 从封面图提取4个主色调 (ColorThief)
- 颜色平滑过渡动画
- 亮/暗主题自适应
- 封面图叠加显示

**Avalonia 实现方案**：
- 创建 `Controls/GradientBackgroundControl.axaml(.cs)`
- **方案A（推荐）**：使用多个 `Ellipse` + `BlurEffect` + `DoubleAnimation` 模拟流体渐变（类似 macOS 动态壁纸效果）
- **方案B**：使用 Skia `SKCanvas` 自定义渲染（更接近原版但更复杂）
- 集成 ColorThief.NET 提取封面主色调
- 使用 `ColorAnimation` 实现颜色过渡
- 依赖属性：`IsBackgroundEnable`, `ImageBytes`, `IsDark`, `UseImageDominantTheme`
- 事件：`ThemeResolved`

**关键文件**：
- 新建：`Controls/GradientBackgroundControl.axaml` + `.cs`
- 新增 NuGet：`ColorThief.NET` 或自行实现颜色提取
- 参考：`original-sound-hq-player-win2d-navi/Controls/GradientBackgroundControl.xaml(.cs)`

### 1.3 LyricsLineControl — 歌词行控件

**WinUI3 原始功能**：
- 双层文本结构（底层半透明 + 顶层高亮）
- Composition InsetClip 逐字高亮动画
- 翻译文本显示
- 动态字体大小

**Avalonia 实现方案**：
- 创建 `Controls/LyricsLineControl.axaml(.cs)`
- 双层 `TextBlock`：底层 `Opacity=0.5`，顶层通过 `Clip` 裁剪实现逐字高亮
- 使用 `Avalonia.Animation` 的 `KeyFrameAnimation` 驱动 `Clip` 的 `RectangleGeometry.Rect` 实现裁剪动画
- 翻译文本 `TextBlock` 通过 `IsVisible` 控制显示
- 依赖属性：`LyricsText`, `TranslateText`, `LyricsFontSize`, `TranslateFontSize`, `IsCurrentLine`, `LineAnimateDuration`, `IsWFWLyrics`

**关键文件**：
- 新建：`Controls/LyricsLineControl.axaml` + `.cs`
- 参考：`original-sound-hq-player-win2d-navi/Controls/LyricsLineControl.xaml(.cs)`

### 1.4 NotifyIconControl — 系统托盘控件

**WinUI3 原始功能**：
- H.NotifyIcon 系统托盘
- 右键菜单：播放控制、播放模式、音量、设置、退出

**Avalonia 实现方案**：
- 创建 `Controls/NotifyIconControl.axaml(.cs)`
- Avalonia 同样支持 `H.NotifyIcon` 包
- 迁移菜单结构和命令绑定
- 添加播放模式互斥选择逻辑

**关键文件**：
- 新建：`Controls/NotifyIconControl.axaml` + `.cs`
- 新增 NuGet：`H.NotifyIcon.Avalonia`
- 参考：`original-sound-hq-player-win2d-navi/Controls/NotifyIconControl.xaml(.cs)`

---

## 阶段二：样式系统升级

### 2.1 按钮样式升级

**WinUI3 原始样式**：
- `CircularHoverButtonStyle` — 圆形悬停按钮（透明背景，悬停显示圆形遮罩）
- `SimpleHoverButtonStyle` — 矢量箭头悬停按钮
- `PlayHoverButtonStyle` — 播放按钮悬停样式
- `NoHoverButtonStyle` — 无悬停效果按钮

**Avalonia 改造**：
- 在 `Styles/Resources.axaml` 中新增/修改 ControlTheme
- `CircularHoverButtonTheme` → 替代当前 `PlayerIconButtonTheme`，增加悬停圆形遮罩动画
- 新增 `NoHoverButtonTheme`
- 播放按钮增加悬停播放三角形箭头显示

### 2.2 滑块样式升级

**WinUI3 原始样式**：
- `GhostSliderStyle` — 极简风格，小 Thumb (10x6)，圆角轨道

**Avalonia 改造**：
- 修改 `MusicProgressSliderTheme` → 更接近 GhostSlider 风格
- Thumb 改为小圆角矩形
- 轨道更细更圆滑

### 2.3 菜单/下拉样式升级

**WinUI3 原始样式**：
- MenuFlyoutItem 悬停右移4px + BackEase 弹性动画
- ComboBoxItem 悬停右移 + 选中左侧 Pill 指示器

**Avalonia 改造**：
- 新增 `MenuItemTheme` 带悬停平移动画
- 新增 `ComboBoxItemTheme` 带 Pill 指示器

---

## 阶段三：主页面布局重构

### 3.1 MainWindow 重构

**WinUI3 原始结构**：
- 双 Frame 架构（MainFrame + PlayingFrame 覆盖层）
- 48px 紧凑 NavigationView（仅图标）
- 标题栏透明度联动

**Avalonia 改造**：
- 保留当前双区域架构（主区域 + PlayerOverlay）
- 将 220px 侧边栏改为 48px 紧凑图标导航栏（参考原版 NavigationView）
- 添加侧边栏展开/收起功能
- 标题栏与 PlayingFrame 可见性联动

### 3.2 PlayerPageView 重构（播放详情页）

**WinUI3 原始结构**：
- 全屏 ShaderBackgroundControl 渐变背景
- 左右两栏：封面+控制 / 歌词
- 歌词区 OpacityMaskView 上下渐变淡出
- 顶部控制栏（返回/全屏/可见性切换）鼠标悬停显示

**Avalonia 改造**：
- 集成 GradientBackgroundControl 作为全屏背景
- 集成 AlbumArtControl 替代简单 Image 封面
- 集成 LyricsLineControl 替代当前歌词模板
- 歌词区添加 `OpacityMask` 上下渐变淡出
- 顶部控制栏添加鼠标悬停显示/隐藏逻辑

### 3.3 底部播放栏升级

**WinUI3 对比**：
- 原版无独立底部播放栏（播放控制在详情页）
- 当前 Avalonia 版有 72px 底部播放栏

**Avalonia 改造**：
- 保留底部播放栏（这是 Avalonia 版的特色功能）
- 升级按钮样式为 CircularHoverButton 风格
- 封面图增加圆角和阴影

---

## 阶段四：转换器补充

需要新增的转换器（参考 WinUI3 版本）：

| 转换器 | 功能 | 优先级 |
|--------|------|--------|
| `PlayStatusToIconConverter` | 播放状态→图标 Glyph | 高 |
| `PlayModeToIconConverter` | 播放模式→图标 Glyph | 高 |
| `VolumeToIconConverter` | 音量值→图标 Glyph | 中 |
| `FavouriteIconConverter` | 收藏状态→图标 Glyph | 中 |
| `FullScreenIconConverter` | 全屏状态→图标 Glyph | 低 |

---

## 实施顺序

1. **AlbumArtControl** — 封面控件是视觉核心
2. **GradientBackgroundControl** — 背景决定整体氛围
3. **LyricsLineControl** — 歌词体验升级
4. **样式系统升级** — 按钮和滑块样式
5. **PlayerPageView 重构** — 集成新控件
6. **MainWindow 布局调整** — 紧凑导航栏
7. **NotifyIconControl** — 系统托盘
8. **转换器补充** — 图标转换

---

## 技术约束

- 不能直接使用 WinUI 代码，必须用 Avalonia API 重写
- Win2D 相关功能用 Skia/Avalonia 原生 API 替代
- Composition 动画用 Avalonia Animation 替代
- 遵循项目规范：MVVM、编译绑定、无内联样式、无硬编码中文
- 所有新控件放在 `Controls/` 目录
