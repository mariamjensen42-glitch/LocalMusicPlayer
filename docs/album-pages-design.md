# 专辑页面复刻设计文档

## 概述

将 original-sound-hq-player-win2d-navi (WinUI3) 的专辑浏览页(AlbumPage)和专辑详情页(SongCollectionPage)完整复刻到 Avalonia 11 项目中。

## 原版参考

- `View/AlbumPage.xaml` — WinUI3 专辑浏览页（SemanticZoom + GridView 分组卡片）
- `View/SongCollectionPage.xaml` — WinUI3 专辑详情页（封面信息头 + 歌曲列表）
- `ViewModel/Pages/AlbumViewModel.cs` — 浏览页 ViewModel
- `ViewModel/Pages/SongCollectionViewModel.cs` — 详情页 ViewModel

## 现有文件

- `Views/Library/AlbumsPageView.axaml` — 当前简单卡片布局
- `Views/Details/AlbumDetailView.axaml` — 当前基础详情
- `ViewModels/AlbumsPageViewModel.cs` — 当前浏览页 VM
- `ViewModels/AlbumDetailViewModel.cs` — 当前详情页 VM
- `Models/AlbumGroup.cs` — 专辑分组模型

---

## Page 1: 专辑浏览页 (AlbumsPageView)

### 布局结构

```
Grid(RowDefinitions="Auto,*")
├── Row0: 标题栏 Border
│   └── TextBlock "专辑" (24px Bold)
└── Row1: ScrollViewer
    └── ItemsControl (ItemsSource = AlbumGroups, 分组显示)
        ├── Group Header: 首字母分组标题 (28px SemiBold)
        └── ItemTemplate: 专辑卡片 (WrapPanel 排列)
```

### 专辑卡片设计 (完全匹配原版 AlbumPage DataTemplate)

```
Grid(Height=205, Width≈180)
├── Row0: 封面区域 Grid(150x150, CornerRadius=5)
│   ├── Image (Stretch=UniformToFill, AlbumArtConverter)
│   └── 默认封面占位图标 FontIcon (E93C, 50px)
│   └── 右下角设备存在图标 FontIcon (16px)
├── Row1: 专辑名 TextBlock (SemiBold, MaxHeight=40, TextWrapping)
└── Row2: 底部信息 Grid(Width=150)
    ├── Column0: 艺术家名 TextBlock (12px, Ellipsis)
    ├── Column1: 歌曲数 TextBlock (12px)
    └── Column2: "首" 文本 TextBlock (12px)
```

### 分组策略

- 按 AlbumName 首字母分组（A-Z, #）
- 每组头部显示大号首字母标题
- 使用 ItemsControl 嵌套：外层遍历分组，内层 WrapPanel 展示卡片

### 交互

- **点击卡片** → 导航到 AlbumDetailView，传入 AlbumGroup
- **右键菜单** → 播放 / 收藏 / 添加到播放列表 / 属性
- **Hover** → 卡片微缩放或阴影增强

---

## Page 2: 专辑详情页 (AlbumDetailView)

### 布局结构

```
ScrollViewer
└── StackPanel(Spacing=24, Margin=32)
    ├── 信息头区域 Grid(ColumnDefinitions="Auto,*")
    │   ├── Column0: 封面 Border(150x150, CornerRadius=5)
    │   │   └── Image (AlbumArtControl 或 Image+Converter)
    │   └── Column1: 信息 StackPanel(Spacing=8, Margin=28,0,0,0)
    │       ├── Row0: 专辑名 TextBlock (30px Bold, 可选中文本)
    │       ├── Row1: 艺术家 TextBlock (20px)
    │       ├── Row2: 年份/歌曲数 TextBlock (14px SecondaryBrush)
    │       └── Row3: 操作按钮行 StackPanel(Orientation=Horizontal, Spacing=12)
    │           ├── [▶ 播放全部] PrimaryButton
    │           ├── [＋ 添加到] SecondaryButton + MenuFlyout
    │           └── [✏ 编辑] SecondaryButton
    │
    └── 歌曲列表 Border(CornerRadius=12)
        ├── 表头 Border
        │   └── Grid(列: 封面40, 收藏Auto, 标题3*, 艺术家1.5*, 专辑3*, 碗号0.5*, 轨号0.5*, 品质0.5*, 时长0.5*, 队列0.5*, 设备0.25*)
        └── ListBox (ItemsSource=Songs)
            └── ItemTemplate: SongRow
                ├── Column0: 封面缩略图 Button(40x40, CornerRadius=5)
                ├── Column1: 收藏切换按钮
                ├── Column2: 歌曲标题 TextBlock
                ├── Column3: 艺术家 Button(可点击跳转)
                ├── Column4: 专辑 Button(可点击跳转)
                ├── Column5: 碟号 TextBlock
                ├── Column6: 轨号 TextBlock
                ├── Column7: 品质图标 Image
                ├── Column8: 时长 TextBlock
                ├── Column9: 添加到队列 Button
                └── Column10: 设备存在图标
```

### 信息头字段映射

| 字段 | 来源 | 格式 |
|------|------|------|
| 专辑名 | AlbumGroup.AlbumName | 30px Bold |
| 副标题 | 所有艺术家 join(" · ") | 20px |
| 三级标题 | "{Year} · {N} 首歌曲" | 14px Secondary |
| 封面 | AlbumGroup.CoverArtPath | 150×150 |

### 歌曲列表列定义

| # | 列名 | 绑定 | 宽度 | 交互 |
|---|------|------|------|------|
| 0 | 封面 | Song.AlbumArtPath | 40×40 | 点击播放 |
| 1 | 收藏 | Song.IsFavorite | Auto | 切换收藏 |
| 2 | 标题 | Song.Title | 3* | - |
| 3 | 艺术家 | Song.Author | 1.5* | 点击→艺术家页 |
| 4 | 专辑 | Song.Album | 3* | 点击→专辑页 |
| 5 | 碟号 | Song.DiskNumber | 0.5* | - |
| 6 | 轨号 | Song.TrackNumber | 0.5* | - |
| 7 | 品质 | Song.Quality | 0.5* | 图标显示 |
| 8 | 时长 | Song.Duration | 0.5* | TimeSpan转换 |
| 9 | 队列 | Command | 0.5* | 添加到当前播放列表 |
| 10 | 设备 | Song.IsExistOnDevice | 0.25* | 图标显示 |

### 交互

- **双击歌曲** → 播放该歌曲，替换播放列表为专辑所有歌曲
- **右键歌曲** → 上下文菜单：
  - 播放
  - 收藏/取消收藏
  - 添加到播放列表（子菜单）
  - 音频格式转换（子菜单：Wav/Mp3/Flac/Ogg/Opus）
  - 添加到当前播放列表
  - 重新获取歌词
  - 在资源管理器中打开
  - 属性
  - 从磁盘删除
- **点击艺术家** → 导航到艺术家详情
- **点击专辑** → 导航到专辑详情（当前专辑则忽略）

---

## ViewModel 变更

### AlbumsPageViewModel 增强

新增属性和方法：
- `AlbumGroups` 保持不变（ObservableCollection<AlbumGroup>）
- 新增 `GroupedAlbums` — 按首字母分组的集合（用于 XAML 分组渲染）
- 新增 `SelectedItem` — 当前右键选中的专辑
- 新增 `AlbumMenuOptions` — 右键菜单选项（ ObservableCollection<MenuModel>）
- 新增命令：`PlayCommand`, `AddToFavourCommand`, `ShowPropertyWindowCommand`, `AddToPlayListCommand`
- `SelectItemCommand` → 增强为导航到 AlbumDetailView

### AlbumDetailViewModel 增强

新增属性：
- `SelectedSong` — 当前选中歌曲
- `SelectedSongs` — 多选歌曲集合
- `SecondTitle` — 艺术家副标题
- `ThirdTitle` — 年份/歌曲数信息
- `SongMenuOptions` — 右键菜单选项

新增命令：
- `PlayAllCommand` — 播放全部
- `AddToPlaylistCommand` — 添加到播放列表
- `EditMetadataCommand` — 编辑元数据
- `PlaySongCommand` — 播放单首
- `ToggleFavoriteCommand` — 切换收藏
- `AddToCurrentPlaylistCommand` — 添加到当前队列
- `OpenInExplorerCommand` — 打开文件位置
- `DeleteSongCommand` — 删除歌曲

---

## 样式与控件复用

### 复用已有控件

- `Controls/AlbumArtControl.axaml` — 封面图片控件（带圆角、阴影、淡入淡出）
- `CircularHoverButtonTheme` — 圆形悬停按钮样式
- `PrimaryButtonTheme` / `SecondaryButtonTheme` — 按钮样式
- `AlbumCoverCardTheme` — 封面卡片样式
- `SongListItemRowTheme` — 歌曲行样式

### 可能需要新增的样式

- `AlbumCardTheme` — 专辑卡片容器样式（hover 效果、阴影）
- `AlbumGroupHeaderTheme` — 分组头文本样式
- `SongTableHeaderTheme` — 歌曲表头样式

---

## WinUI3 → Avalonia 映射

| WinUI3 控件 | Avalonia 替代 | 备注 |
|-------------|--------------|------|
| SemanticZoom | ScrollViewer + 手动分组 | Avalonia 无 SemanticZoom |
| GridView | ItemsControl + WrapPanel | 功能等效 |
| CollectionViewSource.GroupDescriptions | ViewModel 层分组 | 在 VM 中预分组 |
| AutoScrollView | TextBlock.TextTrimming | 截断替代滚动 |
| MenuFlyout | ContextMenu | Avalonia 原生支持 |
| ListViewItemStyle + ContextFlyout | ListBoxItem + ContextMenu | 直接绑定 |
| EntranceThemeTransition | 可选 Avalonia Animation | 页面过渡动画 |
| DevWinUI.AutoScrollView | TextTrimming | 同上 |

---

## 文件变更清单

### 修改文件

1. `Views/Library/AlbumsPageView.axaml` — 重写为分组卡片网格布局
2. `Views/Library/AlbumsPageView.axaml.cs` — 添加右键菜单事件处理
3. `Views/Details/AlbumDetailView.axaml` — 重写为原版详情布局
4. `Views/Details/AlbumDetailView.axaml.cs` — 添加事件处理
5. `ViewModels/AlbumsPageViewModel.cs` — 增加分组、菜单、命令
6. `ViewModels/AlbumDetailViewModel.cs` — 增加完整功能
7. `Styles/Resources.axaml` — 可能新增主题样式

### 可能新增文件

- 无需新增独立文件，复用现有 Controls/
