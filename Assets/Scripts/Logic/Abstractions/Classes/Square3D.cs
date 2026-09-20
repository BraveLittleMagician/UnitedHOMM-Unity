#nullable enable

using System;
using System.Collections.Generic;

public readonly struct Square3D : ISquare<Square3D>
{
    public Square3D(int start) { X = start; Y = start; Z = start; }
    public Square3D(IReadOnlyDictionary<Axis, int> dictionary)
    {
        X = 0; Y = 0; Z = 0;
        foreach (var (axis, value) in dictionary)
        {
            switch (axis)
            {
                case Axis.X: X = value; break;
                case Axis.Y: Y = value; break;
                case Axis.Z: Z = value; break;
            }
        }
    }

    private long SquaredDistanceFromCenter => (long)X * X + (long)Y * Y + (long)Z * Z;
    public int X { get; init; }
    public int Y { get; init; }
    public int Z { get; init; }
    public bool IsZero => (X | Y | Z) == 0;
    public MultipleAxes ActiveAxes => MultipleAxes.Three;
    public IReadOnlyDictionary<Axis, int> Coordinates => new Square3DCoordinates (this);
    public static Square3D Zero => new(0);

    private static Square3D ApplyOperation(Square3D l, Square3D r, Func<int, int, int> op) => new()
    {
        X = op(l.X, r.X),
        Y = op(l.Y, r.Y),
        Z = op(l.Z, r.Z),
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
        }
        ;

        return changed;
    }
    public bool IsAdjacent(Square3D other)
    {
        int diffX = Math.Abs(X - other.X);
        int diffY = Math.Abs(Y - other.Y);
        int diffZ = Math.Abs(Z - other.Z);
        if (diffX == 0 && diffY == 0 && diffZ == 0) return false;
        return diffX < 2 && diffY < 2 && diffZ < 2;
    }
    public int CompareByDistanseTo(Square3D other)
    {
        long thisDistance = SquaredDistanceFromCenter;
        long otherDistance = other.SquaredDistanceFromCenter;
        return thisDistance.CompareTo(otherDistance);
    }
    public int CompareTo(Square3D other)
    {
        int result = Z.CompareTo(other.Z);
        if (result != 0) return result;
        result = Y.CompareTo(other.Y);
        if (result != 0) return result;
        return X.CompareTo(other.X);
    }
    public int CompareByDistanceTo(object? obj)
    {
        if (obj is Square3D other)
            return CompareByDistanseTo(other);
        return 0;
    }
    public int CompareTo(object? obj)
    {
        if (obj is Square3D other) return CompareTo(other);
        throw new ArgumentException($"Объект должен иметь тип {nameof(Square3D)}", nameof(obj));
    }
    public override string ToString() => $"({X}, {Y}, {Z})";
    public Square3D CopyWith(IReadOnlyDictionary<Axis, int> dictionary) => new(dictionary);
    public bool Equals(Square3D other) => X == other.X && Y == other.Y && Z == other.Z;
    public override bool Equals(object? obj) => obj is Square3D other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(X, Y, Z);
    public static bool operator ==(Square3D left, Square3D right) => left.Equals(right);
    public static bool operator !=(Square3D left, Square3D right) => !(left == right);

    public static bool operator <(Square3D l, Square3D r) => l.CompareTo(r) < 0;
    public static bool operator >(Square3D l, Square3D r) => l.CompareTo(r) > 0;
    public static bool operator <=(Square3D l, Square3D r) => l.CompareTo(r) <= 0;
    public static bool operator >=(Square3D l, Square3D r) => l.CompareTo(r) >= 0;
    public static Square3D operator +(Square3D l, Square3D r) => ApplyOperation(l, r, (a, b) => a + b);
    public static Square3D operator -(Square3D l, Square3D r) => ApplyOperation(l, r, (a, b) => a - b);
    public static Square3D operator *(Square3D l, Square3D r) => ApplyOperation(l, r, (a, b) => a * b);
    public static Square3D operator /(Square3D l, Square3D r) => ApplyOperation(l, r, (a, b) => b != 0 ? a / b : 0);
    public static Square3D operator %(Square3D l, Square3D r) => ApplyOperation(l, r, (a, b) => b != 0 ? a % b : 0);
}