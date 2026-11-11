#nullable enable

using System;
using System.Collections.Generic;

internal readonly struct PieceActionFlags : IEquatable<PieceActionFlags>
{
    public bool HasMovedThisTurn { get; init; }
    public bool HasTakenFirstActionOfGame { get; init; }

    public PieceActionFlags(bool hasMovedThisTurn, bool hasTakenFirstActionOfGame)
    {
        HasMovedThisTurn = hasMovedThisTurn;
        HasTakenFirstActionOfGame = hasTakenFirstActionOfGame;
    }

    public void Deconstruct(out bool hasMovedThisTurn, out bool hasTakenFirstActionOfGame)
    {
        hasMovedThisTurn = HasMovedThisTurn;
        hasTakenFirstActionOfGame = HasTakenFirstActionOfGame;
    }

    public bool Equals(PieceActionFlags other) =>
        HasMovedThisTurn == other.HasMovedThisTurn &&
        HasTakenFirstActionOfGame == other.HasTakenFirstActionOfGame;

    public override bool Equals(object? obj) => obj is PieceActionFlags other && Equals(other);

    public override int GetHashCode() =>
        HashCode.Combine(HasMovedThisTurn, HasTakenFirstActionOfGame);

    public static bool operator ==(PieceActionFlags l, PieceActionFlags r) => l.Equals(r);
    public static bool operator !=(PieceActionFlags l, PieceActionFlags r) => !l.Equals(r);

    public override string ToString() =>
        $"PieceActionFlags {{ HasMovedThisTurn = {HasMovedThisTurn}, HasTakenFirstActionOfGame = {HasTakenFirstActionOfGame} }}";
}
