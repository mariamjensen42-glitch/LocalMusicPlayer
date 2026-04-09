# 单元测试规范

## 测试项目结构

```
LocalMusicPlayer.Tests/
├── Services/
│   ├── MusicScannerTests.cs
│   └── PlaylistServiceTests.cs
└── ViewModels/
    └── PlayerViewModelTests.cs
```

## 命名规范

```csharp
// 类名：被测类 + Tests
// 方法名：被测方法 + 场景 + 预期结果
public class MusicScannerTests
{
    [Fact]
    public async Task ScanDirectory_WithValidPath_ReturnsSongs()
    {
    }

    [Fact]
    public async Task ScanDirectory_WithEmptyPath_ReturnsEmpty()
    {
    }
}
```

## 测试结构（AAA 模式）

```csharp
[Fact]
public async Task ScanDirectory_WithValidPath_ReturnsSongs()
{
    // Arrange
    var scanner = new MusicScanner();
    var path = "/path/to/music";

    // Act
    var result = await scanner.ScanDirectoryAsync(path);

    // Assert
    Assert.NotEmpty(result);
}
```

## 测试规则

1. 每个测试只验证一个行为
2. 使用 Arrange-Act-Assert 结构
3. 避免测试间的依赖
4. 公共方法应有测试覆盖
