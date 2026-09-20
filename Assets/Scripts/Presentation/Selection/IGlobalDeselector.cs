#nullable enable

public interface IGlobalDeselector
{
    void RegisterSubscriber(IGlobalDeselectSubscriber subscriber);
    void UnregisterSubscriber(IGlobalDeselectSubscriber subscriber);
    void TriggerGlobalClick();
}