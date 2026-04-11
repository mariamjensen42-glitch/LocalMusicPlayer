using System;
using System.Collections.Generic;

namespace LocalMusicPlayer.Services;

public interface INavigationService
{
    Type? CurrentPageType { get; }

    bool CanGoBack { get; }

    bool IsQueuePanelOpen { get; set; }

    event EventHandler<Type?>? CurrentPageChanged;
    event EventHandler<bool>? QueuePanelChanged;

    void NavigateTo<T>() where T : class;
    void NavigateTo(Type pageType);
    void GoBack();
    void NavigateBack();
    void ClearHistory();
    void ToggleQueuePanel();
}
