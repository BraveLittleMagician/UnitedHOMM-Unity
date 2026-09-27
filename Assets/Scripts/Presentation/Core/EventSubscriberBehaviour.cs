#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class EventSubscriberBehaviour : MonoBehaviour
{
    private readonly List<IDisposable> _subscriptions = new();
    private bool _cleanedUp;

    protected void AddSubscription(IDisposable subscription)
    {
        if (subscription == null)
            throw new ArgumentNullException(nameof(subscription));

        if (_cleanedUp)
        {
            SafeDispose(subscription, "AddSubscription после OnDestroy");
            return;
        }

        _subscriptions.Add(subscription);
    }

    protected void AddSubscription(Action unsubscribe)
    {
        if (unsubscribe == null)
            throw new ArgumentNullException(nameof(unsubscribe));

        AddSubscription(new ActionDisposable(unsubscribe));
    }

    private void OnDestroy()
    {
        CleanupSubscriptions();
        OnCleanup();
    }

    protected virtual void OnCleanup() { }

    private void CleanupSubscriptions()
    {
        if (_cleanedUp) return;
        _cleanedUp = true;

        for (int i = _subscriptions.Count - 1; i >= 0; i--)
        {
            var sub = _subscriptions[i];
            SafeDispose(sub, $"CleanupSubscriptions ({GetType().Name})");
        }

        _subscriptions.Clear();
    }

    private static void SafeDispose(IDisposable disposable, string context)
    {
        if (disposable == null) return;

        try
        {
            disposable.Dispose();
        }
        catch (Exception ex)
        {
            Debug.LogError($"[{nameof(EventSubscriberBehaviour)}] Ошибка Dispose в {context}: {ex}");
        }
    }

    private sealed class ActionDisposable : IDisposable
    {
        private Action? _action;

        public ActionDisposable(Action action) => _action = action;

        public void Dispose()
        {
            var action = _action;
            _action = null;
            action?.Invoke();
        }
    }
}