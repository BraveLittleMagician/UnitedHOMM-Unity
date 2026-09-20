#nullable enable

public interface IClickLeftHub
{
    void RegisterSubscriber(IClickLeftSubscriber sub);
    void UnregisterSubscriber(IClickLeftSubscriber sub);
    void TriggerClickLeft();
}

public interface IClickLeftSubscriber : IMouseEventSubscriber
{
    void TriggerClickLeft();
}