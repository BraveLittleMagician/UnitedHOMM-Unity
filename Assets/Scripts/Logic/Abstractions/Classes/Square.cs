#nullable enable

using System;
using System.Collections.Generic;

public readonly struct Square : ISquare<Square>
{
    public int X { get; init; }
    public int Y { get; init; }
    public int Z { get; init; }
    public int W { get; init; }
    public bool IsZero => (X | Y | Z | W) == 0;
    public int CountOfAxes => 4;
    public MultipleAxes ActiveAxes => MultipleAxes.Four;
    public KeyValuePair<Axis, int> GetCoordinateAt(int index) => index switch
    {
        0 => new(Axis.X, X),
        1 => new(Axis.Y, Y),
        2 => new(Axis.Z, Z),
        3 => new(Axis.W, W),
        _ => throw new ArgumentOutOfRangeException(nameof(index), $"Индекс {index} вне диапазона [0..3] для {nameof(Square)}")
    };
    private long SquaredDistanceFromCenter => (long)X * X + (long)Y * Y + (long)Z * Z + (long)W * W;
    public static Square Zero => new();

    private static Square ApplyOperation(Square l, Square r, Func<int, int, int> op) => new()
    {
        X = op(l.X, r.X),
        Y = op(l.Y, r.Y),
        Z = op(l.Z, r.Z),
        W = op(l.W, r.W)
    };

    public bool TryGetValue(Axis axis, out int value)
    {
        switch (axis)
        {
            case Axis.X: value = X; return true;
            case Axis.Y: value = Y; return true;
            case Axis.Z: value = Z; return true;
            case Axis.W: value = W; return true;
            default: value = 0; return false;
        }
    }
    public Square CopyWith(IReadOnlyDictionary<Axis, int> dictionary)
    {
        int x = X, y = Y, z = Z, w = W;
        foreach (var pair in dictionary)
        {
            switch (pair.Key)
            {
                case Axis.X: x = pair.Value; break;
                case Axis.Y: y = pair.Value; break;
                case Axis.Z: z = pair.Value; break;
                case Axis.W: w = pair.Value; break;
            }
        }
        return new Square { X = x, Y = y, Z = z, W = w };
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
    public int CompareByDistanceTo(Square other)
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
        if (obj is not Square other) return 0;
        return CompareByDistanceTo(other);
    }
    public int CompareTo(object? obj)
    {
        if (obj is Square other) return CompareTo(other);
        throw new ArgumentException($"Объект должен иметь тип {nameof(Square)}", nameof(obj));
    }
    public bool Equals(Square other) => X == other.X && Y == other.Y && Z == other.Z && W == other.W;
    public override bool Equals(object? obj) => obj is Square other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(X, Y, Z, W);
    public override string ToString() => $"({X}, {Y}, {Z}, {W})";

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