# Checklist - 音乐库管理增强功能

## 接口定义

- [x] IFileManagerService 接口定义完成（Rename, Move, Copy, Delete, BatchOperations）
- [x] ICoverManagerService 接口定义完成（Extract, Cache, SetFromFile, Embed）
- [x] IStatisticsService 接口定义完成（TrackPlay, GetReport, GetHistory）
- [x] IMusicLibraryService 扩展完成（GetArtists, GetAlbums, GetGenres 等）

## 服务实现

- [x] FileManagerService 实现：
  - [x] 单文件重命名（支持模板）
  - [x] 单文件移动
  - [x] 单文件复制
  - [x] 单文件删除
  - [x] 批量操作带进度报告
  - [x] 线程安全（后台执行）

- [x] CoverManagerService 实现：
  - [x] 读取内嵌封面
  - [x] 本地缓存机制
  - [x] 从本地文件设置封面
  - [x] 封面嵌入音频文件

- [x] StatisticsService 实现：
  - [x] 播放次数统计
  - [x] 播放时长统计
  - [x] 数据持久化
  - [x] 报告生成

## 模型扩展

- [x] Song 模型扩展：
  - [x] PlayCount 字段
  - [x] LastPlayedAt 字段
  - [x] AddedAt 字段

## ViewModel 实现

- [x] LibraryBrowserViewModel：
  - [x] 分类切换逻辑
  - [x] 艺术家列表
  - [x] 专辑列表（含封面）
  - [x] 流派列表
  - [x] 文件夹树形浏览
  - [x] 搜索和排序

- [x] BatchMetadataEditorViewModel：
  - [x] 多选歌曲处理
  - [x] 共同字段检测
  - [x] 批量保存逻辑
  - [x] 进度报告

- [x] StatisticsReportViewModel：
  - [x] 总览统计数据
  - [x] 排行数据
  - [x] 趋势图表数据
  - [x] 流派分布数据
  - [x] 收听历史

## View 实现

- [x] LibraryBrowserView：
  - [x] 分类切换 Tab/Sidebar
  - [x] 艺术家列表视图
  - [x] 专辑网格视图（含封面）
  - [x] 流派列表视图
  - [x] 文件夹树形视图
  - [x] 歌曲列表视图

- [x] BatchMetadataEditorView：
  - [x] 表单布局
  - [x] 混合值显示
  - [x] 进度条
  - [x] 保存/取消按钮

- [x] StatisticsReportView：
  - [x] 总览卡片
  - [x] 排行榜
  - [x] 趋势图表
  - [x] 流派分布图
  - [x] 历史记录列表

## UI 集成

- [x] 主界面导航入口
- [x] 音乐库页面集成
- [x] 统计报告页面集成
- [x] 播放统计触发集成
- [x] 右键菜单批量编辑选项

## 代码质量

- [x] 服务接口命名符合规范
- [x] 所有类/接口使用 PascalCase
- [x] 私有字段使用 _camelCase
- [x] 无硬编码中文字符串
- [x] 使用 x:DataType 编译期绑定
- [x] 列表使用虚拟化

## 编译验证

- [x] dotnet build 成功
- [x] 无编译警告
