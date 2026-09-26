#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class EventSubscriberBehaviour : MonoBehaviour
{
    private readonly List<Action> _unsubscribers = new();
    private bool _cleanedUp;

    protected void AddSubscription(Action unsubscribe)
    {
        if (unsubscribe == null) throw new ArgumentNullException(nameof(unsubscribe));
        if (_cleanedUp)
            throw new InvalidOperationException($"Нельзя добавлять подписки после OnDestroy в {GetType().Name}.");
        _unsubscribers.Add(unsubscribe);
    }

    private void OnDestroy()
    {
        if (_cleanedUp) return;
        _cleanedUp = true;

        for (int i = _unsubscribers.Count - 1; i >= 0; i--)
        {
            try { _unsubscribers[i]?.Invoke(); }
            catch (Exception ex)
            {
                Debug.LogError($"Ошибка при отписке в {GetType().Name}: {ex}");
            }
        }
        _unsubscribers.Clear();
        OnCleanup();
    }

    protected virtual void OnCleanup() { }
}