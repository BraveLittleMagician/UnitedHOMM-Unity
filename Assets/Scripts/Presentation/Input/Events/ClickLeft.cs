#nullable enable

public class ClickLeft : MouseEvent<IClickLeftSubscriber>, IClickLeftHub
{
    public void TriggerClickLeft()
    {
        Notify(s => s.TriggerClickLeft());
    }
}