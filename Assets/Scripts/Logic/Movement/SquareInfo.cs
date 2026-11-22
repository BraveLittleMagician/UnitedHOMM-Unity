#nullable enable
using System;

public readonly struct SquareInfo : IEquatable<SquareInfo>
{
    public Stayable Stayable { get; }
    public Attackable Attackable { get; }

    public SquareInfo(Stayable stayable, Attackable attackable)
    {
        Stayable = stayable;
        Attackable = attackable;
    }

    public bool CanStopAt => Stayable == Stayable.Stay;
    public bool IsAttackable => Attackable == Attackable.Attack;

    public bool Equals(SquareInfo other)
        => Stayable == other.Stayable && Attackable == other.Attackable;

    public override bool Equals(object? obj)
        => obj is SquareInfo other && Equals(other);

    public override int GetHashCode()
        => HashCode.Combine((int)Stayable, (int)Attackable);

    public static bool operator ==(SquareInfo a, SquareInfo b) => a.Equals(b);
    public static bool operator !=(SquareInfo a, SquareInfo b) => !a.Equals(b);

    public override string ToString()
        => $"(Stayable: {Stayable}, Attackable: {Attackable})";
}