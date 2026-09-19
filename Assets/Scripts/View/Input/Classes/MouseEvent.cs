#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class MouseEvent<TSub> : MonoBehaviour where TSub : notnull, IMouseEventSubscriber
{
    private readonly HashSet<TSub> _forAdd = new();
    private readonly HashSet<TSub> _forRemove = new();
    protected readonly HashSet<TSub> _subscribers = new();
    protected bool _isPerforming = false;

    protected void Notify(Action<TSub> notifyAction)
    {
        _isPerforming = true;
        try
        {
            var dead = new List<TSub?>(_subscribers.Count);

            foreach (var subscriber in _subscribers)
            {
                if (subscriber == null)
                {
                    dead.Add(subscriber);
                    continue;
                }

                try
                {
                    notifyAction(subscriber);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Ошибка при уведомлении {subscriber} в {GetType().Name}: {ex}");
                }
            }

            foreach (var d in dead)
                _subscribers.Remove(d!);
        }
        finally
        {
            _isPerforming = false;

            if (_forAdd.Count > 0)
            {
                foreach (var s in _forAdd) _subscribers.Add(s);
                _forAdd.Clear();
            }
            if (_forRemove.Count > 0)
            {
                foreach (var s in _forRemove) _subscribers.Remove(s);
                _forRemove.Clear();
            }
        }
    }
    public void RegisterSubscriber(TSub subscriber)
    {
        if (_subscribers.Contains(subscriber)) return;
        if (!_isPerforming)
            _subscribers.Add(subscriber);
        else
            _forAdd.Add(subscriber);
    }
    public void UnregisterSubscriber(TSub subscriber)
    {
        if (!_isPerforming)
            _subscribers.Remove(subscriber);
        else
            _forRemove.Add(subscriber);
    }
}