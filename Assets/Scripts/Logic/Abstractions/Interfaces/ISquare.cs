#nullable enable

using System;
using System.Collections.Generic;

public interface ISquare<TSelf> : IAdjacentable<TSelf>, IComparable<TSelf>, IComparable, IComparableByDistance<TSelf>, IComparableByDistance, IEquatable<TSelf> where TSelf : struct, ISquare<TSelf>
{
    public int X { get; init; }
    public bool IsZero { get; }
    public bool IsZeroLine
    {
        get
        {
            foreach (var pair in Coordinates)
            {
                if (pair.Key == Axis.X) continue;
                if (pair.Value != 0) return false;
            }
            return true;
        }
    }
    public MultipleAxes ActiveAxes { get; }
    public IReadOnlyDictionary<Axis, int> Coordinates { get; }
    public bool TryGetValue(Axis axis, out int value);
    public bool IsSameLine(TSelf other)
    {
        foreach (var pair in Coordinates)
        {
            if (pair.Key == Axis.X) continue;
            if (!other.TryGetValue(pair.Key, out var newNodeAxisValue) ||
                (pair.Value - newNodeAxisValue) != 0) return false;
        }
        return true;
    }
    public bool IsNeighborByPosition(TSelf other)
    {
        foreach (var pair in Coordinates)
        {
            if (pair.Key == Axis.X) continue;
            if (!other.TryGetValue(pair.Key, out var newNodeAxisValue) ||
                Math.Abs(pair.Value - newNodeAxisValue) > 1) return false;
        }
        return true;
    }
    public TSelf CopyWith(IReadOnlyDictionary<Axis, int> dictionary);
}