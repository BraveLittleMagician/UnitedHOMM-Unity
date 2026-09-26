#nullable enable

public interface IScrollSubscriber : IMouseEventSubscriber
{
    void Scroll(float scrollDelta);
}