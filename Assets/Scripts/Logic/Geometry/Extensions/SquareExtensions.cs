#nullable enable

using System;

public static class SquareExtensions
{
    public static SquareCoordinates<TSquare> Coordinates<TSquare>(this TSquare square) where TSquare : struct, ISquare<TSquare>
    => new(square);

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
    public static bool IsZeroLine<TSquare>(this TSquare square) where TSquare : struct, ISquare<TSquare>
    {
        foreach (var pair in square.Coordinates())
        {
            if (pair.Key == Axis.X) continue;
            if (pair.Value != 0) return false;
        }
        return true;
    }
    public static bool IsSameLine<TSquare>(this TSquare a, TSquare b) where TSquare : struct, ISquare<TSquare>
    {
        foreach (var pair in a.Coordinates())
        {
            if (pair.Key == Axis.X) continue;
            if (!b.TryGetValue(pair.Key, out var bValue)) return false;
            if (pair.Value != bValue) return false;
        }
        return true;
    }
    public static bool IsNeighborByPosition<TSquare>(this TSquare a, TSquare b) where TSquare : struct, ISquare<TSquare>
    {
        foreach (var pair in a.Coordinates())
        {
            if (pair.Key == Axis.X) continue;
            if (!b.TryGetValue(pair.Key, out var bValue)) return false;
            if (Math.Abs(pair.Value - bValue) > 1) return false;
        }
        return true;
    }
    public static bool IsWithinSquareRadius<TSquare>(this TSquare start, TSquare target, int radius, MultipleAxes activeAxes) where TSquare : struct, ISquare<TSquare>
    {
        var diff = target.Subtract(start);
        for (int i = 0; i < diff.CountOfAxes; i++)
        {
            var coord = diff.GetCoordinateAt(i);
            if (!activeAxes.HasFlag((MultipleAxes)(int)coord.Key)) continue;
            if (Math.Abs(coord.Value) > radius) return false;
        }
        return true;
    }
    public static bool IsWithinCircleRadius<TSquare>(this TSquare start, TSquare target, int radius, MultipleAxes activeAxes) where TSquare : struct, ISquare<TSquare>
    {
        var diff = target.Subtract(start);
        long distSq = 0;
        for (int i = 0; i < diff.CountOfAxes; i++)
        {
            var coord = diff.GetCoordinateAt(i);
            if (!activeAxes.HasFlag((MultipleAxes)(int)coord.Key)) continue;
            distSq += (long)coord.Value * coord.Value;
        }
        return distSq <= (long)radius * radius;
    }
    public static bool Contains(this Square size, Square position) =>
        position.X >= 0 && position.X < size.X &&
        position.Y >= 0 && position.Y < size.Y &&
        position.Z >= 0 && position.Z < size.Z &&
        position.W >= 0 && position.W < size.W;
    public static bool Contains(this Square size, int x, int y, int z, int w) =>
        x >= 0 && x < size.X &&
        y >= 0 && y < size.Y &&
        z >= 0 && z < size.Z &&
        w >= 0 && w < size.W;
}