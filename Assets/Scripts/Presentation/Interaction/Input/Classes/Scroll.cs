#nullable enable

public class Scroll : MouseEvent<IScrollSubscriber>, IScrollHub
{
    public void TriggerScroll(float delta)
    {
        Notify(s => s.Scroll(delta));
    }
}