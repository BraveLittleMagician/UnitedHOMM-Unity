#nullable enable

using System;

public interface IEventBus
{
    void Publish<TEvent>(TEvent eventData) where TEvent : class;
    IDisposable Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class;
    void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : class;
    void Clear();
}