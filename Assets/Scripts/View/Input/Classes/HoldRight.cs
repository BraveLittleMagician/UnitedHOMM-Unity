#nullable enable

public class HoldRight : MouseEvent<IHoldRightSubscriber>, IHoldRightHub
{
    private bool _started = false;

    public void TriggerHoldRightStarted()
    {
        _started = true;
        Notify(s => s.TriggerHoldRightStarted());
    }
    public void TriggerHoldRightCanceled()
    {
        if (_started)
        {
            Notify(s => s.TriggerHoldRightCanceled());
            _started = false;
        }
    }
}