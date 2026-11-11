
#nullable enable

public interface IModifier
{
    string ModifierName { get; }
    int MaxDuration { get; }
    int CurrentDuration { get; }
    int MaxCharges { get; }
    int CurrentCharges { get; }
    bool TryConsumeCharge();
    void RestoreCharges(int charges);
    void RestoreDuration(int duration);
    void Attach(ModifierContext context);
    void Detach();
}