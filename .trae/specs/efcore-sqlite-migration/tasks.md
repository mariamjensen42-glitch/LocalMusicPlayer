# Tasks

- [x] Task 1: 补充 AppDbContext 实体和迁移
  - [x] 1.1: 添加 PlaylistEntity 和 PlaylistSongEntity 实体
  - [x] 1.2: 更新 OnModelCreating 配置新实体
  - [x] 1.3: 重新生成 EF Core 迁移

- [x] Task 2: 注册 DbContext 和 DatabaseService
  - [x] 2.1: 在 App.axaml.cs 中注册 AppDbContext 为 Singleton
  - [x] 2.2: 在 App.axaml.cs 中注册 IDatabaseService
  - [x] 2.3: 在 App.axaml.cs OnFrameworkInitializationCompleted 中调用 InitializeAsync

- [x] Task 3: 重写 ConfigurationService 使用 SQLite
  - [x] 3.1: 注入 AppDbContext
  - [x] 3.2: CurrentSettings 从数据库 Key-Value 表加载
  - [x] 3.3: SaveSettingsAsync 写入数据库
  - [x] 3.4: 添加 JSON 迁移逻辑（首次启动时从 settings.json 导入数据）

- [x] Task 4: 重写 UserPlaylistService 使用 SQLite
  - [x] 4.1: 注入 AppDbContext
  - [x] 4.2: AddToFavorites/RemoveFromFavorites 操作 FavoriteEntity
  - [x] 4.3: GetFavoriteSongs 从数据库查询
  - [x] 4.4: 播放列表 CRUD 操作 PlaylistEntity
  - [x] 4.5: 歌曲库变更时同步收藏状态

- [x] Task 5: 重写 PlayHistoryService 使用 SQLite
  - [x] 5.1: 注入 AppDbContext
  - [x] 5.2: AddToHistory 写入 PlayHistoryEntity
  - [x] 5.3: GetHistory 从数据库查询
  - [x] 5.4: ClearHistory 删除数据库记录
  - [x] 5.5: 限制最大历史记录数 200 条

- [x] Task 6: 验证和测试
  - [x] 6.1: 编译通过
  - [x] 6.2: 应用启动正常，数据库自动创建
  - [x] 6.3: 收藏功能正常，重启后保留
  - [x] 6.4: 播放历史功能正常，重启后保留
  - [x] 6.5: 设置功能正常，重启后保留

# Task Dependencies
- [Task 2] depends on [Task 1]
- [Task 3] depends on [Task 2]
- [Task 4] depends on [Task 2]
- [Task 5] depends on [Task 2]
- [Task 6] depends on [Task 3, Task 4, Task 5]
- Task 3, 4, 5 可并行执行
