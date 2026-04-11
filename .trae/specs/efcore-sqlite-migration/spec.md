# EF Core + SQLite 持久化迁移 Spec

## Why
当前应用使用 JSON 文件（settings.json）存储收藏、播放历史和应用设置，存在数据丢失风险（异步保存未等待完成）和性能问题（全量读写）。引入 EF Core + SQLite 实现可靠的持久化存储。

## What Changes
- **BREAKING**: `IConfigurationService` 不再使用 JSON 文件存储，改为 SQLite
- **BREAKING**: `IUserPlaylistService` 收藏和播放列表存储改为 SQLite
- **BREAKING**: `IPlayHistoryService` 播放历史存储改为 SQLite
- 新增 `AppDbContext` 数据库上下文（已创建）
- 新增 `DatabaseService` 数据库初始化服务（已创建）
- 新增数据库实体：`SongEntity`、`FavoriteEntity`、`PlayHistoryEntity`、`AppSettingsEntity`、`PlaylistEntity`、`PlaylistSongEntity`（已创建部分）
- 修改 `App.axaml.cs` 注册 DbContext 和 DatabaseService
- 修改 `ConfigurationService` 使用 SQLite 存储设置
- 修改 `UserPlaylistService` 使用 SQLite 存储收藏和播放列表
- 修改 `PlayHistoryService` 使用 SQLite 存储播放历史

## Impact
- Affected specs: core-features（配置服务、播放列表服务、播放历史服务）
- Affected code:
  - `Data/AppDbContext.cs`（补充 PlaylistEntity）
  - `Services/ConfigurationService.cs`（重写为数据库存储）
  - `Services/UserPlaylistService.cs`（重写收藏和播放列表存储）
  - `Services/PlayHistoryService.cs`（重写播放历史存储）
  - `App.axaml.cs`（注册 DbContext 和 DatabaseService）

## ADDED Requirements

### Requirement: 数据库初始化
系统 SHALL 在应用启动时自动创建/迁移 SQLite 数据库。

#### Scenario: 首次启动
- **WHEN** 应用首次启动，数据库文件不存在
- **THEN** 自动创建数据库文件并应用所有迁移

#### Scenario: 版本升级
- **WHEN** 应用升级后启动，存在待应用的迁移
- **THEN** 自动应用新迁移，保留已有数据

### Requirement: 收藏持久化
系统 SHALL 通过 SQLite 持久化收藏数据。

#### Scenario: 添加收藏
- **WHEN** 用户点击收藏按钮
- **THEN** 收藏记录立即写入 SQLite 数据库，不阻塞 UI

#### Scenario: 应用重启后恢复收藏
- **WHEN** 应用重启后加载歌曲库
- **THEN** 从数据库读取收藏列表，同步到 Song.IsFavorite 状态

### Requirement: 播放历史持久化
系统 SHALL 通过 SQLite 持久化播放历史。

#### Scenario: 播放歌曲记录历史
- **WHEN** 用户播放一首歌曲
- **THEN** 播放记录立即写入 SQLite 数据库，不阻塞 UI

#### Scenario: 应用重启后恢复历史
- **WHEN** 应用重启后查看播放历史
- **THEN** 从数据库读取播放历史记录

### Requirement: 应用设置持久化
系统 SHALL 通过 SQLite 持久化应用设置（Key-Value 模式）。

#### Scenario: 保存设置
- **WHEN** 修改应用设置（音量、主题等）
- **THEN** 设置立即写入 SQLite 数据库

#### Scenario: 加载设置
- **WHEN** 应用启动时
- **THEN** 从数据库读取设置，若无记录则使用默认值

### Requirement: 播放列表持久化
系统 SHALL 通过 SQLite 持久化用户播放列表。

#### Scenario: 创建/修改播放列表
- **WHEN** 用户创建或修改播放列表
- **THEN** 播放列表数据立即写入 SQLite 数据库

## MODIFIED Requirements

### Requirement: IConfigurationService
`IConfigurationService` 接口保持不变，实现改为使用 SQLite。`CurrentSettings` 属性从数据库加载，`SaveSettingsAsync` 写入数据库。

### Requirement: IUserPlaylistService
`IUserPlaylistService` 接口保持不变，收藏和播放列表存储改为 SQLite。`AddToFavorites`/`RemoveFromFavorites` 直接操作数据库。

### Requirement: IPlayHistoryService
`IPlayHistoryService` 接口保持不变，播放历史存储改为 SQLite。`AddToHistory` 直接操作数据库。

## REMOVED Requirements

### Requirement: JSON 文件存储
**Reason**: 替换为 SQLite 数据库存储
**Migration**: 首次启动时检测 settings.json，将数据迁移到 SQLite 后删除 JSON 文件
