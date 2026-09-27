#nullable enable

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;

public sealed class EventBus : IEventBus
{
    private readonly ILogger _logger;
    private readonly Dictionary<Type, List<SubscriptionEntry>> _subscriptions = new();
    private readonly Dictionary<Type, Type[]> _typeHierarchyCache = new();

    public EventBus(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void Publish<TEvent>(TEvent eventData) where TEvent : class
    {
        if (eventData == null) throw new ArgumentNullException(nameof(eventData));

        var eventType = eventData.GetType();
        var typesToCheck = GetTypesToCheck(eventType);

        foreach (var type in typesToCheck)
        {
            if (!_subscriptions.TryGetValue(type, out var list)) continue;

            int count = list.Count;
            if (count == 0) continue;

            var buffer = ArrayPool<SubscriptionEntry>.Shared.Rent(count);
            try
            {
                list.CopyTo(buffer, 0);

                for (int i = 0; i < count; i++)
                {
                    var entry = buffer[i];
                    try
                    {
                        entry.Wrapper(eventData);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            $"[{nameof(EventBus)}] Ошибка при обработке {eventType.Name} " +
                            $"подписчиком {entry.Original.Target}: {ex}");
                    }
                }
            }
            finally
            {
                ArrayPool<SubscriptionEntry>.Shared.Return(buffer, clearArray: false);
            }
        }
    }

    public IDisposable Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class
    {
        if (handler == null) throw new ArgumentNullException(nameof(handler));

        var eventType = typeof(TEvent);

        if (!_subscriptions.TryGetValue(eventType, out var list))
        {
            list = new List<SubscriptionEntry>();
            _subscriptions[eventType] = list;
        }

        if (list.Any(e => e.Original.Equals(handler)))
        {
            _logger.LogWarning(
                $"[{nameof(EventBus)}] Повторная подписка на {eventType.Name} " +
                $"от {handler.Target}. Игнорирую.");
            
            return new NoOpDisposable();
        }

        void Wrapper(object obj) => handler((TEvent)obj);

        var entry = new SubscriptionEntry(eventType, handler, Wrapper);
        list.Add(entry);

        return new Subscription(this, entry);
    }

    public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : class
    {
        if (handler == null) return;

        var eventType = typeof(TEvent);
        if (!_subscriptions.TryGetValue(eventType, out var list))
            return;

        list.RemoveAll(entry => entry.Original.Equals(handler));

        if (list.Count == 0)
            _subscriptions.Remove(eventType);
    }

    public void Clear()
    {
        _subscriptions.Clear();
    }

    private void RemoveEntry(SubscriptionEntry entry)
    {
        if (!_subscriptions.TryGetValue(entry.EventType, out var list))
            return;

        list.Remove(entry);

        if (list.Count == 0)
            _subscriptions.Remove(entry.EventType);
    }

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

    private sealed class SubscriptionEntry
    {
        public Type EventType { get; }
        public Delegate Original { get; }
        public Action<object> Wrapper { get; }

        public SubscriptionEntry(Type eventType, Delegate original, Action<object> wrapper)
        {
            EventType = eventType;
            Original = original;
            Wrapper = wrapper;
        }
    }

    private sealed class Subscription : IDisposable
    {
        private EventBus? _bus;
        private SubscriptionEntry? _entry;

        public Subscription(EventBus bus, SubscriptionEntry entry)
        {
            _bus = bus;
            _entry = entry;
        }

        public void Dispose()
        {
            if (_bus == null || _entry == null) return;

            _bus.RemoveEntry(_entry);
            _bus = null;
            _entry = null;
        }
    }

    private sealed class NoOpDisposable : IDisposable
    {
        public void Dispose() { }
    }
}