#nullable enable

public interface IClickRightHub
{
    void RegisterSubscriber(IClickRightSubscriber sub);
    void UnregisterSubscriber(IClickRightSubscriber sub);
    void TriggerClickRight();
}

public interface IClickRightSubscriber : IMouseEventSubscriber
{
    void TriggerClickRight();
}