#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

public class EventBus : IEventBus
{
    private readonly Dictionary<Type, List<SubscriptionEntry>> _subscriptions = new();
    private readonly Dictionary<Type, Type[]> _typeHierarchyCache = new();
    
    private Type[] GetTypesToCheck(Type eventType)
    {
        if (_typeHierarchyCache.TryGetValue(eventType, out var cached))
            return cached;

        var types = new List<Type>();

        var current = eventType;
        while (current != null && current != typeof(object))
        {
            types.Add(current);
            current = current.BaseType;
        }

        foreach (var iface in eventType.GetInterfaces())
            types.Add(iface);

        var result = types.ToArray();
        _typeHierarchyCache[eventType] = result;
        return result;
    }

    public void Publish<TEvent>(TEvent eventData) where TEvent : class
    {
        if (eventData == null) throw new ArgumentNullException(nameof(eventData));

        var eventType = eventData.GetType();
        var typesToCheck = GetTypesToCheck(eventType);

        foreach (var type in typesToCheck)
        {
            if (!_subscriptions.TryGetValue(type, out var list))
                continue;

            var snapshot = list.ToArray();
            foreach (var entry in snapshot)
            {
                try
                {
                    entry.Wrapper(eventData);
                }
                catch (Exception ex)
                {
                    Debug.LogError(
                        $"[{nameof(EventBus)}] Ошибка при обработке {eventType.Name} " +
                        $"подписчиком {entry.Original.Target}: {ex}");
                }
            }
        }
    }
    public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class
    {
        if (handler == null) throw new ArgumentNullException(nameof(handler));

        var type = typeof(TEvent);
        if (!_subscriptions.TryGetValue(type, out var list))
        {
            list = new List<SubscriptionEntry>();
            _subscriptions[type] = list;
        }

        void wrapper(object obj) => handler((TEvent)obj);
        list.Add(new SubscriptionEntry(handler, wrapper));
    }
    public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : class
    {
        if (handler == null) return;

        if (_subscriptions.TryGetValue(typeof(TEvent), out var list))
        {
            list.RemoveAll(entry => entry.Original.Equals(handler));
            if (list.Count == 0)
                _subscriptions.Remove(typeof(TEvent));
        }
    }
    public void Clear()
    {
        _subscriptions.Clear();
        _typeHierarchyCache.Clear();
    }

    private readonly struct SubscriptionEntry
    {
        public Delegate Original { get; }
        public Action<object> Wrapper { get; }

        public SubscriptionEntry(Delegate original, Action<object> wrapper)
        {
            Original = original;
            Wrapper = wrapper;
        }
    }
}