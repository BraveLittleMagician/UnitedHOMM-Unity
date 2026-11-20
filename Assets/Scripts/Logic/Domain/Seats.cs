#nullable enable

using System;

public readonly struct Seats :  IEquatable<Seats>
{
    private readonly int _countOfSeats;
    private readonly int _countOfPlayersOnSide;

    public Seats(int countOfSides, int countOfPlayersOnSide)
    {
        _countOfSeats = Math.Max(countOfSides, 2);
        _countOfPlayersOnSide = Math.Max(countOfPlayersOnSide, 1);
    }
    public void Deconstruct(out int countOfSides, out int countOfPlayersOnSide)
    {
        countOfSides = CountOfSides;
        countOfPlayersOnSide = CountOfPlayersOnSide;
    }
    public int CountOfSides => _countOfSeats < 2 ? 2 : _countOfSeats;
    public int CountOfPlayersOnSide => _countOfPlayersOnSide < 1 ? 1 : _countOfPlayersOnSide;
    public override string ToString() => $"{CountOfSides}-{CountOfPlayersOnSide}";

    public bool Equals(Seats other) => CountOfSides == other.CountOfSides && CountOfPlayersOnSide == other.CountOfPlayersOnSide;
    public override bool Equals(object? obj) => obj is Seats other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(CountOfSides, CountOfPlayersOnSide);

    public static bool operator ==(Seats left, Seats right) => left.Equals(right);
    public static bool operator !=(Seats left, Seats right) => !(left == right);
}