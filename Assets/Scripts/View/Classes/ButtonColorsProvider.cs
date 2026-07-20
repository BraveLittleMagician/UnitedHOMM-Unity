#nullable enable

using System.Collections.Generic;

public class ButtonColorsProvider : IButtonColorsProvider
{
    private readonly List<IButtonColorsSubscriber> _subscribers = new();
    private IButtonColors _currentColors;

    public ButtonColorsProvider(IButtonColors initialColors)
    {
        _currentColors = initialColors;
    }

    public void Subscribe(IButtonColorsSubscriber subscriber)
    {
        if (!_subscribers.Contains(subscriber))
            _subscribers.Add(subscriber);
        subscriber.OnButtonColorsChanged(_currentColors);
    }

    public void Unsubscribe(IButtonColorsSubscriber subscriber) => _subscribers.Remove(subscriber);

    public void UpdateColors(IButtonColors newColors)
    {
        _currentColors = newColors;
        foreach (var sub in _subscribers)
            sub.OnButtonColorsChanged(_currentColors);
    }
}