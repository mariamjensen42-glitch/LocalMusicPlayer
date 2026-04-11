# 功能开发记录

## 2026-04-12: 艺术家列表页和专辑列表页

**需求**: 侧边栏"艺术家"和"专辑"按钮应展示全部艺术家/专辑的独立页面，点击卡片后导航到对应的详情页（ArtistDetailView/AlbumDetailView）

**之前行为**: 侧边栏"艺术家"/"专辑" → NavigateToLibraryBrowserCommand → LibraryBrowserView（显示 ArtistInfo/AlbumInfo 列表，点击后只在浏览器内展示歌曲，不进入详情页）

**修改**:
- **[ArtistsPageViewModel.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/ViewModels/ArtistsPageViewModel.cs)**: 新建，从 ILibraryCategoryService 加载 ArtistGroup 列表，提供 SelectItem 命令触发 OnNavigateToDetail 回调
- **[ArtistsPageView.axaml](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/Views/Library/ArtistsPageView.axaml)**: 新建，WrapPanel 网格展示 ArtistGroup 卡片（圆形头像 + 艺术家名 + 歌曲数）
- **[AlbumsPageViewModel.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/ViewModels/AlbumsPageViewModel.cs)**: 新建，从 ILibraryCategoryService 加载 AlbumGroup 列表，提供 SelectItem 命令触发 OnNavigateToDetail 回调
- **[AlbumsPageView.axaml](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/Views/Library/AlbumsPageView.axaml)**: 新建，WrapPanel 网格展示 AlbumGroup 卡片（方形封面 + 专辑名 + 艺术家名 + 歌曲数）
- **[IViewModelFactory.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/Services/Navigation/IViewModelFactory.cs)**: 添加 CreateArtistsPageViewModel / CreateAlbumsPageViewModel
- **[ViewModelFactory.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/Services/Navigation/ViewModelFactory.cs)**: 实现新工厂方法
- **[ViewLocator.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/ViewModels/ViewLocator.cs)**: 添加 ArtistsPageViewModel / AlbumsPageViewModel 映射
- **[MainWindowViewModel.cs](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/ViewModels/MainWindowViewModel.cs)**:
  - NavigationPage 枚举添加 Artists、Albums
  - 添加 ArtistsPageViewModel / AlbumsPageViewModel 属性
  - 添加 NavigateToArtistsPageCommand / NavigateToAlbumsPageCommand 方法，创建 VM 并绑定 OnNavigateToDetail 回调到 NavigateToArtistDetail/NavigateToAlbumDetail
  - OnCurrentPageChanged 添加新 VM 类型处理
- **[MainWindow.axaml](file:///c:/Users/yun/Programming/C%23/LocalMusicPlayer/Views/Main/MainWindow.axaml)**:
  - "艺术家"侧边栏按钮改为 NavigateToArtistsPageCommand
  - "专辑"侧边栏按钮改为 NavigateToAlbumsPageCommand
  - IsChecked 绑定改用 NavigationPageToColorConverter + 对应枚举值

**导航路径**:
```
侧边栏 "艺术家"
  → NavigateToArtistsPageCommand
    → ArtistsPageView (ArtistGroup 卡片网格)
      → 点击卡片 → NavigateToArtistDetail → ArtistDetailView (含 DetailHeaderView)
侧边栏 "专辑"
  → NavigateToAlbumsPageCommand
    → AlbumsPageView (AlbumGroup 卡片网格)
      → 点击卡片 → NavigateToAlbumDetail → AlbumDetailView (含 DetailHeaderView)
```
