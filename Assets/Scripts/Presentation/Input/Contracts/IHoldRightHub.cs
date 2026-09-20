#nullable enable

public interface IHoldRightHub
{
    void RegisterSubscriber(IHoldRightSubscriber sub);
    void UnregisterSubscriber(IHoldRightSubscriber sub);
    void TriggerHoldRightStarted();
    void TriggerHoldRightCanceled();
}

public interface IHoldRightSubscriber : IMouseEventSubscriber
{
    void TriggerHoldRightStarted();
    void TriggerHoldRightCanceled();
}