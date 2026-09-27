#nullable enable

using System;

public static class SquareExtensions
{
    public static SquareCoordinates<TSelf> Coordinates<TSelf>(this TSelf square) where TSelf : struct, ISquare<TSelf>
    => new(square);

    public static bool IsZeroLine<TSelf>(this TSelf square) where TSelf : struct, ISquare<TSelf>
    {
        foreach (var pair in square.Coordinates())
        {
            if (pair.Key == Axis.X) continue;
            if (pair.Value != 0) return false;
        }
        return true;
    }
    public static bool IsSameLine<TSelf>(this TSelf a, TSelf b) where TSelf : struct, ISquare<TSelf> 
    {
        foreach (var pair in a.Coordinates())
        {
            if (pair.Key == Axis.X) continue;
            if (!b.TryGetValue(pair.Key, out var bValue)) return false;
            if (pair.Value != bValue) return false;
        }
        return true;
    }
    public static bool IsNeighborByPosition<TSelf>(this TSelf a, TSelf b) where TSelf : struct, ISquare<TSelf>
    {
        foreach (var pair in a.Coordinates())
        {
            if (pair.Key == Axis.X) continue;
            if (!a.TryGetValue(pair.Key, out var bValue)) return false;
            if (Math.Abs(pair.Value - bValue) > 1) return false;
        }
        return true;
    }
}