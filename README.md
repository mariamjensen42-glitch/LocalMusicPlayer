# LocalMusicPlayer

[![CI](https://github.com/cycling02/LocalMusicPlayer/actions/workflows/ci.yml/badge.svg)](https://github.com/cycling02/LocalMusicPlayer/actions/workflows/ci.yml)
[![Release Please](https://github.com/cycling02/LocalMusicPlayer/actions/workflows/release-please.yml/badge.svg)](https://github.com/cycling02/LocalMusicPlayer/actions/workflows/release-please.yml)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

一款基于 Avalonia UI 的跨平台本地音乐播放器，支持 Windows、Linux 和 macOS。

## ✨ 特性

- 🎵 **多格式支持** - 支持 MP3、FLAC、WAV、AAC 等主流音频格式
- 📁 **智能音乐库** - 自动扫描本地音乐文件，智能分类管理
- 🎨 **现代化界面** - 采用 Fluent Design 设计语言，支持深色/浅色主题
- 🔊 **高品质播放** - 基于 LibVLC 实现，提供稳定的音频播放体验
- 📜 **播放列表** - 支持创建和管理多个播放列表
- ⌨️ **快捷键支持** - 丰富的键盘快捷键，提升操作效率
- 🖥️ **跨平台** - 支持 Windows、Linux、macOS 三大平台

## 🚀 快速开始

### 系统要求

- Windows 10/11
- Linux (Ubuntu 20.04+)
- macOS 11.0+

### 安装

1. 从 [Releases](https://github.com/cycling02/LocalMusicPlayer/releases) 下载对应平台的安装包
2. 解压并运行 `LocalMusicPlayer`

### 开发环境

```bash
# 克隆仓库
git clone https://github.com/cycling02/LocalMusicPlayer.git
cd LocalMusicPlayer

# 还原依赖
dotnet restore

# 构建项目
dotnet build

# 运行
dotnet run
```

## 🛠️ 技术栈

- [.NET 9](https://dotnet.microsoft.com/) - 跨平台开发框架
- [Avalonia UI](https://avaloniaui.net/) - 跨平台 UI 框架
- [LibVLC](https://www.videolan.org/vlc/libvlc.html) - 多媒体播放引擎
- [ReactiveUI](https://www.reactiveui.net/) - 响应式编程框架

## 📁 项目结构

```
LocalMusicPlayer/
├── Models/           # 数据模型
├── Services/         # 业务服务层
├── ViewModels/       # MVVM 视图模型
├── Views/            # Avalonia 视图
├── Converters/       # 数据转换器
├── Styles/           # 样式资源
└── Assets/           # 静态资源
```

## 🤝 贡献指南

我们欢迎所有形式的贡献！

### 提交规范

本项目使用 [Conventional Commits](https://www.conventionalcommits.org/) 规范：

```bash
# 新功能
feat: 添加歌词显示功能

# 修复 Bug
fix: 修复音量调节失效问题

# 文档更新
docs: 更新使用说明

# 代码重构
refactor: 优化播放引擎性能
```

### 开发流程

1. Fork 本仓库
2. 创建特性分支 (`git checkout -b feature/amazing-feature`)
3. 提交更改 (`git commit -m 'feat: add amazing feature'`)
4. 推送到分支 (`git push origin feature/amazing-feature`)
5. 创建 Pull Request

## 📄 许可证

本项目采用 [MIT](LICENSE) 许可证开源。

## 🙏 致谢

- [Avalonia UI](https://avaloniaui.net/) - 优秀的跨平台 UI 框架
- [LibVLCSharp](https://github.com/videolan/libvlcsharp) - .NET 平台的 LibVLC 绑定
- [TagLib#](https://github.com/mono/taglib-sharp) - 音频元数据读取库

---

<p align="center">Made with ❤️ by <a href="https://github.com/cycling02">cycling02</a></p>
