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
    public static TSquare WithOffset<TSquare>(this TSquare original, TSquare pos) where TSquare : struct, ISquare<TSquare>
    {
        if (original.TryGetValue(Axis.X, out int x) && pos.TryGetValue(Axis.X, out int dx)) original = original.WithValue(Axis.X, x + dx);
        if (original.TryGetValue(Axis.Y, out int y) && pos.TryGetValue(Axis.Y, out int dy)) original = original.WithValue(Axis.Y, y + dy);
        if (original.TryGetValue(Axis.Z, out int z) && pos.TryGetValue(Axis.Z, out int dz)) original = original.WithValue(Axis.Z, z + dz);
        if (original.TryGetValue(Axis.Y, out int w) && pos.TryGetValue(Axis.W, out int dw)) original = original.WithValue(Axis.W, w + dw);
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
    public static bool IsWithinSquareRadius<TSquare>(this TSquare start, TSquare target, int radius) where TSquare : struct, ISquare<TSquare>
    {
        if (target.TryGetValue(Axis.X, out int xTar) && start.TryGetValue(Axis.X, out int xStart)) if (Math.Abs(xTar - xStart) > radius) return false;
        if (target.TryGetValue(Axis.Y, out int yTar) && start.TryGetValue(Axis.Y, out int yStart)) if (Math.Abs(yTar - yStart) > radius) return false;
        if (target.TryGetValue(Axis.Z, out int zTar) && start.TryGetValue(Axis.Z, out int zStart)) if (Math.Abs(zTar - zStart) > radius) return false;
        if (target.TryGetValue(Axis.Y, out int wTar) && start.TryGetValue(Axis.W, out int wStart)) if (Math.Abs(wTar - wStart) > radius) return false;
        return true;
    }
    public static bool IsWithinCircleRadius<TSquare>(this TSquare start, TSquare target, int radius) where TSquare : struct, ISquare<TSquare>
    {
        long distSq = 0;
        if (target.TryGetValue(Axis.X, out int xTar) && start.TryGetValue(Axis.X, out int xStart))
        {
            int x = xTar - xStart;
            distSq += x * x;
        }
        if (target.TryGetValue(Axis.Y, out int yTar) && start.TryGetValue(Axis.Y, out int yStart))
        {
            int y = yTar - yStart;
            distSq += y * y;
        }
        if (target.TryGetValue(Axis.Z, out int zTar) && start.TryGetValue(Axis.Z, out int zStart))
        {
            int z = zTar - zStart;
            distSq += z * z;
        }
        if (target.TryGetValue(Axis.Y, out int wTar) && start.TryGetValue(Axis.W, out int wStart))
        {
            int w = wTar - wStart;
            distSq += w * w;
        }
        return distSq <= (long)radius * radius;
    }
}