#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

public sealed class AxisAlignedBox : IEquatable<AxisAlignedBox>
{
    private readonly Dictionary<Axis, (int Min, int Max)> _bounds;

    public AxisAlignedBox(IReadOnlyDictionary<Axis, (int Min, int Max)> bounds) => _bounds = new Dictionary<Axis, (int Min, int Max)>(bounds);
    public AxisAlignedBox(MultipleAxes axes, int size, bool wup, bool wdown)
    {
        _bounds = new Dictionary<Axis, (int, int)>();
        foreach (var axis in axes.GetSeparatedAxes())
            _bounds[axis] = (0, size - 1);
        if (axes == MultipleAxes.Four)
            _bounds[Axis.W] = (wdown ? -1 : 0, wup ? 1 : 0);
    }

    public IReadOnlyDictionary<Axis, (int Min, int Max)> Bounds => _bounds;

    public bool Contains(Square square)
    {
        foreach (var (axis, (min, max)) in _bounds)
            if (!square.TryGetValue(axis, out int value) || value < min || value > max) return false;
        return true;
    }
    public bool TryGetBounds(Axis axis, out int min, out int max)
    {
        if (_bounds.TryGetValue(axis, out var b))
        {
            min = b.Min;
            max = b.Max;
            return true;
        }
        min = max = 0;
        return false;
    }

    public override bool Equals(object? obj) => obj is AxisAlignedBox other && Equals(other);
    public bool Equals(AxisAlignedBox? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (_bounds.Count != other._bounds.Count) return false;

        foreach (var (axis, range) in _bounds)
        {
            if (!other._bounds.TryGetValue(axis, out var otherRange)) return false;
            if (otherRange.Min != range.Min || otherRange.Max != range.Max) return false;
        }
        return true;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var axis in new[] { Axis.X, Axis.Y, Axis.Z, Axis.W })
        {
            hash.Add(axis);
            if (_bounds.TryGetValue(axis, out var range))
                hash.Add(HashCode.Combine(range.Min, range.Max));
        }
        return hash.ToHashCode();
    }

    public static bool operator ==(AxisAlignedBox? left, AxisAlignedBox? right) => left is null ? right is null : left.Equals(right);
    public static bool operator !=(AxisAlignedBox? left, AxisAlignedBox? right) => !(left == right);

}