using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;

namespace LocalMusicPlayer.Behaviors;

public class SliderClickToSeekBehavior : Behavior<Slider>
{
    protected override void OnAttached()
    {
        base.OnAttached();
        if (AssociatedObject != null)
        {
            AssociatedObject.AddHandler(InputElement.PointerPressedEvent, OnPointerPressed, RoutingStrategies.Bubble);
        }
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        if (AssociatedObject != null)
        {
            AssociatedObject.RemoveHandler(InputElement.PointerPressedEvent, OnPointerPressed);
        }
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (AssociatedObject == null) return;

        var point = e.GetPosition(AssociatedObject);
        var bounds = AssociatedObject.Bounds;

        if (bounds.Width <= 0) return;

        // 计算点击位置对应的值
        var ratio = point.X / bounds.Width;
        var value = AssociatedObject.Minimum + ratio * (AssociatedObject.Maximum - AssociatedObject.Minimum);

        // 限制在有效范围内
        value = Math.Clamp(value, AssociatedObject.Minimum, AssociatedObject.Maximum);

        AssociatedObject.Value = value;
    }
}
