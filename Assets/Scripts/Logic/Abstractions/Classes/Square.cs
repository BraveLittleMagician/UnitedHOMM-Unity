#nullable enable

using System;
using System.Collections.Generic;

public readonly struct Square : ISquare<Square>
{
    public Square(int start) { X = start; Y = start; Z = start; W = start; }
    public Square(IReadOnlyDictionary<Axis, int> dictionary)
    {
        X = 0; Y = 0; Z = 0; W = 0;
        foreach (var (axis, value) in dictionary)
        {
            switch (axis)
            {
                case Axis.X: X = value; break;
                case Axis.Y: Y = value; break;
                case Axis.Z: Z = value; break;
                case Axis.W: W = value; break;
            }
        }
    }
    
    private long SquaredDistanceFromCenter => (long)X * X + (long)Y * Y + (long)Z * Z + (long)W * W;
    public int X { get; init; }
    public int Y { get; init; }
    public int Z { get; init; }
    public int W { get; init; }
    public bool IsZero => (X | Y | Z | W) == 0;
    public MultipleAxes ActiveAxes => MultipleAxes.Four;
    public IReadOnlyDictionary<Axis, int> Coordinates => new SquareCoordinates(this);
    public static Square Zero { get; } = new(0);

    private static Square ApplyOperation(Square l, Square r, Func<int, int, int> op) => new()
    {
        X = op(l.X, r.X),
        Y = op(l.Y, r.Y),
        Z = op(l.Z, r.Z),
        W = op(l.W, r.W)
    };

    public bool TryGetValue(Axis axis, out int value)
    {
        bool changed = false;
        value = 0;
        switch (axis)
        {
            case Axis.X: value = X; changed = true; break;
            case Axis.Y: value = Y; changed = true; break;
            case Axis.Z: value = Z; changed = true; break;
            case Axis.W: value = W; changed = true; break;
        };

        return changed;
    }
    public bool IsAdjacent(Square other)
    {
        int diffX = Math.Abs(X - other.X);
        int diffY = Math.Abs(Y - other.Y);
        int diffZ = Math.Abs(Z - other.Z);
        int diffW = Math.Abs(W - other.W);
        if (diffX == 0 && diffY == 0 && diffZ == 0 && diffW == 0) return false;
        return diffX < 2 && diffY < 2 && diffZ < 2 && diffW < 2;
    }
    public int CompareByDistanseTo(Square other)
    {
        long thisDistance = SquaredDistanceFromCenter;
        long otherDistance = other.SquaredDistanceFromCenter;
        return thisDistance.CompareTo(otherDistance);
    }
    public int CompareTo(Square other)
    {
        int result = W.CompareTo(other.W);
        if (result != 0) return result;
        result = Z.CompareTo(other.Z);
        if (result != 0) return result;
        result = Y.CompareTo(other.Y);
        if (result != 0) return result;
        return X.CompareTo(other.X);
    }
    public int CompareByDistanceTo(object? obj)
    {
        if (obj is Square other)
            return CompareByDistanseTo(other);
        return 0;
    }
    public int CompareTo(object? obj)
    {
        if (obj is Square other) return CompareTo(other);
        throw new ArgumentException($"Объект должен иметь тип {nameof(Square)}", nameof(obj));
    }
    public override string ToString() => $"({X}, {Y}, {Z}, {W})";
    public Square CopyWith(IReadOnlyDictionary<Axis, int> dictionary) => new(dictionary);
    public bool Equals(Square other) => X == other.X && Y == other.Y && Z == other.Z && W == other.W;
    public override bool Equals(object? obj) => obj is Square other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(X, Y, Z, W);
    public bool IsSameLine(Square other)
    {
        foreach (var pair in Coordinates)
        {
            if (pair.Key == Axis.X) continue;
            if (!other.TryGetValue(pair.Key, out var newNodeAxisValue) ||
                (pair.Value - newNodeAxisValue) != 0) return false;
        }
        return true;
    }

    public static bool operator ==(Square left, Square right) => left.Equals(right);
    public static bool operator !=(Square left, Square right) => !(left == right);
    public static bool operator <(Square l, Square r) => l.CompareTo(r) < 0;
    public static bool operator >(Square l, Square r) => l.CompareTo(r) > 0;
    public static bool operator <=(Square l, Square r) => l.CompareTo(r) <= 0;
    public static bool operator >=(Square l, Square r) => l.CompareTo(r) >= 0;
    public static Square operator +(Square l, Square r) => ApplyOperation(l, r, (a, b) => a + b);
    public static Square operator -(Square l, Square r) => ApplyOperation(l, r, (a, b) => a - b);
    public static Square operator *(Square l, Square r) => ApplyOperation(l, r, (a, b) => a * b);
    public static Square operator /(Square l, Square r) => ApplyOperation(l, r, (a, b) => b != 0 ? a / b : 0);
    public static Square operator %(Square l, Square r) => ApplyOperation(l, r, (a, b) => b != 0 ? a % b : 0);
}