#nullable enable
using System;

public sealed class CellActionOption
{
    public string Label { get; }
    public string Description { get; }
    public int CurrentCharges { get; }
    public int MaxCharges { get; }
    public Func<IResult> Execute { get; }

    public CellActionOption(
        string label,
        string description,
        int currentCharges,
        int maxCharges,
        Func<IResult> execute)
    {
        Label = label ?? throw new ArgumentNullException(nameof(label));
        Description = description ?? string.Empty;
        CurrentCharges = currentCharges;
        MaxCharges = maxCharges;
        Execute = execute ?? throw new ArgumentNullException(nameof(execute));
    }
}