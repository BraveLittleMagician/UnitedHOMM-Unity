#nullable enable

public interface IScrollHub
{
    void RegisterSubscriber(IScrollSubscriber sub);
    void UnregisterSubscriber(IScrollSubscriber sub);
    void TriggerScroll(float delta);
}

public interface IScrollSubscriber : IMouseEventSubscriber
{
    void Scroll(float scrollDelta);
}