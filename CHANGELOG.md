# Changelog

## [1.6.0](https://github.com/mariamjensen42-glitch/LocalMusicPlayer/compare/v1.5.0...v1.6.0) (2026-04-14)


### ✨ 新功能

* Statistics SQLite migration, EQ equalizer, SmartPlaylist editor UI ([7a04402](https://github.com/mariamjensen42-glitch/LocalMusicPlayer/commit/7a04402166c63f4e713f20c1f0b198f46e32b0d9))

## [1.5.0](https://github.com/mariamjensen42-glitch/LocalMusicPlayer/compare/v1.4.0...v1.5.0) (2026-04-14)


### ✨ 新功能

* 添加音乐浏览、文件夹浏览和歌曲列表功能 ([606134a](https://github.com/mariamjensen42-glitch/LocalMusicPlayer/commit/606134acdeb79094e748fd8b671ff07b9bcce99d))


### 🐛 修复

* dispatch IsMiniMode window resize to UI thread ([1eeaf5a](https://github.com/mariamjensen42-glitch/LocalMusicPlayer/commit/1eeaf5a36e0f436011498824c7981dfe1dc9e2dc))


### ♻️ 重构

* mini mode is same window, not separate window ([db5ac31](https://github.com/mariamjensen42-glitch/LocalMusicPlayer/commit/db5ac31151cb407ff901bc78c4225a0dc3f774de))
* **视图:** 删除冗余视图和ViewModel，简化导航结构 ([a8170b4](https://github.com/mariamjensen42-glitch/LocalMusicPlayer/commit/a8170b4e8decda859177851365ee76e2d703417e))

## [1.4.0](https://github.com/mariamjensen42-glitch/LocalMusicPlayer/compare/v1.3.0...v1.4.0) (2026-04-13)


### ✨ 新功能

* add mini mode and smart playlist features ([97eedd8](https://github.com/mariamjensen42-glitch/LocalMusicPlayer/commit/97eedd861b55af21a3c01f468293660f31863359))


### 🐛 修复

* resolve build errors in smart playlist and mini mode ([08b2dbb](https://github.com/mariamjensen42-glitch/LocalMusicPlayer/commit/08b2dbb92324f87986cd065546d42d1278b45150))

## [1.3.0](https://github.com/mariamjensen42-glitch/LocalMusicPlayer/compare/v1.2.0...v1.3.0) (2026-04-13)


### ✨ 新功能

* 启用XAML编译绑定并重构视图定位器 ([44509d6](https://github.com/mariamjensen42-glitch/LocalMusicPlayer/commit/44509d6d8b13d4c2ffbaa3a501f5637430ffe85d))


### 🐛 修复

* 修复字符串资源重复键错误 ([44509d6](https://github.com/mariamjensen42-glitch/LocalMusicPlayer/commit/44509d6d8b13d4c2ffbaa3a501f5637430ffe85d))


### 📝 文档

* 添加代码优化和修复的规则文档 ([44509d6](https://github.com/mariamjensen42-glitch/LocalMusicPlayer/commit/44509d6d8b13d4c2ffbaa3a501f5637430ffe85d))


### 💄 代码样式

* 更新UI设计文件颜色和样式 ([44509d6](https://github.com/mariamjensen42-glitch/LocalMusicPlayer/commit/44509d6d8b13d4c2ffbaa3a501f5637430ffe85d))


### ♻️ 重构

* 优化MainWindowViewModel事件订阅管理 ([44509d6](https://github.com/mariamjensen42-glitch/LocalMusicPlayer/commit/44509d6d8b13d4c2ffbaa3a501f5637430ffe85d))


### 🧹 其他

* 添加构建验证步骤到CI工作流 ([44509d6](https://github.com/mariamjensen42-glitch/LocalMusicPlayer/commit/44509d6d8b13d4c2ffbaa3a501f5637430ffe85d))

## [1.2.0](https://github.com/mariamjensen42-glitch/LocalMusicPlayer/compare/v1.1.0...v1.2.0) (2026-04-13)


### ✨ 新功能

* add system tray, playlist import/export, history, and multi-select ([b0f1bca](https://github.com/mariamjensen42-glitch/LocalMusicPlayer/commit/b0f1bcabd233a663b276c1c014f25caaa5dac3b0))
* **core:** 添加用户播放列表及库分类功能支持 ([a9ced5e](https://github.com/mariamjensen42-glitch/LocalMusicPlayer/commit/a9ced5e0ea576f0b2acb23f8866147fcd68b60c1))
* **injection:** 集成歌词服务及相关MVVM支持 ([5dff4bc](https://github.com/mariamjensen42-glitch/LocalMusicPlayer/commit/5dff4bc3a3fb8148fb38e59351252cde24d8655d))
* **statistics:** 添加播放统计功能及统计页面 ([e2c4488](https://github.com/mariamjensen42-glitch/LocalMusicPlayer/commit/e2c448849219108a40e4e7007c93f7b329ef0758))
* **ui:** 调整主窗口标题栏布局并添加间距 ([0c02c88](https://github.com/mariamjensen42-glitch/LocalMusicPlayer/commit/0c02c8831424585fa81cd2919b21d506b77c7d22))
* **ui:** 重构主窗口界面并新增播放器样式和全局转换器 ([49dc241](https://github.com/mariamjensen42-glitch/LocalMusicPlayer/commit/49dc24117731bdaa89650c09755af69670f0e163))
* 初始化 Avalonia 音乐播放器项目基础结构 ([ab14477](https://github.com/mariamjensen42-glitch/LocalMusicPlayer/commit/ab1447773c91d7e443fc0013db3e8237ed342635))
* 实现音乐播放器核心功能与界面重构 ([2a27202](https://github.com/mariamjensen42-glitch/LocalMusicPlayer/commit/2a27202ad72b78ddddca86614154ec63564e322e))
* 新增播放历史、单曲循环、播放速度控制等功能 ([3f85092](https://github.com/mariamjensen42-glitch/LocalMusicPlayer/commit/3f8509299fef7494ed25a236f5546bd1752782ef))
* **歌词:** 集成QQ音乐在线歌词搜索功能 ([0f61f85](https://github.com/mariamjensen42-glitch/LocalMusicPlayer/commit/0f61f85ec5b4ec4593b65c9690292fc97052a041))
* 添加EF Core SQLite支持并重构服务层 ([b8f23f3](https://github.com/mariamjensen42-glitch/LocalMusicPlayer/commit/b8f23f3b8641315a6d2e3f574e8a32cf9ca16af8))
* 添加歌词搜索弹窗和逐字高亮功能 ([3c5f460](https://github.com/mariamjensen42-glitch/LocalMusicPlayer/commit/3c5f46077d94232e426b1a972b5b9c3f3ff9fbea))
* **视图:** 实现艺术家和专辑详情页的完整UI ([68db893](https://github.com/mariamjensen42-glitch/LocalMusicPlayer/commit/68db893e45c8ee32ce2caba84044136da85359cf))
* **设置:** 新增系统托盘和启动行为设置功能 ([9fdf210](https://github.com/mariamjensen42-glitch/LocalMusicPlayer/commit/9fdf210bdb0c10942926acb6ca15d245fce02f19))
* **音乐库:** 实现多维分类浏览和批量标签编辑功能 ([ef8f094](https://github.com/mariamjensen42-glitch/LocalMusicPlayer/commit/ef8f094ba8349dbd697501a953f208118dc68316))
* **音乐库管理:** 实现文件拖拽导入和自动监听功能 ([b6aa8da](https://github.com/mariamjensen42-glitch/LocalMusicPlayer/commit/b6aa8da9c5d9fb7335ddeda71756bd108af966e3))
* 音乐播放器功能改进 ([6106dcb](https://github.com/mariamjensen42-glitch/LocalMusicPlayer/commit/6106dcbf18e7bfe1dbb19dc34533411435582244))
* 音乐播放器功能改进 ([cfbaaec](https://github.com/mariamjensen42-glitch/LocalMusicPlayer/commit/cfbaaecde233c9df54f6a753c89c5ad887a94d70))


### 📝 文档

* **ci:** 更新CI/CD文档，集成release-please自动化发布系统 ([5c39c27](https://github.com/mariamjensen42-glitch/LocalMusicPlayer/commit/5c39c2705efbeac189e1c377f220e11624446c31))
* **dev-guide:** 添加LocalMusicPlayer开发指南文档 ([955ddef](https://github.com/mariamjensen42-glitch/LocalMusicPlayer/commit/955ddef95aa0a29242650c115fbdd895d3198b92))
* **tools:** 添加LocalMusicPlayer工具与辅助功能文档 ([955ddef](https://github.com/mariamjensen42-glitch/LocalMusicPlayer/commit/955ddef95aa0a29242650c115fbdd895d3198b92))
* 更新README中的描述文字 ([c5f013e](https://github.com/mariamjensen42-glitch/LocalMusicPlayer/commit/c5f013edfe85d389d75247418079023056700c52))
* 更新README中的项目描述 ([b0323a0](https://github.com/mariamjensen42-glitch/LocalMusicPlayer/commit/b0323a0c54d78287ea282a9cbf12cfaaa13ce15e))
* 更新README文档内容与项目结构信息 ([68db893](https://github.com/mariamjensen42-glitch/LocalMusicPlayer/commit/68db893e45c8ee32ce2caba84044136da85359cf))


### ♻️ 重构

* **播放器:** 分离音乐播放服务与音乐库服务 ([68db893](https://github.com/mariamjensen42-glitch/LocalMusicPlayer/commit/68db893e45c8ee32ce2caba84044136da85359cf))
* 重构项目架构并迁移至 CommunityToolkit.Mvvm ([e94e7ac](https://github.com/mariamjensen42-glitch/LocalMusicPlayer/commit/e94e7acd4932135147b7779de19fe0f673117a44))


### 🔨 构建

* **installer:** 添加Windows安装包构建和上传流程 ([9149d81](https://github.com/mariamjensen42-glitch/LocalMusicPlayer/commit/9149d814f4a5bbd881192fa0899760abe1f80510))


### 🔧 CI/CD

* 优化GitHub Actions配置，添加release-please全自动发布 ([643bc82](https://github.com/mariamjensen42-glitch/LocalMusicPlayer/commit/643bc82893a1555d0fa82bceddef8b5943af1bba))


### 🧹 其他

* **ci:** 添加完整的GitHub Actions工作流配置 ([9435b54](https://github.com/mariamjensen42-glitch/LocalMusicPlayer/commit/9435b5458daa6ba813cdde327c2be30e05d70ae0))
* **master:** release 1.0.0 ([d549cdb](https://github.com/mariamjensen42-glitch/LocalMusicPlayer/commit/d549cdbe8d35f48e9f4f8ed5c4ac15bcfcb0df1c))
* **master:** release 1.0.0 ([579ccd9](https://github.com/mariamjensen42-glitch/LocalMusicPlayer/commit/579ccd98f86f0860d1eb857f8434b376af45d91e))
* **master:** release 1.0.1 ([4dd1ab4](https://github.com/mariamjensen42-glitch/LocalMusicPlayer/commit/4dd1ab41f5559535871c2f64299ba919c1b68c3b))
* **master:** release 1.0.1 ([dc33afc](https://github.com/mariamjensen42-glitch/LocalMusicPlayer/commit/dc33afcb3879263dfd1d7cd3f3c81bcc67a3a5c8))
* **master:** release 1.1.0 ([6eb88bd](https://github.com/mariamjensen42-glitch/LocalMusicPlayer/commit/6eb88bd212f5f153080383d654bf0ea2a2cb13d6))
* **master:** release 1.1.0 ([b81eaf8](https://github.com/mariamjensen42-glitch/LocalMusicPlayer/commit/b81eaf8b8cd5780ef78c3f5d224c152aceb5eda8))
* 更新工作流配置 ([83f6378](https://github.com/mariamjensen42-glitch/LocalMusicPlayer/commit/83f63788500e0553f8bff8df14eb2640a572af91))

## [1.1.0](https://github.com/cycling02/LocalMusicPlayer/compare/v1.0.1...v1.1.0) (2026-04-05)


### ✨ 新功能

* **statistics:** 添加播放统计功能及统计页面 ([e2c4488](https://github.com/cycling02/LocalMusicPlayer/commit/e2c448849219108a40e4e7007c93f7b329ef0758))
* **ui:** 重构主窗口界面并新增播放器样式和全局转换器 ([49dc241](https://github.com/cycling02/LocalMusicPlayer/commit/49dc24117731bdaa89650c09755af69670f0e163))

## [1.0.1](https://github.com/cycling02/LocalMusicPlayer/compare/v1.0.0...v1.0.1) (2026-04-05)


### 📝 文档

* **ci:** 更新CI/CD文档，集成release-please自动化发布系统 ([5c39c27](https://github.com/cycling02/LocalMusicPlayer/commit/5c39c2705efbeac189e1c377f220e11624446c31))

## 1.0.0 (2026-04-05)


### ✨ 新功能

* 初始化 Avalonia 音乐播放器项目基础结构 ([ab14477](https://github.com/cycling02/LocalMusicPlayer/commit/ab1447773c91d7e443fc0013db3e8237ed342635))


### 📝 文档

* **dev-guide:** 添加LocalMusicPlayer开发指南文档 ([955ddef](https://github.com/cycling02/LocalMusicPlayer/commit/955ddef95aa0a29242650c115fbdd895d3198b92))
* **tools:** 添加LocalMusicPlayer工具与辅助功能文档 ([955ddef](https://github.com/cycling02/LocalMusicPlayer/commit/955ddef95aa0a29242650c115fbdd895d3198b92))


### 🔧 CI/CD

* 优化GitHub Actions配置，添加release-please全自动发布 ([643bc82](https://github.com/cycling02/LocalMusicPlayer/commit/643bc82893a1555d0fa82bceddef8b5943af1bba))


### 🧹 其他

* **ci:** 添加完整的GitHub Actions工作流配置 ([9435b54](https://github.com/cycling02/LocalMusicPlayer/commit/9435b5458daa6ba813cdde327c2be30e05d70ae0))
