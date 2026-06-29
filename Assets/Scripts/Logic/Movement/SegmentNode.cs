#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

public class SegmentNode<TSquare> : IEquatable<SegmentNode<TSquare>> where TSquare : struct, ISquare<TSquare>
{
    public SegmentNode(TSquare square) : this(square, new Stayables(0)) { }
    public SegmentNode(TSquare square, Stayables stayables)
    {
        Position = square;
        Stayables = stayables;
    }

    public int LastRelativeIndex => Stayables.LastRelativeIndex;
    public int XMax => Position.X + LastRelativeIndex;
    public TSquare Position { get; }
    public Stayables Stayables { get; }
    public HashSet<SegmentNode<TSquare>> Neighbors { get; } = new();

    private static bool IntersectsX((int XMin, int XMax) current, (int XMin, int XMax) other) => !(current.XMax < other.XMin || other.XMax < current.XMin);
    private static bool TouchesX((int XMin, int XMax) current, (int XMin, int XMax) other) => !(current.XMax < (other.XMin - 1) || other.XMax < (current.XMin - 1));

    public bool TouchesX(int x, int length) => TouchesX((Position.X, XMax), (x, x + length));
    public bool IntersectsX(int x, int length) => IntersectsX((Position.X, XMax), (x, x + length));

    public bool Equals(SegmentNode<TSquare>? other) => other != null && Position.Equals(other.Position) && Stayables == other.Stayables;
    public override bool Equals(object? obj)
    {
        if (obj is not SegmentNode<TSquare> other) return false;
        return Equals(other);
    }
    public override int GetHashCode() => HashCode.Combine(Position, LastRelativeIndex);
    public override string ToString()
    {
        string stayables = Stayables.All.Aggregate("", (all, curr) => $"{all} {curr}").Trim();
        stayables = stayables == "" ? "" : $" ({stayables})";
        string ss = Position.Coordinates.Where(p => p.Key != Axis.X).Aggregate("", (all, curr) => $"{all}, {curr.Key}={curr.Value}");
        string s = $"[X={Position.X}..{XMax}{ss}]";
        return $"{s} {Neighbors.Count} N{stayables}";
    }

    public static bool operator ==(SegmentNode<TSquare>? left, SegmentNode<TSquare>? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }
    public static bool operator !=(SegmentNode<TSquare>? left, SegmentNode<TSquare>? right) => !(left == right);
}