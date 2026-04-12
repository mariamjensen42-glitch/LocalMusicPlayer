---
alwaysApply: false
description: 
---
## 目录结构

```
Models/          # 数据模型
ViewModels/      # 视图模型（ReactiveUI）
Views/           # 视图（XAML）
Services/        # 业务服务接口
Converters/      # XAML 值转换器
Helpers/         # 工具类
Styles/          # 共享样式资源
Behaviors/       # Avalonia 行为

```

## 开发规范

1. **MVVM 模式** — ViewModel 继承 ReactiveObject，View 通过 DataContext 绑定
2. **服务接口** — 业务逻辑通过接口解耦，置于 Services/ 目录
3. **XAML 绑定** — 使用 x:DataType 编译期绑定，禁用 Binding 字符串路径
4. **样式资源** — 共享颜色/主题置于 Styles/Resources.axaml，通过 MergedDictionaries 引用
5. **无注释** — 代码中不添加多余注释，保持简洁

## 禁止事项

- 禁止在 ViewModel 中直接操作 UI 控件
- 禁止使用 Binding 字符串路径（应用 x:DataType 编译绑定）
- 禁止在 XAML 中硬编码中文字符串（应提取到资源文件）
- 禁止内联样式
