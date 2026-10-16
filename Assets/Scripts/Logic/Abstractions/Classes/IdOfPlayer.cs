#nullable enable

using System;

public readonly struct IdOfPlayer : IEquatable<IdOfPlayer>
{
    public IdOfPlayer(int indexOfSide, int indexOfPlayer)
    {
        IndexOfSide = Math.Max(indexOfSide, 0);
        IndexOfPlayerOnSide = Math.Max(indexOfPlayer, 0);
    }

    public int IndexOfSide { get; }
    public int IndexOfPlayerOnSide { get; }
    public override string ToString() => $"{IndexOfSide}-{IndexOfPlayerOnSide}";

    public bool Equals(IdOfPlayer other) => IndexOfSide == other.IndexOfSide && IndexOfPlayerOnSide == other.IndexOfPlayerOnSide;
    public override bool Equals(object? obj) => obj is IdOfPlayer other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(IndexOfSide, IndexOfPlayerOnSide);

    public static bool operator ==(IdOfPlayer left, IdOfPlayer right) => left.Equals(right);
    public static bool operator !=(IdOfPlayer left, IdOfPlayer right) => !(left == right);
}