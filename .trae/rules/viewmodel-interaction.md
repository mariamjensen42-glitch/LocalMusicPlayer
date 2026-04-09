# ViewModel 交互规范

## ViewModel 职责

- 仅处理业务逻辑和状态
- 不直接操作 UI 控件
- 通过命令响应用户操作
- 通过属性通知 View 更新

## 命令定义

```csharp
// 使用 ReactiveUI 命令
public ReactiveCommand<Unit, Unit> PlayCommand { get; }
public ReactiveCommand<Song, Unit> SelectSongCommand { get; }

// 构造命令
PlayCommand = ReactiveCommand.CreateFromTask(PlayAsync);
SelectSongCommand = ReactiveCommand.CreateFromTask<Song>(SelectSongAsync);
```

## 属性变更通知

```csharp
private string _currentSongTitle;
public string CurrentSongTitle
{
    get => _currentSongTitle;
    set => this.RaiseAndSetIfChanged(ref _currentSongTitle, value);
}
```

## ViewModel 通信

- 父子 ViewModel 通过构造函数传递
- 兄弟 ViewModel 通过消息总线
- 使用 `MessageBus` 解耦

## 禁止事项

- 禁止在 ViewModel 中使用 Dispatcher
- 禁止直接操作 View 控件
- 禁止在 ViewModel 中 new ViewModel
