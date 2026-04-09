# Avalonia UI 样式规范

## 样式文件结构

```
Styles/
├── Resources.axaml        # 颜色、字体、尺寸等资源
├── Controls/
│   ├── ButtonStyles.axaml
│   ├── SliderStyles.axaml
│   └── ListStyles.axaml
└── Themes/
    ├── LightTheme.axaml
    └── DarkTheme.axaml
```

## Resources.axaml 结构

```xml
<ResourceDictionary>
    <!-- 颜色 -->
    <Color x:Key="PrimaryColor">#FF6200EE</Color>
    <SolidColorBrush x:Key="PrimaryBrush" Color="{StaticResource PrimaryColor}"/>

    <!-- 字体 -->
    <FontFamily x:Key="DefaultFont">Microsoft YaHei UI</FontFamily>

    <!-- 尺寸 -->
    <x:Double x:Key="DefaultFontSize">14</x:Double>
    <Thickness x:Key="DefaultPadding">8</Thickness>

    <!-- 圆角 -->
    <CornerRadius x:Key="DefaultCornerRadius">4</CornerRadius>
</ResourceDictionary>
```

## 控件样式命名

| 控件 | 默认样式键 | 按压样式键 |
|------|-----------|-----------|
| Button | PrimaryButton | PrimaryButtonPointerOver |
| Slider | PrimarySlider | PrimarySliderPointerOver |
| ListBox | PrimaryListBox | PrimaryListBoxItemPointerOver |

## 样式使用规则

1. 控件样式通过 `StaticResource` 引用
2. 避免在控件上直接设置属性，优先使用样式
3. 相同视觉属性的控件共享同一样式
4. 主题色通过资源键引用，便于切换

## 示例

```xml
<Button Classes="PrimaryButton"
        Content="播放"
        HorizontalAlignment="Center"/>
```
