#nullable enable
public readonly struct ModifierState
{
    public string Name { get; }
    public int MaxDuration { get; }
    public int CurrentDuration { get; }
    public int MaxCharges { get; }
    public int CurrentCharges { get; }

    public ModifierState(string name, int maxDuration, int currentDuration, int maxCharges, int currentCharges)
    {
        Name = name;
        MaxDuration = maxDuration;
        CurrentDuration = currentDuration;
        MaxCharges = maxCharges;
        CurrentCharges = currentCharges;
    }
}