#nullable enable

public class HoldMiddle : MouseEvent<IHoldMiddleSubscriber>, IHoldMiddleHub
{
    private bool _started = false;

    public void TriggerHoldMiddleStarted()
    {
        _started = true;
        Notify(s => s.TriggerHoldMiddleStarted());
    }

    public void TriggerHoldMiddleCanceled()
    {
        if (_started)
        {
            Notify(s => s.TriggerHoldMiddleCanceled());
            _started = false;
        }
    }
}