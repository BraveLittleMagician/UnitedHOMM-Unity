#nullable enable

using System.Collections.Generic;
using System.Linq;

public sealed class AxisAlignedBox
{
    private readonly Dictionary<Axis, (int Min, int Max)> _bounds;

    public AxisAlignedBox(IReadOnlyDictionary<Axis, (int Min, int Max)> bounds) => _bounds = new Dictionary<Axis, (int Min, int Max)>(bounds);
    public AxisAlignedBox(MultipleAxes axes, int size, int minCoordinate = 0)
    {
        _bounds = new Dictionary<Axis, (int, int)>();
        foreach (var axis in axes.GetSeparatedAxes())
            _bounds[axis] = (minCoordinate, minCoordinate + size - 1);
    }

    public bool Contains(Square square)
    {
        foreach (var (axis, (min, max)) in _bounds)
            if (!square.TryGetValue(axis, out int value) || value < min || value > max) return false;
        return true;
    }

    public AxisAlignedBox Expand(int amount)
    {
        var newBounds = _bounds.ToDictionary(
            kv => kv.Key,
            kv => (kv.Value.Min - amount, kv.Value.Max + amount)
        );
        return new AxisAlignedBox(newBounds);
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
}