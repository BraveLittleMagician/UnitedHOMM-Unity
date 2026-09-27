#nullable enable

using System;

public readonly struct LineKey : IEquatable<LineKey>
{
    public LineKey(int y, int z, int w) => (Y, Z, W) = (y, z, w);

    public int Y { get; }
    public int Z { get; }
    public int W { get; }

    public LineKey Shift<TSquare>(TSquare original) where TSquare : struct, ISquare<TSquare>
    {
        var newLine = new LineKey (Y, Z, W);

        if (Y != 0 && original.TryGetValue(Axis.Y, out int y)) newLine = newLine.WithValue(Axis.Y, y + Y);
        if (Z != 0 && original.TryGetValue(Axis.Z, out int z)) newLine = newLine.WithValue(Axis.Z, z + Z);
        if (W != 0 && original.TryGetValue(Axis.W, out int w)) newLine = newLine.WithValue(Axis.W, w + W);

        return newLine;
    }

    private LineKey WithValue(Axis axis, int value) => axis switch
    {
        Axis.Y => new LineKey(value, Z, W),
        Axis.Z => new LineKey(Y, value, W),
        Axis.W => new LineKey(Y, Z, value),
        _ => throw new ArgumentOutOfRangeException(nameof(axis), axis, "Неизвестная ось")
    };

    public bool Equals(LineKey other) => Y == other.Y && Z == other.Z && W == other.W;
    public override bool Equals(object? obj) => obj is LineKey other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Y, Z, W);
    public override string ToString() => $"[Y={Y}, Z={Z}, W={W}]";

    public static bool operator ==(LineKey left, LineKey right) => left.Equals(right);
    public static bool operator !=(LineKey left, LineKey right) => !(left == right);
}