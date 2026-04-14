---
alwaysApply: false
description: 
---
# 命名规范

## C#

| 类型 | 规则 | 示例 |
|------|------|------|
| 类/接口 | PascalCase | `SongService`, `IMusicPlayer` |
| 方法 | PascalCase | `PlayMusic()`, `ScanDirectory()` |
| 属性/字段 | PascalCase | `SongTitle`, `IsPlaying` |
| 私有字段 | _camelCase | `_currentSong`, `_playlist` |
| 常量 | PascalCase | `MaxVolume`, `DefaultPath` |
| 枚举值 | PascalCase | `PlayState.Playing` |

## XAML

| 类型 | 规则 | 示例 |
|------|------|------|
| 控件命名 | PascalCase | `PlayButton`, `SongList` |
| 资源键 | PascalCase | `PrimaryBrush`, `PlayButtonStyle` |
| 文件名 | PascalCase | `MainWindow.axaml`, `Resources.axaml` |
