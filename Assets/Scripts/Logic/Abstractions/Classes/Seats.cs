#nullable enable

using System;

public readonly struct Seats :  IEquatable<Seats>
{
    public Seats(int countOfSides, int countOfPlayersOnSide)
    {
        CountOfSides = Math.Max(countOfSides, 2);
        CountOfPlayersOnSide = Math.Max(countOfPlayersOnSide, 1);
    }
    public int CountOfSides { get; }
    public int CountOfPlayersOnSide { get; }
    public override string ToString() => $"{CountOfSides}-{CountOfPlayersOnSide}";

    public bool Equals(Seats other) => CountOfSides == other.CountOfSides && CountOfPlayersOnSide == other.CountOfPlayersOnSide;
    public override bool Equals(object? obj) => obj is Seats other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(CountOfSides, CountOfPlayersOnSide);

    public static bool operator ==(Seats left, Seats right) => left.Equals(right);
    public static bool operator !=(Seats left, Seats right) => !(left == right);
}