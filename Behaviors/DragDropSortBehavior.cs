using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using LocalMusicPlayer.Models;

namespace LocalMusicPlayer.Behaviors;

public class DragDropSortBehavior : Behavior<ListBox>
{
    private int _dragStartIndex = -1;
    private bool _isDragging;

    public static readonly StyledProperty<ICommand> MoveCommandProperty =
        AvaloniaProperty.Register<DragDropSortBehavior, ICommand>(nameof(MoveCommand));

    public ICommand MoveCommand
    {
        get => GetValue(MoveCommandProperty);
        set => SetValue(MoveCommandProperty, value);
    }

    protected override void OnAttached()
    {
        if (AssociatedObject != null)
        {
            AssociatedObject.AddHandler(InputElement.PointerPressedEvent, OnPointerPressed, RoutingStrategies.Bubble);
            AssociatedObject.AddHandler(InputElement.PointerMovedEvent, OnPointerMoved, RoutingStrategies.Bubble);
            AssociatedObject.AddHandler(InputElement.PointerReleasedEvent, OnPointerReleased, RoutingStrategies.Bubble);
        }
    }

    protected override void OnDetaching()
    {
        if (AssociatedObject != null)
        {
            AssociatedObject.RemoveHandler(InputElement.PointerPressedEvent, OnPointerPressed);
            AssociatedObject.RemoveHandler(InputElement.PointerMovedEvent, OnPointerMoved);
            AssociatedObject.RemoveHandler(InputElement.PointerReleasedEvent, OnPointerReleased);
        }
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (AssociatedObject == null || e.GetCurrentPoint(AssociatedObject).Properties.IsLeftButtonPressed == false)
            return;

        var point = e.GetPosition(AssociatedObject);
        var item = GetListBoxItemAtPoint(point);

        if (item != null)
        {
            _dragStartIndex = AssociatedObject.Items.IndexOf(item);
            _isDragging = true;
        }
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_isDragging || AssociatedObject == null || _dragStartIndex < 0)
        {
            _isDragging = false;
            _dragStartIndex = -1;
            return;
        }

        var point = e.GetPosition(AssociatedObject);
        var targetIndex = GetInsertIndex(point);

        if (targetIndex >= 0 && targetIndex != _dragStartIndex)
        {
            if (MoveCommand != null && MoveCommand.CanExecute((_dragStartIndex, targetIndex)))
            {
                MoveCommand.Execute((_dragStartIndex, targetIndex));
            }
            else
            {
                var items = AssociatedObject.ItemsSource as ObservableCollection<Song>;
                if (items != null && _dragStartIndex < items.Count && targetIndex < items.Count)
                {
                    items.Move(_dragStartIndex, targetIndex);
                }
            }
        }

        _isDragging = false;
        _dragStartIndex = -1;
    }

    private object? GetListBoxItemAtPoint(Point point)
    {
        if (AssociatedObject == null)
            return null;

        var controls = AssociatedObject.GetVisualChildren();
        foreach (var control in controls)
        {
            if (control is ListBoxItem item)
            {
                var bounds = item.Bounds;
                if (bounds.Contains(point))
                    return item.DataContext ?? item;
            }
        }

        var result = AssociatedObject.InputHitTest(point);
        if (result != null)
        {
            var current = result as Visual;
            while (current != null)
            {
                if (current is ListBoxItem listBoxItem)
                    return listBoxItem.DataContext ?? listBoxItem;
                current = current.GetVisualParent() as Visual;
            }
        }

        return null;
    }

    private int GetInsertIndex(Point point)
    {
        if (AssociatedObject == null)
            return -1;

        var items = AssociatedObject.ItemsSource as ObservableCollection<Song>;
        if (items == null || items.Count == 0)
            return -1;

        var scrollViewer = AssociatedObject.FindDescendantOfType<ScrollViewer>();
        if (scrollViewer == null)
            return -1;

        var itemHeight = AssociatedObject.Bounds.Height / Math.Max(1, items.Count);
        var scrollOffset = scrollViewer.Offset.Y;
        var adjustedY = point.Y + scrollOffset;
        var targetIndex = (int)(adjustedY / itemHeight);

        if (targetIndex < 0)
            targetIndex = 0;
        if (targetIndex >= items.Count)
            targetIndex = items.Count - 1;

        return targetIndex;
    }
}
