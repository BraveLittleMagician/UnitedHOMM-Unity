#nullable enable

using System;
using System.Collections.Generic;

public readonly struct Square2D : ISquare<Square2D>
{
    public Square2D(int start) { X = start; Y = start; }
    public Square2D(IReadOnlyDictionary<Axis, int> dictionary)
    {
        X = 0; Y = 0;
        foreach (var (axis, value) in dictionary)
        {
            switch (axis)
            {
                case Axis.X: X = value; break;
                case Axis.Y: Y = value; break;
            }
        }
    }

    private long SquaredDistanceFromCenter => (long)X * X + (long)Y * Y;
    public int X { get; init; }
    public int Y { get; init; }
    public bool IsZero => (X | Y) == 0;
    public MultipleAxes ActiveAxes => MultipleAxes.Two;
    public IReadOnlyDictionary<Axis, int> Coordinates => new Square2DCoordinates(this);
    public static Square2D Zero { get; } = new(0);

    private static Square2D ApplyOperation(Square2D l, Square2D r, Func<int, int, int> op) => new()
    {
        X = op(l.X, r.X),
        Y = op(l.Y, r.Y),
    };

    public bool TryGetValue(Axis axis, out int value)
    {
        bool changed = false;
        value = 0;
        switch (axis)
        {
            case Axis.X: value = X; changed = true; break;
            case Axis.Y: value = Y; changed = true; break;
        };

        return changed;
    }
    public bool IsAdjacent(Square2D other)
    {
        int diffX = Math.Abs(X - other.X);
        int diffY = Math.Abs(Y - other.Y);
        if (diffX == 0 && diffY == 0) return false;
        return diffX < 2 && diffY < 2;
    }
    public int CompareByDistanceTo(Square2D other)
    {
        long thisDistance = SquaredDistanceFromCenter;
        long otherDistance = other.SquaredDistanceFromCenter;
        return thisDistance.CompareTo(otherDistance);
    }
    public int CompareTo(Square2D other)
    {
        int result = Y.CompareTo(other.Y);
        if (result != 0) return result;
        return X.CompareTo(other.X);
    }
    public int CompareByDistanceTo(object? obj)
    {
        if (obj is not Square2D other) return 0;
        return CompareByDistanceTo(other);
    }
    public int CompareTo(object? obj)
    {
        if (obj is Square2D other) return CompareTo(other);
        throw new ArgumentException($"Объект должен иметь тип {nameof(Square2D)}", nameof(obj));
    }
    public override string ToString() => $"[{X}, {Y}]";
    public Square2D CopyWith(IReadOnlyDictionary<Axis, int> dictionary) => new (dictionary);
    public bool Equals(Square2D other) => X == other.X && Y == other.Y;
    public override bool Equals(object? obj) => obj is Square2D other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(X, Y);

    public static bool operator ==(Square2D left, Square2D right) => left.Equals(right);
    public static bool operator !=(Square2D left, Square2D right) => !(left == right);
    public static bool operator <(Square2D l, Square2D r) => l.CompareTo(r) < 0;
    public static bool operator >(Square2D l, Square2D r) => l.CompareTo(r) > 0;
    public static bool operator <=(Square2D l, Square2D r) => l.CompareTo(r) <= 0;
    public static bool operator >=(Square2D l, Square2D r) => l.CompareTo(r) >= 0;
    public static Square2D operator +(Square2D l, Square2D r) => ApplyOperation(l, r, (a, b) => a + b);
    public static Square2D operator -(Square2D l, Square2D r) => ApplyOperation(l, r, (a, b) => a - b);
    public static Square2D operator *(Square2D l, Square2D r) => ApplyOperation(l, r, (a, b) => a * b);
    public static Square2D operator /(Square2D l, Square2D r) => ApplyOperation(l, r, (a, b) => b != 0 ? a / b : 0);
    public static Square2D operator %(Square2D l, Square2D r) => ApplyOperation(l, r, (a, b) => b != 0 ? a % b : 0);
}