#nullable enable
using System;

public readonly struct RuleStep<TSquare> where TSquare : struct, ISquare<TSquare>
{
    public TSquare Offset { get; }
    public Stayable Stayable { get; }
    public int Repeat { get; }

    public RuleStep(TSquare offset, Stayable stayable, int repeat)
    {
        if (offset.IsZero) throw new ArgumentException("Offset must not be zero.", nameof(offset));
        if (!offset.IsAdjacent(default)) throw new ArgumentException("Offset must be a single-step neighbor of origin (|dx|,|dy|,|dz|,|dw| <= 1).", nameof(offset));
        if (repeat != -1 && repeat <= 0) throw new ArgumentOutOfRangeException(nameof(repeat), "Repeat must be -1 or positive.");
        Offset = offset;
        Stayable = stayable;
        Repeat = repeat;
    }
}