# Tasks - 音乐库管理增强功能

## 依赖关系
- Task 1、2、3 可并行执行（基础服务）
- Task 4 依赖 Task 1、2
- Task 5 依赖 Task 1
- Task 6 依赖 Task 1、3
- Task 7 依赖 Task 4、5、6
- Task 8 为最终验证

## 任务列表

- [x] Task 1: 创建文件管理服务 IFileManagerService 和 FileManagerService
  - [ ] 创建 Services/IFileManagerService.cs 接口
  - [ ] 创建 Services/FileManagerService.cs 实现
  - [ ] 实现文件重命名（支持命名模板）
  - [ ] 实现文件移动功能
  - [ ] 实现文件复制功能
  - [ ] 实现文件删除功能
  - [ ] 实现批量文件操作（带进度报告）

- [x] Task 2: 创建封面管理服务 ICoverManagerService 和 CoverManagerService
  - [ ] 创建 Services/ICoverManagerService.cs 接口
  - [ ] 创建 Services/CoverManagerService.cs 实现
  - [ ] 实现读取音频文件内嵌封面
  - [ ] 实现封面本地缓存机制
  - [ ] 实现从本地文件设置封面
  - [ ] 实现封面嵌入音频文件功能

- [x] Task 3: 创建统计服务 IStatisticsService 和 StatisticsService
  - [ ] 创建 Services/IStatisticsService.cs 接口
  - [ ] 创建 Services/StatisticsService.cs 实现
  - [ ] 扩展 Song 模型（PlayCount, LastPlayedAt, AddedAt）
  - [ ] 实现播放次数统计
  - [ ] 实现播放时长统计
  - [ ] 实现收听数据持久化（JSON/数据库）

- [x] Task 4: 实现多维分类浏览功能
  - [ ] 扩展 IMusicLibraryService 接口（GetArtists, GetAlbums, GetGenres 等）
  - [ ] 创建 ViewModels/LibraryBrowserViewModel.cs
  - [ ] 创建 Views/LibraryBrowserView.axaml 和 .cs
  - [ ] 实现分类切换（歌曲/艺术家/专辑/流派/文件夹）
  - [ ] 实现艺术家列表视图
  - [ ] 实现专辑列表视图（含封面）
  - [ ] 实现流派列表视图
  - [ ] 实现文件夹树形浏览

- [x] Task 5: 实现批量标签编辑功能
  - [ ] 创建 ViewModels/BatchMetadataEditorViewModel.cs
  - [ ] 创建 Views/BatchMetadataEditorView.axaml 和 .cs
  - [ ] 实现多选歌曲支持
  - [ ] 实现共同字段检测和显示
  - [ ] 实现批量保存（带进度条）
  - [ ] 在歌曲列表右键菜单添加"批量编辑"选项

- [x] Task 6: 实现数据统计和报告功能
  - [ ] 创建 ViewModels/StatisticsReportViewModel.cs
  - [ ] 创建 Views/StatisticsReportView.axaml 和 .cs
  - [ ] 实现总览统计（歌曲数、总时长、播放次数）
  - [ ] 实现最常播放排行（艺术家/专辑/歌曲）
  - [ ] 实现收听趋势图表
  - [ ] 实现流派分布统计
  - [ ] 实现收听历史记录

- [x] Task 7: UI 集成与导航
  - [ ] 在主界面添加"音乐库"导航入口
  - [ ] 在主界面添加"统计报告"导航入口
  - [ ] 集成 LibraryBrowserView 到主窗口
  - [ ] 集成 StatisticsReportView 到主窗口
  - [ ] 在播放界面集成播放统计触发

- [x] Task 8: 编译验证与测试
  - [ ] dotnet build 成功
  - [ ] 无编译警告
  - [ ] 验证多维浏览功能正常
  - [ ] 验证批量编辑功能正常
  - [ ] 验证文件管理功能正常
  - [ ] 验证统计功能正常

# Task Dependencies
- Task 4 依赖 Task 1、2
- Task 5 依赖 Task 1
- Task 6 依赖 Task 1、3
- Task 7 依赖 Task 4、5、6
