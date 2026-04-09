# 依赖注入规范

## 服务注册

```csharp
// Services/ServiceCollectionExtensions.cs
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMusicServices(this IServiceCollection services)
    {
        services.AddSingleton<IMusicPlayer, LibVLCPlayer>();
        services.AddSingleton<IMusicScanner, MusicScanner>();
        services.AddSingleton<IPlaylistService, PlaylistService>();
        return services;
    }
}
```

## 生命周期

| 生命周期 | 使用场景 |
|---------|---------|
| `Singleton` | 无状态服务、播放器、扫描器 |
| `Scoped` | 有状态但需隔离的服务 |
| `Transient` | 每次请求新建的服务 |

## ViewModel 注入

```csharp
public partial class MainWindowViewModel : ReactiveObject
{
    public MainWindowViewModel(IMusicPlayer player, IPlaylistService playlist)
    {
        _player = player;
        _playlist = playlist;
    }
}
```

## 规则

1. 服务接口放在 Services/ 目录
2. 实现类 internal，对外暴露接口
3. ViewModel 通过构造函数注入
4. 避免 service locator 模式
