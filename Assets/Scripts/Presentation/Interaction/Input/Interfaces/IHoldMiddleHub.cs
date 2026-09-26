
#nullable enable

public interface IHoldMiddleHub
{
    void RegisterSubscriber(IHoldMiddleSubscriber sub);
    void UnregisterSubscriber(IHoldMiddleSubscriber sub);
    void TriggerHoldMiddleStarted();
    void TriggerHoldMiddleCanceled();
}