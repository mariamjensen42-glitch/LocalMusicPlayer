# 集成 QQ音乐在线歌词搜索计划

## 目标

将 Lyricify.Lyrics.Helper 库中的 QQ音乐歌词搜索功能**直接复制代码**到 LocalMusicPlayer 项目中，实现从 QQ音乐 获取在线歌词。

## 当前状态

* 项目使用 .NET 9.0 + Avalonia + CommunityToolkit.Mvvm

* 现有 `LyricsService` 仅支持读取本地歌词文件

* 需要添加在线歌词搜索功能

## 集成步骤

### 1. 添加 NuGet 包依赖

在 `LocalMusicPlayer.csproj` 中添加 Lyricify 依赖的包：

```xml
<PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
<PackageReference Include="SharpZipLib" Version="1.4.2" />
```

### 2. 创建项目目录结构

在项目中创建以下目录：

```
Services/OnlineLyrics/
  - QQMusic/
    - Api.cs (改造自 Lyricify 的 QQMusic Api)
    - Response.cs
Searchers/
  - QQMusicSearchResult.cs
  - QQMusicSearcher.cs
Models/
  - TrackMetadata.cs (实现 ITrackMetadata)
```

### 3. 复制并改造核心代码

#### 3.1 BaseApi.cs → Services/OnlineLyrics/BaseApi.cs

* HTTP 请求基类

* 处理 JSON 序列化和反序列化（改用 System.Text.Json 或保留 Newtonsoft.Json）

#### 3.2 QQMusic/Api.cs → Services/OnlineLyrics/QQMusic/Api.cs

* 核心搜索方法 `Search()`

* 歌词获取方法 `GetLyric()` 和 `GetLyricsAsync()`

* 需适配项目风格

#### 3.3 QQMusicSearcher.cs → Searchers/QQMusicSearcher.cs

* 搜索歌曲的实现

* 构造搜索字符串的逻辑

#### 3.4 TrackMetadata 实现

* 创建 `TrackMetadata.cs` 实现 `ITrackMetadata` 接口

* 从现有 `Song` 模型映射

### 4. 创建在线歌词服务

创建 `IOnlineLyricsService.cs` 和 `OnlineLyricsService.cs`：

```csharp
public interface IOnlineLyricsService
{
    Task<LyricSearchResult?> SearchLyricsAsync(Song song);
}

public class OnlineLyricsService : IOnlineLyricsService
{
    private readonly QQMusicSearcher _searcher;
    private readonly QQMusic.Api _api;

    public async Task<LyricSearchResult?> SearchLyricsAsync(Song song)
    {
        // 1. 使用 Searcher 搜索歌曲
        // 2. 获取第一个搜索结果
        // 3. 调用 Api 获取歌词
        // 4. 转换为项目格式返回
    }
}
```

### 5. 扩展现有 LyricsService

在 `Services/Media/LyricsService.cs` 中添加：

```csharp
public async Task<List<LyricLine>> GetLyricsAsync(Song song)
{
    // 先尝试本地歌词
    var localLyrics = GetLyrics(song.FilePath);
    if (localLyrics.Count > 0)
        return localLyrics;

    // 本地没有，尝试在线歌词
    var onlineService = new OnlineLyricsService();
    var onlineLyrics = await onlineService.SearchLyricsAsync(song);
    if (onlineLyrics != null)
        return onlineLyrics.Lyrics;

    return new List<LyricLine>();
}
```

### 6. 在 DI 容器中注册服务

在 `Helpers/ServiceCollectionExtensions.cs` 中添加：

```csharp
services.AddSingleton<IOnlineLyricsService, OnlineLyricsService>();
```

### 7. 在 ViewModel 中使用

在 `PlayerPageViewModel.cs` 中：

* 添加 `SearchOnlineLyricsCommand` 命令

* 添加歌词加载状态属性

* 绑定到 UI 按钮

## 需要复制的文件清单

| 源文件                                 | 目标路径                                        | 改造说明     |
| ----------------------------------- | ------------------------------------------- | -------- |
| `Providers/Web/BaseApi.cs`          | `Services/OnlineLyrics/BaseApi.cs`          | JSON 库适配 |
| `Providers/Web/QQMusic/Api.cs`      | `Services/OnlineLyrics/QQMusic/Api.cs`      | 核心 API   |
| `Providers/Web/QQMusic/Response.cs` | `Services/OnlineLyrics/QQMusic/Response.cs` | 响应模型     |
| `Searchers/QQMusicSearcher.cs`      | `Searchers/QQMusicSearcher.cs`              | 搜索实现     |
| `Searchers/QQMusicSearchResult.cs`  | `Searchers/QQMusicSearchResult.cs`          | 结果类      |
| `Searchers/ISearcher.cs`            | `Searchers/ISearcher.cs`                    | 接口       |
| `Models/ITrackMetadata.cs`          | `Models/ITrackMetadata.cs`                  | 接口       |

## 注意事项

1. **JSON 库选择**：Lyricify 使用 Newtonsoft.Json，项目使用 System.Text.Json，建议统一使用 Newtonsoft.Json 以避免兼容问题
2. **命名空间**：所有复制的代码使用项目统一的命名空间 `LocalMusicPlayer`
3. **异步规范**：方法名以 `Async` 结尾
4. **QQ音乐 API**：可能需要处理跨域问题，BaseApi 中有 Proxy 配置可参考
5. **歌词格式**：Lyricify 返回的歌词需转换为项目现有的 `LyricLine` 格式

