#nullable enable
using System;

public readonly struct AmbushTarget
{
    public IPiece Victim { get; }
    public Square VictimCell { get; }

    public AmbushTarget(IPiece victim, Square victimCell)
    {
        Victim = victim ?? throw new ArgumentNullException(nameof(victim));
        VictimCell = victimCell;
    }
}
