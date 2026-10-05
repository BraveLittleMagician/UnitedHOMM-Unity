#nullable enable

using System;

public readonly struct IndexOfPlayer : IEquatable<IndexOfPlayer>
{
    public IndexOfPlayer(int indexOfSide, int indexOfPlayer)
    {
        IndexOfSide = Math.Max(indexOfSide, 0);
        IndexOfPlayerOnSide = Math.Max(indexOfPlayer, 0);
    }

    public int IndexOfSide { get; }
    public int IndexOfPlayerOnSide { get; }
    public override string ToString() => $"{IndexOfSide}-{IndexOfPlayerOnSide}";

    public bool Equals(IndexOfPlayer other) => IndexOfSide == other.IndexOfSide && IndexOfPlayerOnSide == other.IndexOfPlayerOnSide;
    public override bool Equals(object? obj) => obj is IndexOfPlayer other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(IndexOfSide, IndexOfPlayerOnSide);

    public static bool operator ==(IndexOfPlayer left, IndexOfPlayer right) => left.Equals(right);
    public static bool operator !=(IndexOfPlayer left, IndexOfPlayer right) => !(left == right);
}