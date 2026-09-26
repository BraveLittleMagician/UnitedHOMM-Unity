#nullable enable

public class ClickRight : MouseEvent<IClickRightSubscriber>, IClickRightHub
{
    public void TriggerClickRight()
    {
        Notify(s => s.TriggerClickRight());
    }
}