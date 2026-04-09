# CommunityToolkit.Mvvm 重构任务列表

## 任务 1: 更新项目依赖

- [x] 1.1 移除 ReactiveUI 包引用
  - 从 LocalMusicPlayer.csproj 删除 `ReactiveUI.Avalonia` 引用
  - 从 LocalMusicPlayer.csproj 删除 `ReactiveUI.SourceGenerators` 引用

- [x] 1.2 添加 CommunityToolkit.Mvvm 包引用
  - 添加 `CommunityToolkit.Mvvm` version 8.3.2

- [x] 1.3 验证依赖更新
  - 运行 `dotnet restore` 确保包正确安装

## 任务 2: 重构 ViewModelBase

- [x] 2.1 修改基类继承
  - 从 `ReactiveObject` 改为 `ObservableObject`
  - 添加 `partial` 修饰符
  - 移除不必要的 using

- [x] 2.2 验证基类重构
  - 确保所有继承 ViewModelBase 的类仍能编译

## 任务 3: 重构 MainWindowViewModel

- [x] 3.1 转换属性为 ObservableProperty
  - `CurrentPage` → `[ObservableProperty] private ViewModelBase _currentPage;`
  - `LibraryStats` → `[ObservableProperty] private string _libraryStats;`
  - `CurrentPlaylist` → `[ObservableProperty] private Playlist? _currentPlaylist;`
  - `SearchText` → `[ObservableProperty] private string _searchText;`
  - `Volume` → `[ObservableProperty] private int _volume;`
  - `IsMuted` → `[ObservableProperty] private bool _isMuted;`
  - 添加 `[NotifyPropertyChangedFor]` 用于计算属性

- [x] 3.2 转换命令为 RelayCommand
  - `PlayCommand` → `[RelayCommand] private void Play()`
  - `PauseCommand` → `[RelayCommand] private void Pause()`
  - `StopCommand` → `[RelayCommand] private void Stop()`
  - `NextCommand` → `[RelayCommand] private void Next()`
  - `PreviousCommand` → `[RelayCommand] private void Previous()`
  - `MuteCommand` → `[RelayCommand] private void Mute()`
  - `ShuffleCommand` → `[RelayCommand] private void Shuffle()`
  - `RepeatCommand` → `[RelayCommand] private void Repeat()`
  - `PlaySongCommand` → `[RelayCommand] private void PlaySong(string path)`
  - `ToggleFavoriteCommand` → `[RelayCommand] private void ToggleFavorite(string path)`
  - `NavigateToSettingsCommand` → `[RelayCommand] private void NavigateToSettings()`
  - `NavigateToLibraryCommand` → `[RelayCommand] private void NavigateToLibrary()`
  - `NavigateToPlayerCommand` → `[RelayCommand] private void NavigateToPlayer()`
  - `NavigateToStatisticsCommand` → `[RelayCommand] private void NavigateToStatistics()`
  - `NavigateToPlaylistCommand` → `[RelayCommand] private void NavigateToPlaylist()`
  - `NavigateToCategoryCommand` → `[RelayCommand] private void NavigateToCategory()`
  - `ToggleQueuePanelCommand` → `[RelayCommand] private void ToggleQueuePanel()`
  - `PlayAllCommand` → `[RelayCommand] private void PlayAll()`

- [x] 3.3 替换 Observable.Interval 为 DispatcherTimer
  - 创建 `_positionTimer` DispatcherTimer
  - 在构造函数中初始化并启动定时器
  - 定时器 Tick 事件中更新 Position/Duration 属性

- [x] 3.4 替换 WhenAnyValue 订阅
  - 移除 `this.WhenAnyValue(x => x.Volume).Subscribe(...)`
  - 改用 partial method 或直接更新

- [x] 3.5 更新事件处理
  - `_playbackStateService.PlaybackStateChanged` 事件处理改用 `OnPropertyChanged`
  - `_playbackStateService.CurrentSongChanged` 事件处理改用 `OnPropertyChanged`
  - `_navigationService.QueuePanelChanged` 事件处理改用 `OnPropertyChanged`

- [x] 3.6 移除 ReactiveUI using
  - 移除 `using ReactiveUI;`
  - 移除 `using System.Reactive;`
  - 移除 `using System.Reactive.Linq;`

## 任务 4: 重构 PlayerPageViewModel

- [x] 4.1 转换属性为 ObservableProperty
  - `Lyrics` → `[ObservableProperty] private ObservableCollection<LyricLine> _lyrics;`
  - `CurrentLyricIndex` → `[ObservableProperty] private int _currentLyricIndex;`
  - `HasLyrics` → `[ObservableProperty] private bool _hasLyrics;`
  - 添加 `[NotifyPropertyChangedFor]` 用于计算属性

- [x] 4.2 转换命令为 RelayCommand
  - `PlayCommand` → `[RelayCommand] private void Play()`
  - `PauseCommand` → `[RelayCommand] private void Pause()`
  - `NextCommand` → `[RelayCommand] private void Next()`
  - `PreviousCommand` → `[RelayCommand] private void Previous()`
  - `ShuffleCommand` → `[RelayCommand] private void Shuffle()`
  - `RepeatCommand` → `[RelayCommand] private void Repeat()`
  - `ToggleFavoriteCommand` → `[RelayCommand] private void ToggleFavorite()`
  - `NavigateBackCommand` → `[RelayCommand] private void NavigateBack()`
  - `ToggleMuteCommand` → `[RelayCommand] private void ToggleMute()`
  - `ToggleQueuePanelCommand` → `[RelayCommand] private void ToggleQueuePanel()`

- [x] 4.3 替换 Observable.Interval 为 DispatcherTimer
  - 创建位置更新定时器

- [x] 4.4 更新事件处理
  - 改用 `OnPropertyChanged`

## 任务 5: 重构 QueueViewModel

- [x] 5.1 转换属性为 ObservableProperty
  - 如有需要添加属性

- [x] 5.2 转换命令为 RelayCommand
  - `ClosePanelCommand` → `[RelayCommand] private void ClosePanel()`
  - `RemoveSongCommand` → `[RelayCommand] private void RemoveSong(int index)`
  - `ClearQueueCommand` → `[RelayCommand] private void ClearQueue()`
  - `PlaySongCommand` → `[RelayCommand] private void PlaySong(Song song)`

- [x] 5.3 更新事件处理
  - 改用 `OnPropertyChanged`

## 任务 6: 重构其他 ViewModel

- [x] 6.1 重构 PlaylistManagementViewModel
  - 转换属性和命令为 MVVM Toolkit 特性

- [x] 6.2 重构 LibraryCategoryViewModel
  - 转换属性和命令为 MVVM Toolkit 特性

- [x] 6.3 重构 StatisticsViewModel
  - 转换属性和命令为 MVVM Toolkit 特性

- [x] 6.4 重构 SettingsViewModel
  - 转换属性和命令为 MVVM Toolkit 特性

- [x] 6.5 重构 MetadataEditorViewModel
  - 转换属性和命令为 MVVM Toolkit 特性

## 任务 7: 验证和测试

- [x] 7.1 编译验证
  - 运行 `dotnet build` 确保无编译错误
  - 修复所有编译错误

- [ ] 7.2 功能验证
  - 播放/暂停/停止功能
  - 导航功能
  - 队列功能
  - 播放列表管理功能

- [ ] 7.3 回归测试
  - 确保原有功能未受影响

# 任务依赖关系

```
任务 1 (更新依赖) ──> 任务 2 (ViewModelBase)
                          │
                          v
                    任务 3 (MainWindowViewModel)
                          │
                          v
                    任务 4 (PlayerPageViewModel)
                          │
                          v
                    任务 5 (QueueViewModel)
                          │
                          v
                    任务 6 (其他 ViewModel)
                          │
                          v
                    任务 7 (验证测试)
```
