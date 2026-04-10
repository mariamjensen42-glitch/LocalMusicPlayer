---
alwaysApply: false
description: 
---
# 错误处理规范

## 异常处理原则

1. **捕获具体异常** - 避免空的 catch 块
2. **及时处理** - 不要吞掉异常而不做任何事
3. **记录日志** - 异常应记录到日志系统
4. **向上传递** - 无法处理的异常应 throws

## 服务层异常

- 业务异常使用自定义异常类
- 封装底层异常时保留原始异常
- 对外接口返回统一的错误响应

## ViewModel 异常处理

```csharp
try
{
    await _musicService.PlayAsync();
}
catch (PlayerException ex)
{
    Log.Error(ex);
    NotificationError(ex.Message);
}
```

## 禁止事项

- 禁止使用 `catch (Exception)` 而不做处理
- 禁止在 UI 层直接暴露异常信息
- 禁止在循环内捕获异常
