using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Avalonia.Xaml.Interactivity;

namespace LocalMusicPlayer.Behaviors;

public class LyricsAutoScrollBehavior : Behavior<ItemsControl>
{
    private ScrollViewer? _scrollViewer;

    public static readonly StyledProperty<ScrollViewer?> TargetScrollViewerProperty =
        AvaloniaProperty.Register<LyricsAutoScrollBehavior, ScrollViewer?>(nameof(TargetScrollViewer));

    public ScrollViewer? TargetScrollViewer
    {
        get => GetValue(TargetScrollViewerProperty);
        set => SetValue(TargetScrollViewerProperty, value);
    }

    public static readonly StyledProperty<int> CurrentIndexProperty =
        AvaloniaProperty.Register<LyricsAutoScrollBehavior, int>(nameof(CurrentIndex), -1);

    public int CurrentIndex
    {
        get => GetValue(CurrentIndexProperty);
        set => SetValue(CurrentIndexProperty, value);
    }

    static LyricsAutoScrollBehavior()
    {
        CurrentIndexProperty.Changed.AddClassHandler<LyricsAutoScrollBehavior>((x, _) => x.OnCurrentIndexChanged());
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        if (AssociatedObject == null) return;

        AssociatedObject.AttachedToLogicalTree += OnAttachedToLogicalTree;
        AssociatedObject.DetachedFromLogicalTree += OnDetachedFromLogicalTree;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        if (AssociatedObject == null) return;

        AssociatedObject.AttachedToLogicalTree -= OnAttachedToLogicalTree;
        AssociatedObject.DetachedFromLogicalTree -= OnDetachedFromLogicalTree;
    }

    private void OnAttachedToLogicalTree(object? sender, LogicalTreeAttachmentEventArgs e)
    {
        _scrollViewer = TargetScrollViewer ?? FindParentScrollViewer(AssociatedObject);
    }

    private void OnDetachedFromLogicalTree(object? sender, LogicalTreeAttachmentEventArgs e)
    {
        _scrollViewer = null;
    }

    private void OnCurrentIndexChanged()
    {
        var index = CurrentIndex;
        if (index < 0 || AssociatedObject == null) return;

        Dispatcher.UIThread.Post(() => ScrollToIndex(index));
    }

    private void ScrollToIndex(int index)
    {
        if (AssociatedObject == null || _scrollViewer == null) return;

        var container = AssociatedObject.ContainerFromIndex(index);
        if (container == null) return;

        try
        {
            var transform = container.TransformToVisual(AssociatedObject);
            if (transform == null) return;

            var containerTop = new Point(0, 0).Transform(transform.Value).Y;
            var absoluteY = containerTop + AssociatedObject.Margin.Top;
            var targetOffset = absoluteY
                               + (container.Bounds.Height / 2)
                               - (_scrollViewer.Viewport.Height * 0.35);

            var maxOffset = Math.Max(0, _scrollViewer.Extent.Height - _scrollViewer.Viewport.Height);
            targetOffset = Math.Clamp(targetOffset, 0, maxOffset);

            _scrollViewer.Offset = new Vector(0, targetOffset);
        }
        catch (InvalidOperationException)
        {
        }
    }

    private static ScrollViewer? FindParentScrollViewer(Control? control)
    {
        while (control != null)
        {
            if (control is ScrollViewer sv) return sv;
            control = control.GetLogicalParent<Control>();
        }

        return null;
    }
}
