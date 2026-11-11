#nullable enable
using System;

public readonly struct EnPassantTarget
{
    public IPiece Victim { get; }
    public Square VictimCell { get; }
    public Square ViaCell { get; }

    public EnPassantTarget(IPiece victim, Square victimCell, Square viaCell)
    {
        Victim = victim ?? throw new ArgumentNullException(nameof(victim));
        VictimCell = victimCell;
        ViaCell = viaCell;
    }
}