# 全局「添加到歌单」右键菜单功能设计

## 日期: 2026-04-12

## 功能概述

在全局歌曲列表中（首页、专辑详情、艺术家详情、歌单管理等）添加右键菜单「添加到歌单」选项，点击后弹出歌单选择列表，用户可选择目标歌单完成添加。

## 架构设计

### 核心思路

采用 **IDialogService 扩展 + 各 ViewModel 声明命令** 方案：

1. **IDialogService** 新增 `ShowAddToPlaylistDialogAsync(Song song)` 方法
2. **DialogService** 实现弹窗逻辑（显示歌单列表供选择）
3. **各 ViewModel** 添加 `AddToPlaylistCommand`（Home, AlbumDetail, ArtistDetail, PlaylistManagement）
4. **各 View** 在歌曲行 DataTemplate 中添加 ContextMenu

### 数据流

```
用户右键歌曲 → ContextMenu → AddToPlaylistCommand(song)
    ↓
ViewModel.AddToPlaylistAsync(filePath)
    ↓
_dialogService.ShowAddToPlaylistDialogAsync(song)
    ↓
弹窗显示 UserPlaylists 列表
    ↓
用户选择歌单 → _userPlaylistService.AddSongToPlaylistAsync(playlistId, song)
```

## 改动清单

### 1. IDialogService (Services/Navigation/IDialogService.cs)
- 新增: `Task ShowAddToPlaylistDialogAsync(Song song)`

### 2. DialogService (Services/Navigation/DialogService.cs)
- 实现 `ShowAddToPlaylistDialogAsync`
- 弹窗内容:
  - 标题: "Add to Playlist"
  - ListBox 显示所有 UserPlaylists（排除 favorites）
  - 点击歌单项执行添加并关闭弹窗
  - 底部可选: "Create New Playlist" 按钮

### 3. HomeViewModel (ViewModels/HomeViewModel.cs)
- 新增: `[RelayCommand] private async Task AddToPlaylistAsync(string filePath)`
- 调用 `_dialogService.ShowAddToPlaylistDialogAsync(song)`

### 4. AlbumDetailViewModel (ViewModels/AlbumDetailViewModel.cs)
- 同上

### 5. ArtistDetailViewModel (ViewModels/ArtistDetailViewModel.cs)
- 同上

### 6. PlaylistManagementViewModel (ViewModels/PlaylistManagementViewModel.cs)
- 新增: `AddToPlaylistCommand`（已有类似方法 `AddSongToSelectedPlaylistAsync`，可复用或新增）

### 7. HomeView.axaml (Views/Library/HomeView.axaml)
- 在歌曲 Border 中添加 ContextMenu
- MenuItem: 「添加到歌单」（图标 &#xE710;）

### 8. AlbumDetailView.axaml (Views/Details/AlbumDetailView.axaml)
- 同上

### 9. ArtistDetailView.axaml (Views/Details/ArtistDetailView.axaml)
- 同上

### 10. PlaylistManagementView.axaml (Views/Playlist/PlaylistManagementView.axaml)
- 在现有 ContextMenu 中追加 Separator + MenuItem

## UI 设计

### ContextMenu 结构
```
┌─────────────────────┐
│ ▶ Play              │
│ ✏ Edit Info         │
│ 📋 Batch Edit       │
│ ─────────────────── │
│ ➕ Add to Playlist ► │ ← 新增
│ ─────────────────── │
│ 🗑 Remove (仅歌单页) │
└─────────────────────┘
        ↓ 展开子菜单
┌─────────────────┐
│ ❤ Favorites     │
│ 📁 Playlist 1   │
│ 📁 Playlist 2   │
│ ...             │
└─────────────────┘
```

### 弹窗设计
- 尺寸: 320 x 400
- 背景: BgCardBrush (#1E1E2E)
- 歌单项: 可点击的列表项，悬停高亮
- 图标: 歌单图标 (📁 &#xE8FA;)

## 错误处理

- 无歌单时: 弹窗内显示空状态提示
- 添加失败: 静默处理（已存在的歌曲不重复添加）
- 服务不可用: try-catch 包裹，不崩溃

## 国际化

- 使用 DynamicResource 绑定字符串资源
- 新增 Strings.axaml 资源: String_AddToPlaylist
