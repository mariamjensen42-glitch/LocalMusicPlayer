---
alwaysApply: false
description: 
---
# 日志规范

## 日志级别

| 级别 | 使用场景 |
|------|----------|
| `Debug` | 开发调试信息 |
| `Info` | 正常业务流程 |
| `Warning` | 潜在问题但不影响功能 |
| `Error` | 操作失败需要调查 |
| `Fatal` | 系统级严重错误 |

## 记录要求

### 必须记录
- 用户登录/登出
- 播放操作开始和结束
- 扫描音乐文件
- 异常和错误

### 不记录
- 调试用的临时日志
- 敏感信息（密码、Token）
- 大数据量的请求/响应

## 格式

```
[时间戳] [级别] [模块名] 消息内容
```

## 示例

```csharp
Log.Info("[MusicScanner] 开始扫描目录: {Path}", path);
Log.Error("[Player] 播放失败: {Error}", ex.Message);
```
