using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LocalMusicPlayer.ViewModels;

public abstract partial class ViewModelBase : ObservableObject, IDisposable
{
    private readonly List<IDisposable> _disposables = new();
    private bool _disposed;

    protected void AddDisposable(IDisposable disposable)
    {
        _disposables.Add(disposable);
    }

    protected void SubscribeEvent(Action subscribe, Action unsubscribe)
    {
        subscribe();
        AddDisposable(new EventSubscription(unsubscribe));
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        foreach (var disposable in _disposables)
            disposable.Dispose();
        _disposables.Clear();
        DisposeCore();
    }

    protected virtual void DisposeCore() { }

    private class EventSubscription : IDisposable
    {
        private readonly Action _unsubscribe;
        public EventSubscription(Action unsubscribe) => _unsubscribe = unsubscribe;
        public void Dispose() => _unsubscribe();
    }
}
