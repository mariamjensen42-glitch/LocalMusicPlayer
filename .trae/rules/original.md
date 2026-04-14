---
alwaysApply: false
---
# original-sound-hq-player-win2d-navi 项目结构索引

## 目录结构

| 路径 | 功能 |
|------|------|
| Assets/ | 应用资源（图片、图标、Logo等） |
| AudioConverters/ | 音频转换器模块 |
| Behaviors/ | 交互行为类 |
| Controls/ | 自定义控件 |
| Converters/ | XAML值转换器 |
| Extensions/ | 扩展方法 |
| External/ | 外部依赖库 |
| Helper/ | 辅助工具类 |
| Libraries/ | BASS音频库（x64/x86/arm64） |
| Manager/ | 音频管理器 |
| Model/ | 数据模型 |
| OnlineAPIs/ | 在线音乐API |
| Reader/ | 文件读取器 |
| Services/ | 业务服务层 |
| Taskbar/ | 任务栏集成 |
| Utils/ | 通用工具类 |
| View/ | 视图页面 |
| ViewModel/ | 视图模型 |
| WebService/ | Web服务 |

## 详细文件索引

### Assets/
- Album.png, Artist.png, Music.png - 专辑/艺人/音乐图标
- SpectrumVisualization.png - 频谱可视化图标
- SplashScreen.*.png - 启动画面
- Square*.png, Wide*.png - 磁贴图标
- default_cover_black.png, default_cover_white.png - 默认封面
- icon.ico, play.ico, stop.ico, last.ico, next.ico - 播放控制图标
- hr.png, hr_backup.png - 分隔线资源

### AudioConverters/
| 文件 | 功能 |
|------|------|
| BassAudioConverter.cs | BASS音频格式转换 |

### Behaviors/
| 文件 | 功能 |
|------|------|
| AlbumCoverBehavior.cs | 专辑封面行为 |
| FadeImageBehavior.cs | 图片淡入淡出行为 |
| FadeImageBehaviorWin2d.cs | Win2d图片淡入淡出行为 |
| LyricsAutoScrollBehavior.cs | 歌词自动滚动行为 |

### Controls/
| 文件 | 功能 |
|------|------|
| AlbumArtControl.xaml/.cs | 专辑艺术控件 |
| GradientBackgroundControl.xaml/.cs | 渐变背景控件 |
| LyricsLineControl.xaml/.cs | 歌词行控件 |
| NotifyIconControl.xaml/.cs | 系统托盘图标控件 |
| ShaderBackgroundControl.xaml/.cs | Shader背景控件 |

### Converters/
| 文件 | 功能 |
|------|------|
| AdvancedThumbToolTipValueConverter.cs | 滑块提示值转换 |
| AlbumArtConverter.cs | 专辑封面转换 |
| BoolToBackGroundOpacityConverter.cs | 布尔转背景透明度 |
| BoolToColorConverter.cs | 布尔转颜色 |
| BoolToFontSizeConverter.cs | 布尔转字体大小 |
| BoolToFontWeightConverter.cs | 布尔转字重 |
| BoolToNegationVisibleConverter.cs | 布尔取反可见性 |
| BoolToOpacityConverter.cs | 布尔转透明度 |
| BoolToThicknessConverter.cs | 布尔转边距 |
| BoolToVisibilityConverter.cs | 布尔转可见性 |
| BooleanNegationConverter.cs | 布尔取反 |
| ByteArrayToImageConverter.cs | 字节数组转图片 |
| FavouriteIconConverter.cs | 收藏图标转换 |
| FavouriteTextConverter.cs | 收藏文本转换 |
| FullScreenIconConverter.cs | 全屏图标转换 |
| HideNavigationViewIconConverter.cs | 隐藏导航图标转换 |
| IsCoverExistToVisibleConverter.cs | 封面是否存在转可见性 |
| IsExistOnDeviceConverter.cs | 设备是否存在转换 |
| MusicStatusToHRimgConverter.cs | 音乐状态转高清图片 |
| MusicToTitleConverter.cs | 音乐转标题转换 |
| PageTypeStringToVisibleConverter.cs | 页面类型字符串转可见性 |
| PlayDetailButtonVisibleConverter.cs | 播放详情按钮可见性 |
| PlayModeToIconConverter.cs | 播放模式转图标 |
| PlayStatusToIconConverter.cs | 播放状态转图标 |
| PlayStatusToStringConverter.cs | 播放状态转字符串 |
| SampleRateBitDepthConverter.cs | 采样率位深转换 |
| SongCollectionPageTypeToEditVisibleConverter.cs | 歌曲集页面类型转编辑可见性 |
| SongCollectionPageTypeToIconConverter.cs | 歌曲集页面类型转图标 |
| SongCollectionPageTypeToRadiusConverter.cs | 歌曲集页面类型转圆角 |
| StringToVisibilityConverter.cs | 字符串转可见性 |
| TextAlignmentToHorizontalAlignmentConverter.cs | 文本对齐转水平对齐 |
| ThicknessToDoubleConverter.cs | 边距转双精度 |
| TimeSpanToStringConverter.cs | 时间跨度转字符串 |
| VolumeToIconConverter.cs | 音量转图标 |

### Extensions/
| 文件 | 功能 |
|------|------|
| DispatcherQueueExtensions.cs | 调度队列扩展 |
| ImageExtensions.cs | 图片扩展 |
| MenuFlyoutExtensions.cs | 菜单弹出扩展 |
| ObservableCollectionExtensions.cs | 可观察集合扩展 |

### External/
#### AnimatedWin2dControls/
- AnimatedTextBlock/ - 动画文本块控件
  - Effects/ - 文本效果（Blur, Default, Elastic, Fade, MotionBlur, Pivot, Wipe, Zoom）
  - Enums/ - 枚举定义
  - Internals/ - 内部辅助类

#### Lyricify.Lyrics.Helper/
歌词解析和搜索库
- Decrypter/ - 歌词解密（Krc, Qrc）
- Generators/ - 歌词生成器
- Helpers/ - 辅助工具
- Models/ - 数据模型
- Parsers/ - 歌词解析器
- Providers/ - 歌词提供者（Web API）
- Searchers/ - 歌词搜索器

#### WindowsMediaController/
- Main.cs - Windows媒体控制器接口

### Helper/
| 文件 | 功能 |
|------|------|
| AppJsonSerializerContextHelper.cs | JSON序列化上下文 |
| CustomAcrylicSystemBackdrop.cs | 自定义丙烯酸背景 |
| CustomMicaSystemBackdrop.cs | 自定义云母背景 |
| ImageHelper.cs | 图片辅助工具 |
| IpcEqualizerGainJsonContext.cs | IPC均衡器增益JSON上下文 |
| IpcJsonContext.cs | IPC JSON上下文 |
| PlayerJsonContext.cs | 播放器JSON上下文 |
| PowerManagementHelper.cs | 电源管理辅助 |
| RichTextBlockHelper.cs | 富文本块辅助 |
| SettingsJsonContext.cs | 设置JSON上下文 |
| SingleInstanceHelper.cs | 单实例辅助 |
| ThemeStyleHelper.cs | 主题样式辅助 |
| UsbWriterHelper.cs | USB写入辅助 |
| WindowHelper.cs | 窗口辅助 |
| WindowSizeHelper.cs | 窗口尺寸辅助 |

### Libraries/
| 路径 | 功能 |
|------|------|
| arm64/ | ARM64架构BASS库 |
| x64/ | x64架构BASS库 |
| x86/ | x86架构BASS库 |

BASS库包括：bass.dll, bass_fx.dll, bassenc_*.dll, bassmix.dll 等音频处理库

### Manager/
| 文件 | 功能 |
|------|------|
| BassManager.cs | BASS音频引擎管理器 |

### Model/
| 文件 | 功能 |
|------|------|
| AppData.cs | 应用数据 |
| AppSettings.cs | 应用设置 |
| AudioFileInfo.cs | 音频文件信息 |
| BassOutputDevice.cs | BASS输出设备 |
| BulkObservableCollection.cs | 批量可观察集合 |
| EffectComboBoxItem.cs | 效果下拉框项 |
| Folder.cs | 文件夹模型 |
| FontInfo.cs | 字体信息 |
| IpcEqualizerGain.cs | IPC均衡器增益 |
| IpcSetting.cs | IPC设置 |
| LastPlayListState.cs | 上次播放列表状态 |
| LyricLine.cs | 歌词行 |
| MenuModel.cs | 菜单模型 |
| MessageType.cs | 消息类型 |
| Music.cs | 音乐模型 |
| MusicGroup.cs | 音乐分组 |
| PlayList.cs | 播放列表 |
| PlayListMusic.cs | 播放列表音乐 |
| PlayListMusicItem.cs | 播放列表音乐项 |
| RequestMessage.cs | 请求消息 |
| ResponseMessage.cs | 响应消息 |
| SaveEqualizer.cs | 保存均衡器 |
| SavePlayState.cs | 保存播放状态 |
| SaveSettings.cs | 保存设置 |
| SortOption.cs | 排序选项 |
| SubFolder.cs | 子文件夹 |
| UsbDeviceMusic.cs | USB设备音乐 |
| UsbDeviceSubFolder.cs | USB设备子文件夹 |
| UsbStorageDevice.cs | USB存储设备 |
| WeakImageCache.cs | 弱图片缓存 |

### OnlineAPIs/CloudMusicAPI/
| 文件 | 功能 |
|------|------|
| NeteaseCloudMusicApi.cs | 网易云音乐API |
| NeteaseCloudMusicApiProvider.cs | 网易云音乐API提供者 |
| CloudMusicSearchHelper.cs | 云音乐搜索辅助 |
| Extensions/ | HTTP扩展 |
| Utils/ | 工具（Crypto, Request, Options, Extensions） |

### Reader/
| 文件 | 功能 |
|------|------|
| AudioCoverReader.cs | 音频封面读取 |
| UsbStorageDeviceReader.cs | USB存储设备读取 |

### Services/
| 文件 | 功能 |
|------|------|
| AppInitializerService.cs | 应用初始化服务 |
| AddFolderService.cs | 添加文件夹服务 |
| AudioConverterService.cs | 音频转换服务 |
| AutoRescanService.cs | 自动重新扫描服务 |
| BassPlayerCommandService.cs | BASS播放器命令服务 |
| IpcService.cs | IPC通信服务 |
| InitialFileScan.cs | 初始文件扫描 |
| LyricsRefreshService.cs | 歌词刷新服务 |
| MusicDatabaseService.cs | 音乐数据库服务 |
| NavigationService/ | 导航服务 |
| NotificationService.cs | 通知服务 |
| SystemMediaControlsService.cs | 系统媒体控制服务 |
| UsbDeviceSubFolderRescan.cs | USB设备子文件夹重扫描 |

### Services/NavigationService/
| 文件 | 功能 |
|------|------|
| INavigationService.cs | 导航服务接口 |
| INavigationServiceFactory.cs | 导航服务工厂接口 |
| INavigatable.cs | 可导航接口 |
| NavigationService.cs | 导航服务实现 |
| NavigationServiceFactory.cs | 导航服务工厂实现 |

### Taskbar/
| 文件 | 功能 |
|------|------|
| ITaskbarList3.cs | 任务栏列表接口 |
| TaskbarHelper.cs | 任务栏辅助 |
| TaskbarProgressState.cs | 任务栏进度状态 |
| ThumbButton.cs | 缩略图按钮 |
| ThumbButtonFlags.cs | 缩略图按钮标志 |
| ThumbButtonMask.cs | 缩略图按钮掩码 |
| ThumbButtonClickedEventArgs.cs | 缩略图按钮点击事件 |

### Utils/
| 文件 | 功能 |
|------|------|
| BindUtils.cs | 绑定工具 |
| CharStringCache.cs | 字符字符串缓存 |
| GarbledTextFixer.cs | 乱码文本修复 |
| ToolUtils.cs | 工具类 |

### View/
| 文件 | 功能 |
|------|------|
| MainPage.xaml/.cs | 主页面 |
| MusicBrowsePage/ | 音乐浏览页面 |
| FolderBrowsePage.xaml/.cs | 文件夹浏览页面 |
| FavouritePlayListPage.xaml/.cs | 收藏播放列表页面 |
| PlayingDetailPage.xaml/.cs | 播放详情页面 |
| PlayListPage.xaml/.cs | 播放列表页面 |
| PlayListSongPage.xaml/.cs | 播放列表歌曲页面 |
| SongListPage.xaml/.cs | 歌曲列表页面 |
| SongFolderListPage.xaml/.cs | 歌曲文件夹列表页面 |
| SongCollectionPage.xaml/.cs | 歌曲集页面 |
| SongArtistListPage.xaml/.cs | 歌手列表页面 |
| ArtistPage.xaml/.cs | 歌手页面 |
| AlbumPage.xaml/.cs | 专辑页面 |
| AddFolderPage.xaml/.cs | 添加文件夹页面 |
| SettingsPage.xaml/.cs | 设置页面 |
| SubView/ | 子视图 |
| AlbumDetailWindow.xaml/.cs | 专辑详情窗口 |
| EqualizerDialog.xaml/.cs | 均衡器对话框 |
| MusicDetailsWindow.xaml/.cs | 音乐详情窗口 |
| ProgressDialog.xaml/.cs | 进度对话框 |

### ViewModel/
| 文件 | 功能 |
|------|------|
| AppViewModel.cs | 应用视图模型 |
| AppViewModel.Settings.cs | 应用设置视图模型 |
| Pages/ | 页面视图模型 |
| MainViewModel.cs | 主视图模型 |
| PlayingDetailViewModel.cs | 播放详情视图模型 |
| PlayListViewModel.cs | 播放列表视图模型 |
| PlayListSongViewModel.cs | 播放列表歌曲视图模型 |
| SongListViewModel.cs | 歌曲列表视图模型 |
| SongFolderListViewModel.cs | 歌曲文件夹列表视图模型 |
| SongCollectionViewModel.cs | 歌曲集视图模型 |
| SongArtistViewModel.cs | 歌手视图模型 |
| FolderViewModel.cs | 文件夹视图模型 |
| FavouritePlayListViewModel.cs | 收藏播放列表视图模型 |
| ArtistViewModel.cs | 艺术家视图模型 |
| AlbumViewModel.cs | 专辑视图模型 |
| AddFolderViewModel.cs | 添加文件夹视图模型 |
| MusicBrowseViewModel.cs | 音乐浏览视图模型 |
| SettingsViewModel.cs | 设置视图模型 |

### WebService/
| 文件 | 功能 |
|------|------|
| LrcService.cs | 歌词服务 |

### 根目录
| 文件 | 功能 |
|------|------|
| App.xaml/.cs | 应用入口 |
| MainWindow.xaml/.cs | 主窗口 |
| LICENSE | 许可证 |
| 项目使用 WinUI 框架 | |
