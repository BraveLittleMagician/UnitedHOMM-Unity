#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

public class UIService : MonoBehaviour
{
    private IButtonColorsProvider _provider = null!;
    private readonly HashSet<IButtonColorsSubscriber> _subscribers = new();

    private void OnDestroy()
    {
        _subscribers.Clear();
    }

    public void Initialize(IButtonColorsProvider buttonColorsProvider)
    {
        _provider = buttonColorsProvider ?? throw new ArgumentNullException(nameof(_provider));
        foreach (var field in _subscribers) field.SetProvider(_provider);
    }
    public void RegisterSubscriber(IButtonColorsSubscriber subscriber)
    {
        if (!_subscribers.Contains(subscriber))
            _subscribers.Add(subscriber);
    }
    public void UnregisterSubscriber(IButtonColorsSubscriber subscriber)
    {
        _subscribers.Remove(subscriber);
    }
}