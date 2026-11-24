#nullable enable
using System;

public readonly struct PositionInfo : IEquatable<PositionInfo>
{
    public Stayable Stayable { get; }
    public Attackable Attackable { get; }

    public PositionInfo(Stayable stayable, Attackable attackable)
    {
        Stayable = stayable;
        Attackable = attackable;
    }

    public bool CanStopAt => Stayable == Stayable.Stay;
    public bool IsAttackable => Attackable == Attackable.Attack;

    public bool Equals(PositionInfo other)
        => Stayable == other.Stayable && Attackable == other.Attackable;

    public override bool Equals(object? obj)
        => obj is PositionInfo other && Equals(other);

    public override int GetHashCode()
        => HashCode.Combine((int)Stayable, (int)Attackable);

    public static bool operator ==(PositionInfo a, PositionInfo b) => a.Equals(b);
    public static bool operator !=(PositionInfo a, PositionInfo b) => !a.Equals(b);

    public override string ToString()
        => $"(Stayable: {Stayable}, Attackable: {Attackable})";
}