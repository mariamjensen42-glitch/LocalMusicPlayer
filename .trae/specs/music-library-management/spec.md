# 音乐库管理增强功能规格说明

## Why

当前 LocalMusicPlayer 已具备基础音乐库管理功能（拖拽导入、文件监听、播放列表管理、元数据编辑），但缺少以下关键功能：

1. **多维分类浏览** - 无法按艺术家、专辑、流派等维度浏览曲库
2. **批量标签编辑** - 只能单首编辑，无法批量修改多首歌曲元数据
3. **文件管理** - 无法直接在应用内对音乐文件进行重命名、移动、删除
4. **封面管理** - 缺少专辑封面自动匹配和统一管理
5. **数据统计** - 没有播放统计和个人收听报告

## What Changes

### 新增功能
- **多维分类浏览**：支持按歌曲、艺术家、专辑、流派、文件夹维度浏览
- **批量标签编辑器**：支持多选歌曲批量编辑元数据
- **文件管理**：直接对本地文件重命名、复制、移动、删除
- **封面管理**：读取内嵌封面、自动匹配网络封面、手动设置封面
- **数据统计**：统计播放次数、时长，生成个人收听报告

### 新增组件
- **LibraryBrowserView / ViewModel**：多维分类浏览界面
- **BatchMetadataEditorView / ViewModel**：批量标签编辑器
- **FileManagerService / IFileManagerService**：文件管理服务
- **CoverManagerService / ICoverManagerService**：封面管理服务
- **StatisticsService / IStatisticsService**：统计服务
- **StatisticsReportView / ViewModel**：收听报告界面

## Impact

- Affected specs: 音乐库浏览、元数据编辑、文件管理
- Affected code:
  - Services/（新增 FileManagerService, CoverManagerService, StatisticsService）
  - ViewModels/（新增 LibraryBrowserViewModel, BatchMetadataEditorViewModel, StatisticsReportViewModel）
  - Views/（新增 LibraryBrowserView, BatchMetadataEditorView, StatisticsReportView）
  - Models/（Song 模型扩展播放统计字段）

## ADDED Requirements

### Requirement: 多维分类浏览

系统 SHALL 支持从多个维度浏览音乐库。

#### Scenario: 按歌曲浏览
- **WHEN** 用户选择"歌曲"分类
- **THEN** 显示所有歌曲列表，支持排序和搜索

#### Scenario: 按艺术家浏览
- **WHEN** 用户选择"艺术家"分类
- **THEN** 显示艺术家列表，点击艺术家显示其所有歌曲

#### Scenario: 按专辑浏览
- **WHEN** 用户选择"专辑"分类
- **THEN** 显示专辑列表（含封面），点击专辑显示专辑内歌曲

#### Scenario: 按流派浏览
- **WHEN** 用户选择"流派"分类
- **THEN** 显示流派列表，点击流派显示该流派歌曲

#### Scenario: 按文件夹浏览
- **WHEN** 用户选择"文件夹"分类
- **THEN** 显示文件夹树形结构，浏览各文件夹内歌曲

### Requirement: 批量标签编辑

系统 SHALL 支持批量编辑多首歌曲的元数据。

#### Scenario: 选择多首歌曲
- **WHEN** 用户在歌曲列表中选择多首歌曲（Ctrl/Shift+点击）
- **THEN** 右键菜单显示"批量编辑"选项

#### Scenario: 批量编辑共同字段
- **WHEN** 用户打开批量编辑器
- **THEN** 显示所有歌曲的共同字段（艺术家、专辑、流派等）
- **AND** 修改后应用到所有选中歌曲

#### Scenario: 保留不同字段
- **WHEN** 某字段在多首歌曲中值不同
- **THEN** 该字段显示为"<混合值>"或留空
- **AND** 用户不修改时保持原值不变

#### Scenario: 批量保存
- **WHEN** 用户确认批量编辑
- **THEN** 显示进度条，逐首保存修改
- **AND** 保存失败时记录错误并继续

### Requirement: 文件管理

系统 SHALL 支持直接管理本地音乐文件。

#### Scenario: 文件重命名
- **WHEN** 用户选择重命名文件
- **THEN** 根据元数据生成新文件名（如"艺术家 - 标题.mp3"）
- **AND** 支持自定义命名模板

#### Scenario: 文件移动
- **WHEN** 用户选择移动文件
- **THEN** 弹出目标文件夹选择对话框
- **AND** 移动文件并更新库中路径

#### Scenario: 文件复制
- **WHEN** 用户选择复制文件
- **THEN** 选择目标位置后复制文件
- **AND** 复制后的文件也加入音乐库

#### Scenario: 文件删除
- **WHEN** 用户选择删除文件
- **THEN** 弹出确认对话框
- **AND** 删除文件并从音乐库移除

#### Scenario: 批量文件操作
- **WHEN** 用户选择多首歌曲进行文件操作
- **THEN** 批量执行，显示进度和结果

### Requirement: 封面管理

系统 SHALL 支持专辑封面的读取、自动匹配和手动设置。

#### Scenario: 读取内嵌封面
- **WHEN** 歌曲文件包含内嵌封面
- **THEN** 自动读取并显示

#### Scenario: 自动匹配封面
- **WHEN** 歌曲无内嵌封面
- **THEN** 根据艺术家和专辑名从网络搜索匹配封面
- **AND** 提供多个候选供用户选择

#### Scenario: 手动设置封面
- **WHEN** 用户点击更换封面
- **THEN** 支持从本地选择图片文件
- **AND** 支持从剪贴板粘贴图片

#### Scenario: 封面嵌入文件
- **WHEN** 用户确认封面
- **THEN** 将封面嵌入音频文件元数据

#### Scenario: 封面缓存
- **WHEN** 封面下载或设置后
- **THEN** 本地缓存封面图片
- **AND** 下次加载时优先使用缓存

### Requirement: 数据统计

系统 SHALL 统计播放数据并生成收听报告。

#### Scenario: 播放次数统计
- **WHEN** 歌曲播放完成（超过50%时长）
- **THEN** 增加该歌曲播放次数
- **AND** 记录最后播放时间

#### Scenario: 播放时长统计
- **WHEN** 用户播放歌曲
- **THEN** 累计总播放时长
- **AND** 按日/周/月统计收听时长

#### Scenario: 生成收听报告
- **WHEN** 用户打开统计页面
- **THEN** 显示：
  - 总歌曲数、总时长
  - 最常播放的艺术家/专辑/歌曲
  - 收听趋势图表
  - 流派分布

#### Scenario: 收听历史
- **WHEN** 用户查看历史
- **THEN** 显示最近播放记录
- **AND** 支持按日期筛选

## MODIFIED Requirements

### Requirement: Song 模型扩展

在现有 Song 模型基础上增加：

| 字段 | 类型 | 说明 |
|------|------|------|
| `PlayCount` | int | 播放次数 |
| `LastPlayedAt` | DateTime? | 最后播放时间 |
| `AddedAt` | DateTime | 添加到库时间 |

### Requirement: IMusicLibraryService 扩展

增加按维度查询方法：

| 方法 | 说明 |
|------|------|
| `GetArtists()` | 获取所有艺术家列表 |
| `GetAlbums()` | 获取所有专辑列表 |
| `GetGenres()` | 获取所有流派列表 |
| `GetSongsByArtist(string artist)` | 获取指定艺术家歌曲 |
| `GetSongsByAlbum(string album)` | 获取指定专辑歌曲 |
| `GetSongsByGenre(string genre)` | 获取指定流派歌曲 |

## REMOVED Requirements

无

## 技术实现约束

### 依赖注入
- 新服务通过接口注入
- ViewModel 依赖服务接口而非具体实现

### 线程安全
- 文件操作在后台线程执行
- UI 更新通过 Dispatcher 切换到主线程
- 批量操作显示进度指示，支持取消

### 错误处理
- 文件操作错误优雅降级并记录日志
- 网络封面获取失败时使用默认封面
- 统计服务异常不影响播放功能

### UI 约束
- 使用 x:DataType 编译期绑定
- XAML 中不硬编码中文字符串（使用资源文件）
- 列表使用虚拟化支持大数据量
