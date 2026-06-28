#nullable enable


using System;

public readonly struct IndexOfPlayer : IEquatable<IndexOfPlayer>
{
    public IndexOfPlayer(int indexOfSide, int indexOfPlayer)
    {
        IndexOfSide = Math.Max(indexOfSide, 0);
        IndexOnPlayerOnSide = Math.Max(indexOfPlayer, 0);
    }

    public int IndexOfSide { get; }
    public int IndexOnPlayerOnSide { get; }
    public override string ToString() => $"{IndexOfSide}-{IndexOnPlayerOnSide}";

    public bool Equals(IndexOfPlayer other) => IndexOfSide == other.IndexOfSide && IndexOnPlayerOnSide == other.IndexOnPlayerOnSide;
    public override bool Equals(object? obj) => obj is IndexOfPlayer other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(IndexOfSide, IndexOnPlayerOnSide);

    public static bool operator ==(IndexOfPlayer left, IndexOfPlayer right) => left.Equals(right);
    public static bool operator !=(IndexOfPlayer left, IndexOfPlayer right) => !(left == right);
}