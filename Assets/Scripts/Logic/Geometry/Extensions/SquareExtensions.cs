#nullable enable

using System;
using System.Collections.Generic;

public static class SquareExtensions
{
    public static SquareCoordinates<TSelf> Coordinates<TSelf>(this TSelf square) where TSelf : struct, ISquare<TSelf>
    => new(square);

    /*
    public static Square WithValuesFrom<TSquare>(this Square original, TSquare pos) where TSquare : struct, ISquare<TSquare>
    {
        if (pos.TryGetValue(Axis.X, out int posX)) original = original.WithValue(Axis.X, posX);
        if (pos.TryGetValue(Axis.Y, out int posY)) original = original.WithValue(Axis.Y, posY);
        if (pos.TryGetValue(Axis.Z, out int posZ)) original = original.WithValue(Axis.Z, posZ);
        if (pos.TryGetValue(Axis.W, out int posW)) original = original.WithValue(Axis.W, posW);
        return original;
    }
    */

    public static TSquare WithValuesFrom<TSquare>(this TSquare original, TSquare pos) where TSquare : struct, ISquare<TSquare>
    {
        if (pos.TryGetValue(Axis.X, out int posX)) original = original.WithValue(Axis.X, posX);
        if (pos.TryGetValue(Axis.Y, out int posY)) original = original.WithValue(Axis.Y, posY);
        if (pos.TryGetValue(Axis.Z, out int posZ)) original = original.WithValue(Axis.Z, posZ);
        if (pos.TryGetValue(Axis.W, out int posW)) original = original.WithValue(Axis.W, posW);
        return original;
    }
    public static TSquare WithOffset<TSquare>(this TSquare original, int dx, int dy, int dz, int dw) where TSquare : struct, ISquare<TSquare>
    {
        if (dx != 0 && original.TryGetValue(Axis.X, out int x)) original = original.WithValue(Axis.X, x + dx);
        if (dy != 0 && original.TryGetValue(Axis.Y, out int y)) original = original.WithValue(Axis.Y, y + dy);
        if (dz != 0 && original.TryGetValue(Axis.Z, out int z)) original = original.WithValue(Axis.Z, z + dz);
        if (dw != 0 && original.TryGetValue(Axis.W, out int w)) original = original.WithValue(Axis.W, w + dw);
        return original;
    }
    public static TSquare WithOffset<TSquare>(this TSquare original, TSquare pos) where TSquare : struct, ISquare<TSquare>
    {
        if (original.TryGetValue(Axis.X, out int x) && pos.TryGetValue(Axis.X, out int dx)) original = original.WithValue(Axis.X, x + dx);
        if (original.TryGetValue(Axis.Y, out int y) && pos.TryGetValue(Axis.Y, out int dy)) original = original.WithValue(Axis.Y, y + dy);
        if (original.TryGetValue(Axis.Z, out int z) && pos.TryGetValue(Axis.Z, out int dz)) original = original.WithValue(Axis.Z, z + dz);
        if (original.TryGetValue(Axis.Y, out int w) && pos.TryGetValue(Axis.W, out int dw)) original = original.WithValue(Axis.W, w + dw);
        return original;
    }
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
            if (!b.TryGetValue(pair.Key, out var bValue)) return false;
            if (Math.Abs(pair.Value - bValue) > 1) return false;
        }
        return true;
    }
}