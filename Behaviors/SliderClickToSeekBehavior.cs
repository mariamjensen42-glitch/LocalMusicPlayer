using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Xaml.Interactivity;

namespace LocalMusicPlayer.Behaviors;

public class SliderClickToSeekBehavior : Behavior<Slider>
{
    protected override void OnAttached()
    {
        base.OnAttached();
        if (AssociatedObject == null) return;

        AssociatedObject.PointerPressed += OnPointerPressed;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        if (AssociatedObject == null) return;

        AssociatedObject.PointerPressed -= OnPointerPressed;
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (AssociatedObject == null) return;

        var point = e.GetPosition(AssociatedObject);
        var seekValue = CalculateValue(point, AssociatedObject.Bounds.Width, AssociatedObject.Minimum,
            AssociatedObject.Maximum);

        if (seekValue.HasValue)
        {
            AssociatedObject.Value = seekValue.Value;
        }
    }

    private static double? CalculateValue(Point point, double width, double minimum, double maximum)
    {
        if (width <= 0 || maximum <= minimum) return null;

        var ratio = Math.Clamp(point.X / width, 0.0, 1.0);
        return minimum + ratio * (maximum - minimum);
    }
}