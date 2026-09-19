#nullable enable

public interface IHoldRightSubscriber : IMouseEventSubscriber
{
    void TriggerHoldRightStarted();
    void TriggerHoldRightCanceled();
}