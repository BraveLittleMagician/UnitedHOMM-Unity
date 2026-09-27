#nullable enable

using System;
using System.Collections.Generic;

public static class FieldExtensions
{
    private static List<TSquare> GetStayables1D<TSquare>(List<Stayable> stayables, TSquare startSquare) where TSquare : struct, ISquare<TSquare>
    {
        List<TSquare> result = new ();
        int maxRadius = stayables.Count;
        for (int x = -maxRadius; x <= maxRadius; x++)
        {
            if (x == 0) continue;
            int radius = Math.Abs(x);
            if (radius <= maxRadius && stayables[radius - 1] == Stayable.Stay)
                result.Add(startSquare.WithOffset(x, 0, 0, 0));
        }
        return result;
    }
    private static List<TSquare> GetStayables2D<TSquare>(List<Stayable> stayables, TSquare startSquare) where TSquare : struct, ISquare<TSquare>
    {
        List<TSquare> result = new();
        int maxRadius = stayables.Count;
        for (int x = -maxRadius; x <= maxRadius; x++)
            for (int y = -maxRadius; y <= maxRadius; y++)
            {
                if (x == 0 && y == 0) continue;
                int radius = Math.Max(Math.Abs(x), Math.Abs(y));
                if (radius <= maxRadius && stayables[radius - 1] == Stayable.Stay)
                    result.Add(startSquare.WithOffset(x, y, 0, 0));
            }
        return result;
    }
    private static List<TSquare> GetStayables3D<TSquare>(List<Stayable> stayables, TSquare startSquare) where TSquare : struct, ISquare<TSquare>
    {
        List<TSquare> result = new();
        int maxRadius = stayables.Count;
        for (int x = -maxRadius; x <= maxRadius; x++)
            for (int y = -maxRadius; y <= maxRadius; y++)
                for (int z = -maxRadius; z <= maxRadius; z++)
                {
                    if (x == 0 && y == 0 && z == 0) continue;
                    int radius = Math.Max(Math.Max(Math.Abs(x), Math.Abs(y)), Math.Abs(z));
                    if (radius <= maxRadius && stayables[radius - 1] == Stayable.Stay)
                        result.Add(startSquare.WithOffset(x, y, z, 0));
                }
        return result;
    }
    private static List<TSquare> GetStayables4D<TSquare>(List<Stayable> stayables, TSquare startSquare) where TSquare : struct, ISquare<TSquare>
    {
        List<TSquare> result = new();
        int maxRadius = stayables.Count;
        for (int x = -maxRadius; x <= maxRadius; x++)
            for (int y = -maxRadius; y <= maxRadius; y++)
                for (int z = -maxRadius; z <= maxRadius; z++)
                    for (int w = -maxRadius; w <= maxRadius; w++)
                    {
                        if (x == 0 && y == 0 && z == 0 && w == 0) continue;
                        int radius = Math.Max(Math.Max(Math.Max(Math.Abs(x), Math.Abs(y)), Math.Abs(z)), Math.Abs(w));
                        if (radius <= maxRadius && stayables[radius - 1] == Stayable.Stay)
                            result.Add(startSquare.WithOffset(x, y, z, w));
                    }
        return result;
    }

    private static List<TSquare> GetStayablesCircle1D<TSquare>(List<Stayable> stayables, TSquare startSquare) where TSquare : struct, ISquare<TSquare>
    {
        List<TSquare> result = new();
        int maxRadius = stayables.Count;
        for (int x = -maxRadius; x <= maxRadius; x++)
        {
            if (x == 0) continue;
            int radius = Math.Abs(x);
            if (radius <= maxRadius && stayables[radius - 1] == Stayable.Stay)
                result.Add(startSquare.WithOffset(x, 0, 0, 0));
        }
        return result;
    }
    private static List<TSquare> GetStayablesCircle2D<TSquare>(List<Stayable> stayables, TSquare startSquare) where TSquare : struct, ISquare<TSquare>
    {
        List<TSquare> result = new();
        int maxRadius = stayables.Count;
        for (int x = -maxRadius; x <= maxRadius; x++)
        {
            for (int y = -maxRadius; y <= maxRadius; y++)
            {
                if (x == 0 && y == 0) continue;
                double distance = Math.Sqrt(x * x + y * y);
                int radius = (int)Math.Ceiling(distance);
                if (radius <= maxRadius && stayables[radius - 1] == Stayable.Stay)
                    result.Add(startSquare.WithOffset(x, y, 0, 0));
            }
        }
        return result;
    }
    private static List<TSquare> GetStayablesCircle3D<TSquare>(List<Stayable> stayables, TSquare startSquare) where TSquare : struct, ISquare<TSquare>
    {
        List<TSquare> result = new();
        int maxRadius = stayables.Count;
        for (int x = -maxRadius; x <= maxRadius; x++)
            for (int y = -maxRadius; y <= maxRadius; y++)
                for (int z = -maxRadius; z <= maxRadius; z++)
                {
                    if (x == 0 && y == 0 && z == 0) continue;
                    double distance = Math.Sqrt(x * x + y * y + z * z);
                    int radius = (int)Math.Ceiling(distance);
                    if (radius <= maxRadius && stayables[radius - 1] == Stayable.Stay)
                        result.Add(startSquare.WithOffset(x, y, z, 0));
                }
        return result;
    }
    private static List<TSquare> GetStayablesCircle4D<TSquare>(List<Stayable> stayables, TSquare startSquare) where TSquare : struct, ISquare<TSquare>
    {
        List<TSquare> result = new();
        int maxRadius = stayables.Count;
        for (int x = -maxRadius; x <= maxRadius; x++)
            for (int y = -maxRadius; y <= maxRadius; y++)
                for (int z = -maxRadius; z <= maxRadius; z++)
                    for (int w = -maxRadius; w <= maxRadius; w++)
                    {
                        if (x == 0 && y == 0 && z == 0 && w == 0) continue;
                        double distance = Math.Sqrt(x * x + y * y + z * z + w * w);
                        int radius = (int)Math.Ceiling(distance);
                        if (radius <= maxRadius && stayables[radius - 1] == Stayable.Stay)
                            result.Add(startSquare.WithOffset(x, y, z, w));
                    }
        return result;
    }

    private static Dictionary<TSquare, Stayable> GetPositions1D<TSquare>(TSquare startSquare, int maxRadius, List<Stayable> stayables) where TSquare : struct, ISquare<TSquare>
    {
        var result = new Dictionary<TSquare, Stayable>() { { startSquare, Stayable.NotStay } };
        if (maxRadius == 0) return result;
        for (int x = -maxRadius; x <= maxRadius; x++)
        {
            if (x == 0) continue;
            int radius = Math.Abs(x);
            var square = startSquare.WithOffset(x, 0, 0, 0);
            result[square] = stayables[radius - 1];
        }
        return result;
    }
    private static Dictionary<TSquare, Stayable> GetPositions2D<TSquare>(TSquare startSquare, int maxRadius, List<Stayable> stayables) where TSquare : struct, ISquare<TSquare>
    {
        var result = new Dictionary<TSquare, Stayable>() { { startSquare, Stayable.NotStay } }; 
        if (maxRadius == 0) return result;
        for (int x = -maxRadius; x <= maxRadius; x++)
            for (int y = -maxRadius; y <= maxRadius; y++)
            {
                if (x == 0 && y == 0) continue;
                int radius = Math.Max(Math.Abs(x), Math.Abs(y));
                if (radius > maxRadius) continue;
                var square = startSquare.WithOffset(x, y, 0, 0);
                result[square] = stayables[radius - 1];
            }
        return result;
    }
    private static Dictionary<TSquare, Stayable> GetPositions3D<TSquare>(TSquare startSquare, int maxRadius, List<Stayable> stayables) where TSquare : struct, ISquare<TSquare>
    {
        var result = new Dictionary<TSquare, Stayable>() { { startSquare, Stayable.NotStay } }; 
        if (maxRadius == 0) return result;
        for (int x = -maxRadius; x <= maxRadius; x++)
            for (int y = -maxRadius; y <= maxRadius; y++)
                for (int z = -maxRadius; z <= maxRadius; z++)
                {
                    if (x == 0 && y == 0 && z == 0) continue;
                    int radius = Math.Max(Math.Max(Math.Abs(x), Math.Abs(y)), Math.Abs(z));
                    if (radius > maxRadius) continue;
                    var square = startSquare.WithOffset(x, y, z, 0);
                    result[square] = stayables[radius - 1];
                }
        return result;
    }
    private static Dictionary<TSquare, Stayable> GetPositions4D<TSquare>(TSquare startSquare, int maxRadius, List<Stayable> stayables) where TSquare : struct, ISquare<TSquare>
    {
        var result = new Dictionary<TSquare, Stayable>() { { startSquare, Stayable.NotStay } }; 
        if (maxRadius == 0) return result;
        for (int x = -maxRadius; x <= maxRadius; x++)
            for (int y = -maxRadius; y <= maxRadius; y++)
                for (int z = -maxRadius; z <= maxRadius; z++)
                    for (int w = -maxRadius; w <= maxRadius; w++)
                    {
                        if (x == 0 && y == 0 && z == 0 && w == 0) continue;
                        int radius = Math.Max(Math.Max(Math.Max(Math.Abs(x), Math.Abs(y)), Math.Abs(z)), Math.Abs(w));
                        if (radius > maxRadius) continue;
                        var square = startSquare.WithOffset(x, y, z, w);
                        result[square] = (radius - 1) < 0 ? Stayable.NotStay : stayables[radius - 1];
                    }
        return result;
    }

    private static Dictionary<TSquare, Stayable> GetPositionsCircle1D<TSquare>(TSquare startSquare, int maxRadius, IReadOnlyList<Stayable> stayables) where TSquare : struct, ISquare<TSquare>
    {
        var result = new Dictionary<TSquare, Stayable>() { { startSquare, Stayable.NotStay } }; 
        if (maxRadius == 0) return result;
        for (int x = -maxRadius; x <= maxRadius; x++)
        {
            if (x == 0) continue;
            int radius = Math.Abs(x);
            var square = startSquare.WithOffset(x, 0, 0, 0);
            result[square] = stayables[radius - 1];
        }
        return result;
    }
    private static Dictionary<TSquare, Stayable> GetPositionsCircle2D<TSquare>(TSquare startSquare, int maxRadius, IReadOnlyList<Stayable> stayables) where TSquare : struct, ISquare<TSquare>
    {
        var result = new Dictionary<TSquare, Stayable>() { { startSquare, Stayable.NotStay } };
        if (maxRadius == 0) return result;
        for (int x = -maxRadius; x <= maxRadius; x++)
            for (int y = -maxRadius; y <= maxRadius; y++)
            {
                if (x == 0 && y == 0) continue;
                double distance = Math.Sqrt(x * x + y * y);
                int radius = (int)Math.Ceiling(distance);
                if (radius > maxRadius) continue;
                var square = startSquare.WithOffset(x, y, 0, 0);
                result[square] = stayables[radius - 1];
            }
        return result;
    }
    private static Dictionary<TSquare, Stayable> GetPositionsCircle3D<TSquare>(TSquare startSquare, int maxRadius, IReadOnlyList<Stayable> stayables) where TSquare : struct, ISquare<TSquare>
    {
        var result = new Dictionary<TSquare, Stayable>() { { startSquare, Stayable.NotStay } };
        if (maxRadius == 0) return result;
        for (int x = -maxRadius; x <= maxRadius; x++)
            for (int y = -maxRadius; y <= maxRadius; y++)
                for (int z = -maxRadius; z <= maxRadius; z++)
                {
                    if (x == 0 && y == 0 && z == 0) continue;
                    double distance = Math.Sqrt(x * x + y * y + z * z);
                    int radius = (int)Math.Ceiling(distance);
                    if (radius > maxRadius) continue;
                    var square = startSquare.WithOffset(x, y, z, 0);
                    result[square] = stayables[radius - 1];
                }
        return result;
    }
    private static Dictionary<TSquare, Stayable> GetPositionsCircle4D<TSquare>(TSquare startSquare, int maxRadius, IReadOnlyList<Stayable> stayables) where TSquare : struct, ISquare<TSquare>
    {
        var result = new Dictionary<TSquare, Stayable>() { { startSquare, Stayable.NotStay } };
        if (maxRadius == 0) return result;
        for (int x = -maxRadius; x <= maxRadius; x++)
            for (int y = -maxRadius; y <= maxRadius; y++)
                for (int z = -maxRadius; z <= maxRadius; z++)
                    for (int w = -maxRadius; w <= maxRadius; w++)
                    {
                        if (x == 0 && y == 0 && z == 0 && w == 0) continue;
                        double distance = Math.Sqrt(x * x + y * y + z * z + w * w);
                        int radius = (int)Math.Ceiling(distance);
                        if (radius > maxRadius) continue;
                        var square = startSquare.WithOffset(x, y, z, w);
                        result[square] = stayables[radius - 1];
                    }
        return result;
    }


    public static List<TSquare> AdjacentStayables<TSquare>(CountOfDimensions count, TSquare startSquare, List<Stayable> stayables) where TSquare : struct, ISquare<TSquare> => count switch
    {
        CountOfDimensions.Zero => new List<TSquare>(),
        CountOfDimensions.One => GetStayables1D(stayables, startSquare),
        CountOfDimensions.Two => GetStayables2D(stayables, startSquare),
        CountOfDimensions.Three => GetStayables3D(stayables, startSquare),
        CountOfDimensions.Four => GetStayables4D(stayables, startSquare),
        _ => new List<TSquare>(),
    };
    public static List<TSquare> AdjacentStayablesCircle<TSquare>(CountOfDimensions count, TSquare startSquare, List<Stayable> stayables) where TSquare : struct, ISquare<TSquare> => count switch
    {
        CountOfDimensions.Zero => new List<TSquare>(),
        CountOfDimensions.One => GetStayablesCircle1D(stayables, startSquare),
        CountOfDimensions.Two => GetStayablesCircle2D(stayables, startSquare),
        CountOfDimensions.Three => GetStayablesCircle3D(stayables, startSquare),
        CountOfDimensions.Four => GetStayablesCircle4D(stayables, startSquare),
        _ => new List<TSquare>(),
    };
    public static Dictionary<TSquare, Stayable> AdjacentPositions<TSquare>(CountOfDimensions count, TSquare startSquare, int maxRadius, List<Stayable> stayables) where TSquare : struct, ISquare<TSquare> => count switch
    {
        CountOfDimensions.Zero =>  new Dictionary<TSquare, Stayable>(),
        CountOfDimensions.One =>   GetPositions1D(startSquare, maxRadius, stayables),
        CountOfDimensions.Two =>   GetPositions2D(startSquare, maxRadius, stayables),
        CountOfDimensions.Three => GetPositions3D(startSquare, maxRadius, stayables),
        CountOfDimensions.Four =>  GetPositions4D(startSquare, maxRadius, stayables),
        _ => new Dictionary<TSquare, Stayable>(),
    };
    public static Dictionary<TSquare, Stayable> AdjacentPositionsCircle<TSquare>(CountOfDimensions count, TSquare startSquare, int maxRadius, List<Stayable> stayables) where TSquare : struct, ISquare<TSquare> => count switch
    {
        CountOfDimensions.Zero => new Dictionary<TSquare, Stayable>(),
        CountOfDimensions.One =>   GetPositionsCircle1D(startSquare, maxRadius, stayables),
        CountOfDimensions.Two =>   GetPositionsCircle2D(startSquare, maxRadius, stayables),
        CountOfDimensions.Three => GetPositionsCircle3D(startSquare, maxRadius, stayables),
        CountOfDimensions.Four =>  GetPositionsCircle4D(startSquare, maxRadius, stayables),
        _ => new Dictionary<TSquare, Stayable>(),
    };
}