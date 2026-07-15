#nullable enable

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GlobalDeselector : MonoBehaviour, IGlobalDeselector
{
    private static readonly HashSet<IGlobalDeselectSubscriber> _subscribers = new();
    
    public void RegisterSubscriber(IGlobalDeselectSubscriber subscriber)
    {
        if (!_subscribers.Contains(subscriber))
            _subscribers.Add(subscriber);
    }
    public void UnregisterSubscriber(IGlobalDeselectSubscriber subscriber)
    {
        _subscribers.Remove(subscriber);
    }
    public void TriggerGlobalClick()
    {
        foreach (var subscriber in _subscribers.ToArray()) subscriber.Deselect();
    }
}