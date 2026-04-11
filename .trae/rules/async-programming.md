---
alwaysApply: true
---
# 异步编程规范

## 基本规则

1. 方法名以 `Async` 结尾
2. 返回 `Task` 或 `Task<T>`
3. 使用 `await` 等待异步操作
4. 不要在同步方法中调用异步方法

## 正确示例

```csharp
public async Task PlayAsync()
{
    await _player.InitializeAsync();
    await _player.SetPlaylistAsync(_playlist);
    await _player.PlayAsync();
}
```

## 错误示例

```csharp
// 错误：没有 async 却返回 Task
public Task PlayAsync()
{
    return _player.PlayAsync();
}

// 错误：同步方法调用异步
public void LoadPlaylist()
{
    _playlist.LoadAsync().Wait();
}
```

## ConfigureAwait

- 库代码使用 `ConfigureAwait(false)`
- UI 代码不使用 `ConfigureAwait`
- 保持一致性

## 异常处理

```csharp
try
{
    await _service.OperationAsync();
}
catch (OperationException ex)
{
    Log.Error(ex);
    throw;
}
```
