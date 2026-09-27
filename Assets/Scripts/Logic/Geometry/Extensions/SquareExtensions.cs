#nullable enable

using System;

public static class SquareExtensions
{
    public static bool IsZeroLine<TS>(this ISquare<TS> square) where TS : struct, ISquare<TS> 
    {
        foreach (var pair in square.Coordinates)
        {
            if (pair.Key == Axis.X) continue;
            if (pair.Value != 0) return false;
        }
        return true;
    }
    public static bool IsSameLine<TS>(this ISquare<TS> a, TS b) where TS : struct, ISquare<TS> 
    {
        foreach (var pair in a.Coordinates)
        {
            if (pair.Key == Axis.X) continue;
            if (!b.TryGetValue(pair.Key, out var newNodeAxisValue) ||
                (pair.Value - newNodeAxisValue) != 0) return false;
        }
        return true;
    }
    public static bool IsNeighborByPosition<TS>(this ISquare<TS> a, TS b) where TS : struct, ISquare<TS>
    {
        foreach (var pair in a.Coordinates)
        {
            if (pair.Key == Axis.X) continue;
            if (!a.TryGetValue(pair.Key, out var newNodeAxisValue) ||
                Math.Abs(pair.Value - newNodeAxisValue) > 1) return false;
        }
        return true;
    }
}