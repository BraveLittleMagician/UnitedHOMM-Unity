#nullable enable

using System;
using System.Collections.Generic;

public sealed class AxisAlignedBox : IEquatable<AxisAlignedBox>
{
    public Square Size { get; }

    public AxisAlignedBox(Square size) => Size = size;

    public bool Contains(Square position) =>
        position.X >= 0 && position.X < Size.X &&
        position.Y >= 0 && position.Y < Size.Y &&
        position.Z >= 0 && position.Z < Size.Z &&
        position.W >= 0 && position.W < Size.W;

    public bool Equals(AxisAlignedBox other) => Size == other.Size;
    public override bool Equals(object? obj) => obj is AxisAlignedBox other && Equals(other);
    public override int GetHashCode() => Size.GetHashCode();
    public override string ToString() => $"Size {Size}";

    public static bool operator ==(AxisAlignedBox l, AxisAlignedBox r) => l.Equals(r);
    public static bool operator !=(AxisAlignedBox l, AxisAlignedBox r) => !(l == r);

    public static AxisAlignedBox FromConfig(ConfigOfBoard config)
    {
        int n = config.FieldSize;
        int wLayers = 1 + (config.WDown ? 1 : 0) + (config.WUp ? 1 : 0);

        return config.Axes switch
        {
            MultipleAxesFromTwo.Two => new AxisAlignedBox(new Square { X = n, Y = n, Z = 1, W = 1 }),
            MultipleAxesFromTwo.Three => new AxisAlignedBox(new Square { X = n, Y = n, Z = n, W = 1 }),
            MultipleAxesFromTwo.Four => new AxisAlignedBox(new Square { X = n, Y = n, Z = n, W = wLayers }),
            _ => throw new ArgumentOutOfRangeException(nameof(config.Axes))
        };
    }

}