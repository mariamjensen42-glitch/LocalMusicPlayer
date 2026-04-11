using System;
using System.Collections.Generic;
using System.Linq;

namespace LocalMusicPlayer.Services;

public class NavigationService : INavigationService
{
    private readonly Stack<Type> _history = new();
    private bool _isQueuePanelOpen;

    public Type? CurrentPageType { get; private set; }

    public bool CanGoBack => _history.Count > 0;

    public bool IsQueuePanelOpen
    {
        get => _isQueuePanelOpen;
        set
        {
            if (_isQueuePanelOpen != value)
            {
                _isQueuePanelOpen = value;
                QueuePanelChanged?.Invoke(this, value);
            }
        }
    }

    public event EventHandler<Type?>? CurrentPageChanged;
    public event EventHandler<bool>? QueuePanelChanged;

    public void NavigateTo<T>() where T : class
    {
        NavigateTo(typeof(T));
    }

    public void NavigateTo(Type pageType)
    {
        if (CurrentPageType != null)
        {
            _history.Push(CurrentPageType);
        }

        CurrentPageType = pageType;
        CurrentPageChanged?.Invoke(this, CurrentPageType);
    }

    public void GoBack()
    {
        if (_history.Count > 0)
        {
            CurrentPageType = _history.Pop();
            CurrentPageChanged?.Invoke(this, CurrentPageType);
        }
    }

    public void NavigateBack()
    {
        GoBack();
    }

    public void ClearHistory()
    {
        _history.Clear();
    }

    public void ToggleQueuePanel()
    {
        IsQueuePanelOpen = !IsQueuePanelOpen;
    }
}
