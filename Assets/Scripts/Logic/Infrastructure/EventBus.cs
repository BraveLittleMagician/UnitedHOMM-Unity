#nullable enable


using System;
using System.Collections.Generic;
using System.Linq;

public class EventBus : IEventBus
{
    private readonly Dictionary<Type, List<Delegate>> _subscriptions = new();

    public void Publish<TEvent>(TEvent eventData) where TEvent : class
    {
        if (!_subscriptions.TryGetValue(typeof(TEvent), out var handlers)) return;

        foreach (var handler in handlers.ToList())
            (handler as Action<TEvent>)?.Invoke(eventData);
    }
    public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class
    {
        var type = typeof(TEvent);
        if (!_subscriptions.ContainsKey(type))
            _subscriptions[type] = new List<Delegate>();
        _subscriptions[type].Add(handler);
    }
    public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : class
    {
        var type = typeof(TEvent);
        if (_subscriptions.TryGetValue(type, out var handlers))
            handlers.Remove(handler);
    }
    public void Clear()
    {
        _subscriptions.Clear();
    }
}