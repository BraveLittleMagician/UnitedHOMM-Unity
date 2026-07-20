#nullable enable

public interface IButtonColorsProvider
{
    void Subscribe(IButtonColorsSubscriber subscriber);
    void Unsubscribe(IButtonColorsSubscriber subscriber);
}