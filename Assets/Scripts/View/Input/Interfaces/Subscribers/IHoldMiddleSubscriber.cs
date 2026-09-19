
#nullable enable

public interface IHoldMiddleSubscriber : IMouseEventSubscriber
{
    void TriggerHoldMiddleStarted();
    void TriggerHoldMiddleCanceled();
}