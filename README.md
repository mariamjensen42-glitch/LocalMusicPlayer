# LocalMusicPlayer

[![CI](https://github.com/cycling02/LocalMusicPlayer/actions/workflows/ci.yml/badge.svg)](https://github.com/cycling02/LocalMusicPlayer/actions/workflows/ci.yml)
[![Release Please](https://github.com/cycling02/LocalMusicPlayer/actions/workflows/release-please.yml/badge.svg)](https://github.com/cycling02/LocalMusicPlayer/actions/workflows/release-please.yml)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/)
[![Avalonia](https://img.shields.io/badge/Avalonia-11.3.8-blue.svg)](https://avaloniaui.net/)
[![LibVLC](https://img.shields.io/badge/LibVLC-3.9.6-orange.svg)](https://www.videolan.org/)
[![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20Linux%20%7C%20macOS-lightgrey.svg)]()

一款基于 Avalonia UI 的跨平台本地音乐播放器，支持 Windows、Linux 和 macOS。

## ✨ 功能特性

### 已删除的功能模块
- SmartPlaylist - 智能播放列表
- OnlineLyrics - 在线歌词搜索
- Statistics - 播放统计系统
- BatchMetadataEditor - 批量元数据编辑
- SystemTray - 系统托盘和自动启动
### 新增的统一视图
- MusicLibraryView - 统一的音乐库视图，使用Tab切换：
  - 全部歌曲
  - 专辑
  - 艺术家
  - 文件夹
### 简化的导航
主窗口导航简化为4个主要页面：

- 音乐库 (MusicLibrary)
- 收藏 (Favorites)
- 播放列表 (Playlists)
- 设置 (Settings)
### 保留的MVP功能
- ✅ 音乐扫描 + 元数据读取
- ✅ 播放控制（播放/暂停/进度/音量/播放模式）
- ✅ 用户播放列表
- ✅ 歌曲搜索
- ✅ 本地LRC歌词显示
- ✅ 专辑封面显示
- ✅ 收藏功能
## 🚀 快速开始

### 系统要求

- Windows 10/11
- Linux (Ubuntu 20.04+)
- macOS 11.0+

### 开发环境

```bash
git clone https://github.com/cycling02/LocalMusicPlayer.git
cd LocalMusicPlayer
dotnet restore
dotnet build
dotnet run
```

## 🛠️ 技术栈

- [.NET 9](https://dotnet.microsoft.com/) - 跨平台开发框架
- [Avalonia UI 11.3.8](https://avaloniaui.net/) - 跨平台 UI 框架
- [LibVLCSharp 3.9.6](https://www.videolan.org/vlc/libvlc.html) - 多媒体播放引擎
- [TagLibSharp 2.3.0](https://github.com/mono/taglib-sharp) - 音频元数据读取
- [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) - MVVM 框架
- [EF Core + SQLite](https://docs.microsoft.com/ef/core/) - 本地数据库

## 📁 项目结构

```
LocalMusicPlayer/
├── Models/              # 数据模型
├── Services/            # 业务服务层
│   ├── File/            # 文件管理、监控
│   ├── Library/         # 音乐库扫描、分类
│   ├── Media/           # 封面、歌词
│   ├── Navigation/      # 导航、对话框
│   ├── OnlineLyrics/    # 在线歌词搜索
│   ├── Playback/        # 音频播放
│   ├── Playlist/        # 播放列表、历史
│   ├── SmartPlaylist/   # 智能播放列表
│   ├── Statistics/      # 播放统计
│   └── System/          # 系统配置、托盘
├── ViewModels/          # MVVM 视图模型
├── Views/               # Avalonia XAML 视图
├── Converters/          # 数据转换器
├── Styles/              # 样式资源
├── Data/                # EF Core 数据库
└── Assets/             # 静态资源
```

## 🤝 贡献指南

### 提交规范

本项目使用中文提交规范：

```bash
feat: 添加歌词显示功能
fix: 修复音量调节失效问题
docs: 更新使用说明
refactor: 优化播放引擎性能
```

## 📄 许可证

[MIT](LICENSE)

***

<p align="center">Made with ❤️ by <a href="https://github.com/cycling02">cycling02</a></p>
