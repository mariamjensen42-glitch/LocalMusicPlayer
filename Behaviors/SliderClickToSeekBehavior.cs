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
            AssociatedObject.AddHandler(InputElement.PointerReleasedEvent, OnPointerReleased, RoutingStrategies.Bubble);
        }
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        if (AssociatedObject != null)
        {
            AssociatedObject.RemoveHandler(InputElement.PointerPressedEvent, OnPointerPressed);
            AssociatedObject.RemoveHandler(InputElement.PointerReleasedEvent, OnPointerReleased);
        }
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (AssociatedObject == null) return;

        var point = e.GetPosition(AssociatedObject);
        CalculateAndSeek(point);
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
    }

    private void CalculateAndSeek(Point point)
    {
        if (AssociatedObject == null) return;

        var bounds = AssociatedObject.Bounds;

        if (bounds.Width <= 0) return;

        var ratio = point.X / bounds.Width;
        var value = AssociatedObject.Minimum + ratio * (AssociatedObject.Maximum - AssociatedObject.Minimum);

        value = Math.Clamp(value, AssociatedObject.Minimum, AssociatedObject.Maximum);

        AssociatedObject.Value = value;
    }
}
