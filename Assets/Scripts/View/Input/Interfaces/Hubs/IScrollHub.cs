#nullable enable

public interface IScrollHub
{
    void RegisterSubscriber(IScrollSubscriber sub);
    void UnregisterSubscriber(IScrollSubscriber sub);
    void TriggerScroll(float delta);
}