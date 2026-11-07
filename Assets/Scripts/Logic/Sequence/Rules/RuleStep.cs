#nullable enable
using System;

public readonly struct RuleStep
{
    public Square Offset { get; }
    public Stayable Stayable { get; }
    public Attackable Attackable { get; }
    public int Repeat { get; }

    public RuleStep(Square offset, Stayable stayable, int repeat, Attackable attackable = Attackable.NotAttack)
    {
        if (offset.IsZero)
            throw new ArgumentException("Offset must not be zero.", nameof(offset));
        if (!offset.IsAdjacent(Square.Zero))
            throw new ArgumentException(
                "Offset must be a single-step neighbor of origin (|dx|,|dy|,|dz|,|dw| <= 1).",
                nameof(offset));
        if (repeat != -1 && repeat <= 0)
            throw new ArgumentOutOfRangeException(nameof(repeat), "Repeat must be -1 or positive.");
        Offset = offset;
        Stayable = stayable;
        Attackable = attackable;
        Repeat = repeat;
    }
}