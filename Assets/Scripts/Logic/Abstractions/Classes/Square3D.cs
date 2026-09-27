#nullable enable

using System;
using System.Collections.Generic;

public readonly struct Square3D : ISquare<Square3D>
{
    public int X { get; init; }
    public int Y { get; init; }
    public int Z { get; init; }
    public bool IsZero => (X | Y | Z) == 0;
    public int CountOfAxes => 3;
    public MultipleAxes ActiveAxes => MultipleAxes.Three;
    public KeyValuePair<Axis, int> GetCoordinateAt(int index) => index switch
    {
        0 => new(Axis.X, X),
        1 => new(Axis.Y, Y),
        2 => new(Axis.Z, Z),
        _ => throw new ArgumentOutOfRangeException(nameof(index), $"Индекс {index} вне диапазона [0..2] для {nameof(Square3D)}")
    };
    private long SquaredDistanceFromCenter => (long)X * X + (long)Y * Y + (long)Z * Z;
    public static Square3D Zero => new();

    private static Square3D ApplyOperation(Square3D l, Square3D r, Func<int, int, int> op) => new()
    {
        X = op(l.X, r.X),
        Y = op(l.Y, r.Y),
        Z = op(l.Z, r.Z),
    };

    public bool TryGetValue(Axis axis, out int value)
    {
        switch (axis)
        {
            case Axis.X: value = X; return true;
            case Axis.Y: value = Y; return true;
            case Axis.Z: value = Z; return true;
            default: value = 0; return false;
        }
    }
    public Square3D CopyWith(IReadOnlyDictionary<Axis, int> dictionary)
    {
        int x = X, y = Y, z = Z;
        foreach (var pair in dictionary)
        {
            switch (pair.Key)
            {
                case Axis.X: x = pair.Value; break;
                case Axis.Y: y = pair.Value; break;
                case Axis.Z: z = pair.Value; break;
            }
        }
        return new Square3D { X = x, Y = y, Z = z };
    }

    public bool IsAdjacent(Square3D other)
    {
        int diffX = Math.Abs(X - other.X);
        int diffY = Math.Abs(Y - other.Y);
        int diffZ = Math.Abs(Z - other.Z);
        if (diffX == 0 && diffY == 0 && diffZ == 0) return false;
        return diffX < 2 && diffY < 2 && diffZ < 2;
    }
    public int CompareByDistanceTo(Square3D other)
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
        if (obj is not Square3D other) return 0;
        return CompareByDistanceTo(other);
    }
    public int CompareTo(object? obj)
    {
        if (obj is Square3D other) return CompareTo(other);
        throw new ArgumentException($"Объект должен иметь тип {nameof(Square3D)}", nameof(obj));
    }
    public bool Equals(Square3D other) => X == other.X && Y == other.Y && Z == other.Z;
    public override bool Equals(object? obj) => obj is Square3D other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(X, Y, Z);
    public override string ToString() => $"({X}, {Y}, {Z})";

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