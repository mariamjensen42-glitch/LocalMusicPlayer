# Bug Fixes - LocalMusicPlayer

## 2026-04-13 - Strings.axaml 重复键错误

### 问题描述
运行时抛出 `ArgumentException: An item with the same key has already been added. Key: StringNoSongs`

### 原因
在优化过程中，新增字符串资源时未检查是否已存在，导致 `Strings.axaml` 中出现重复键：

| 重复键 | 位置 |
|-------|------|
| `StringNoSongs` | 第 174 行, 第 181 行 |
| `StringSeconds` | 第 131 行, 第 195 行 |
| `StringAddToPlaylist` | 第 63 行, 第 189 行 |
| `StringRemove` | 第 28 行, 第 190 行 |

### 解决方案
删除重复的键定义，保留原有定义（较早出现的位置）

### 修复后的唯一键
- `StringNoSongs` - 暂无歌曲 (第 174 行)
- `StringSeconds` - 秒 (第 131 行)
- `StringAddToPlaylist` - 添加到播放列表 (第 63 行)
- `StringRemove` - 移除 (第 28 行)

### 状态
✅ 已修复 - 程序正常运行
