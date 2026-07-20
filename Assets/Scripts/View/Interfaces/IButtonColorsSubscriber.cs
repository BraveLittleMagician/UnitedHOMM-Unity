#nullable enable

public interface IButtonColorsSubscriber
{
    void OnButtonColorsChanged(IButtonColors colors);
    void SetProvider(IButtonColorsProvider provider);
}