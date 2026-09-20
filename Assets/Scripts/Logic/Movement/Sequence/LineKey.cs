#nullable enable

using System;

public readonly struct LineKey : IEquatable<LineKey>
{
    public LineKey(Square2D pos)
    {
        pos.TryGetValue(Axis.Y, out int y); Y = y; 
        Z = 0;
        W = 0;
    }
    public LineKey(Square3D pos)
    {
        pos.TryGetValue(Axis.Y, out int y); Y = y;
        pos.TryGetValue(Axis.Z, out int z); Z = z;
        W = 0;
    }
    public LineKey(Square pos)
    {
        pos.TryGetValue(Axis.Y, out int y); Y = y;
        pos.TryGetValue(Axis.Z, out int z); Z = z;
        pos.TryGetValue(Axis.W, out int w); W = w;
    }
    private LineKey(int y, int z, int w) => (Y, Z, W) = (y, z, w);

    public int Y { get; }
    public int Z { get; }
    public int W { get; }

    public LineKey Shift(Square startSquare) => new (Y + startSquare.Y, Z + startSquare.Z, W + startSquare.W);

    public bool Equals(LineKey other) => Y == other.Y && Z == other.Z && W == other.W;
    public override bool Equals(object? obj) => obj is LineKey other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Y, Z, W);
    public override string ToString() => $"[Y={Y}, Z={Z}, W={W}]";

    public static bool operator ==(LineKey left, LineKey right) => left.Equals(right);
    public static bool operator !=(LineKey left, LineKey right) => !(left == right);
}