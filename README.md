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

### 音乐播放
- 支持 MP3、FLAC、WAV、AAC 等主流音频格式
- 基于 LibVLC 高品质播放引擎
- 播放控制：播放、暂停、停止、上一曲、下一曲
- 进度控制：进度条拖拽、音量调节、静音
- 播放模式：顺序播放、随机播放、循环播放

### 音乐库管理
- 自动扫描本地音乐文件夹
- 实时监控文件夹变化，新增/删除文件自动更新
- 支持按歌手、专辑、文件夹、收藏分类浏览
- 歌曲详情：标题、艺术家、专辑、时长、封面、播放次数

### 歌手/专辑详情页
- 展示封面图、名称、歌曲数量、总时长
- 歌曲列表：序号、标题、专辑/艺术家、播放次数、时长
- 快捷操作：播放全部、随机播放、单曲播放

### 播放列表
- 创建、编辑、删除自定义播放列表
- 收藏歌曲功能
- 播放队列管理，实时查看和调整播放顺序

### 播放统计
- 记录每首歌曲的播放次数
- 记录最后播放时间
- 统计总播放时长、歌曲数量等

### 歌词支持
- 自动匹配歌曲歌词文件
- 实时同步歌词显示

### 界面功能
- 现代化深色主题设计
- 侧边栏导航：音乐库、分类、统计、设置
- 底部播放控制栏：当前播放信息、播放控制、进度条、音量
- 右侧播放队列面板

### 快捷键支持
- 空格键：播放/暂停
- 左右方向键：上一曲/下一曲
- 上下方向键：音量调节
- Ctrl+F：搜索

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

## 📁 项目结构

```
LocalMusicPlayer/
├── Models/              # 数据模型 (Song, Playlist, ArtistGroup, AlbumGroup 等)
├── Services/             # 业务服务层 (播放器、扫描、播放列表、统计等)
│   ├── MusicPlayerService/    # 音频播放
│   ├── ScanService/          # 音乐库扫描
│   ├── PlaylistService/      # 播放列表管理
│   ├── StatisticsService/    # 播放统计
│   └── ...
├── ViewModels/           # MVVM 视图模型
├── Views/                # Avalonia XAML 视图
├── Converters/           # 数据转换器
├── Styles/               # 样式资源 (颜色、主题、按钮样式)
└── Assets/               # 静态资源
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

---

<p align="center">Made with ❤️ by <a href="https://github.com/cycling02">cycling02</a></p>