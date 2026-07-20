#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class UIService : MonoBehaviour
{
    private IButtonColorsProvider _provider = null!;
    private readonly HashSet<IButtonColorsSubscriber> _subscribers = new();

    private void Start()
    {
        foreach (var field in _subscribers) field.SetProvider(_provider);
    }
    private void OnDestroy()
    {
        _subscribers.Clear();
    }

    [Inject]
    public void Construct(IButtonColorsProvider buttonColorsProvider)
    {
        _provider = buttonColorsProvider ?? throw new ArgumentNullException(nameof(_provider)); ;
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