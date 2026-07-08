#nullable enable

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(GlobalDeselector))]
[RequireComponent(typeof(ClickEventer))]
public class GlobalDeselector : MonoBehaviour
{
    private static readonly HashSet<IGlobalDeselectSubscriber> _subscribers = new ();
    public static GlobalDeselector Instance { get; private set; } = null!;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

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