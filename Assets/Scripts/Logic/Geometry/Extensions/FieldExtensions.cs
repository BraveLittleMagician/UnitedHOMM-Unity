#nullable enable

using System;
using System.Collections.Generic;

public static class FieldExtensions
{
    private static List<Square> GetStayables1D(List<Stayable> stayables, Square startSquare)
    {
        List<Square> result = new ();
        int maxRadius = stayables.Count;
        for (int x = -maxRadius; x <= maxRadius; x++)
        {
            if (x == 0) continue;
            int radius = Math.Abs(x);
            if (radius <= maxRadius && stayables[radius - 1] == Stayable.Stay)
                result.Add(new Square { X = x } + startSquare);
        }
        return result;
    }
    private static List<Square> GetStayables2D(List<Stayable> stayables, Square startSquare)
    {
        List<Square> result = new();
        int maxRadius = stayables.Count;
        for (int x = -maxRadius; x <= maxRadius; x++)
            for (int y = -maxRadius; y <= maxRadius; y++)
            {
                if (x == 0 && y == 0) continue;
                int radius = Math.Max(Math.Abs(x), Math.Abs(y));
                if (radius <= maxRadius && stayables[radius - 1] == Stayable.Stay)
                    result.Add(new Square { X = x, Y = y } + startSquare);
            }
        return result;
    }
    private static List<Square> GetStayables3D(List<Stayable> stayables, Square startSquare)
    {
        List<Square> result = new();
        int maxRadius = stayables.Count;
        for (int x = -maxRadius; x <= maxRadius; x++)
            for (int y = -maxRadius; y <= maxRadius; y++)
                for (int z = -maxRadius; z <= maxRadius; z++)
                {
                    if (x == 0 && y == 0 && z == 0) continue;
                    int radius = Math.Max(Math.Max(Math.Abs(x), Math.Abs(y)), Math.Abs(z));
                    if (radius <= maxRadius && stayables[radius - 1] == Stayable.Stay)
                        result.Add(new Square { X = x, Y = y, Z = z } + startSquare);
                }
        return result;
    }
    private static List<Square> GetStayables4D(List<Stayable> stayables, Square startSquare)
    {
        List<Square> result = new();
        int maxRadius = stayables.Count;
        for (int x = -maxRadius; x <= maxRadius; x++)
            for (int y = -maxRadius; y <= maxRadius; y++)
                for (int z = -maxRadius; z <= maxRadius; z++)
                    for (int w = -maxRadius; w <= maxRadius; w++)
                    {
                        if (x == 0 && y == 0 && z == 0 && w == 0) continue;
                        int radius = Math.Max(Math.Max(Math.Max(Math.Abs(x), Math.Abs(y)), Math.Abs(z)), Math.Abs(w));
                        if (radius <= maxRadius && stayables[radius - 1] == Stayable.Stay)
                            result.Add(new Square { X = x, Y = y, Z = z, W = w } + startSquare);
                    }
        return result;
    }

    private static List<Square> GetStayablesCircle1D(List<Stayable> stayables, Square startSquare)
    {
        List<Square> result = new();
        int maxRadius = stayables.Count;
        for (int x = -maxRadius; x <= maxRadius; x++)
        {
            if (x == 0) continue;
            int radius = Math.Abs(x);
            if (radius <= maxRadius && stayables[radius - 1] == Stayable.Stay)
                result.Add(new Square { X = x } + startSquare);
        }
        return result;
    }
    private static List<Square> GetStayablesCircle2D(List<Stayable> stayables, Square startSquare)
    {
        List<Square> result = new();
        int maxRadius = stayables.Count;
        for (int x = -maxRadius; x <= maxRadius; x++)
        {
            for (int y = -maxRadius; y <= maxRadius; y++)
            {
                if (x == 0 && y == 0) continue;
                double distance = Math.Sqrt(x * x + y * y);
                int radius = (int)Math.Ceiling(distance);
                if (radius <= maxRadius && stayables[radius - 1] == Stayable.Stay)
                    result.Add(new Square { X = x, Y = y } + startSquare);
            }
        }
        return result;
    }
    private static List<Square> GetStayablesCircle3D(List<Stayable> stayables, Square startSquare)
    {
        List<Square> result = new();
        int maxRadius = stayables.Count;
        for (int x = -maxRadius; x <= maxRadius; x++)
            for (int y = -maxRadius; y <= maxRadius; y++)
                for (int z = -maxRadius; z <= maxRadius; z++)
                {
                    if (x == 0 && y == 0 && z == 0) continue;
                    double distance = Math.Sqrt(x * x + y * y + z * z);
                    int radius = (int)Math.Ceiling(distance);
                    if (radius <= maxRadius && stayables[radius - 1] == Stayable.Stay)
                        result.Add(new Square { X = x, Y = y, Z = z } + startSquare);
                }
        return result;
    }
    private static List<Square> GetStayablesCircle4D(List<Stayable> stayables, Square startSquare)
    {
        List<Square> result = new();
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
                            result.Add(new Square { X = x, Y = y, Z = z, W = w } + startSquare);
                    }
        return result;
    }

    private static Dictionary<Square, Stayable> GetPositions1D(Square startSquare, int maxRadius, List<Stayable> stayables)
    {
        var result = new Dictionary<Square, Stayable>() { { startSquare, Stayable.NotStay } };
        if (maxRadius == 0) return result;
        for (int x = -maxRadius; x <= maxRadius; x++)
        {
            if (x == 0) continue;
            int radius = Math.Abs(x);
            var square = new Square { X = x } + startSquare;
            result[square] = stayables[radius - 1];
        }
        return result;
    }
    private static Dictionary<Square, Stayable> GetPositions2D(Square startSquare, int maxRadius, List<Stayable> stayables)
    {
        var result = new Dictionary<Square, Stayable>() { { startSquare, Stayable.NotStay } }; 
        if (maxRadius == 0) return result;
        for (int x = -maxRadius; x <= maxRadius; x++)
            for (int y = -maxRadius; y <= maxRadius; y++)
            {
                if (x == 0 && y == 0) continue;
                int radius = Math.Max(Math.Abs(x), Math.Abs(y));
                if (radius > maxRadius) continue;
                var square = new Square { X = x, Y = y } + startSquare;
                result[square] = stayables[radius - 1];
            }
        return result;
    }
    private static Dictionary<Square, Stayable> GetPositions3D(Square startSquare, int maxRadius, List<Stayable> stayables)
    {
        var result = new Dictionary<Square, Stayable>() { { startSquare, Stayable.NotStay } }; 
        if (maxRadius == 0) return result;
        for (int x = -maxRadius; x <= maxRadius; x++)
            for (int y = -maxRadius; y <= maxRadius; y++)
                for (int z = -maxRadius; z <= maxRadius; z++)
                {
                    if (x == 0 && y == 0 && z == 0) continue;
                    int radius = Math.Max(Math.Max(Math.Abs(x), Math.Abs(y)), Math.Abs(z));
                    if (radius > maxRadius) continue;
                    var square = new Square { X = x, Y = y, Z = z } + startSquare;
                    result[square] = stayables[radius - 1];
                }
        return result;
    }
    private static Dictionary<Square, Stayable> GetPositions4D(Square startSquare, int maxRadius, List<Stayable> stayables)
    {
        var result = new Dictionary<Square, Stayable>() { { startSquare, Stayable.NotStay } }; 
        if (maxRadius == 0) return result;
        for (int x = -maxRadius; x <= maxRadius; x++)
            for (int y = -maxRadius; y <= maxRadius; y++)
                for (int z = -maxRadius; z <= maxRadius; z++)
                    for (int w = -maxRadius; w <= maxRadius; w++)
                    {
                        if (x == 0 && y == 0 && z == 0 && w == 0) continue;
                        int radius = Math.Max(Math.Max(Math.Max(Math.Abs(x), Math.Abs(y)), Math.Abs(z)), Math.Abs(w));
                        if (radius > maxRadius) continue;
                        var square = new Square { X = x, Y = y, Z = z, W = w } + startSquare;
                        result[square] = (radius - 1) < 0 ? Stayable.NotStay : stayables[radius - 1];
                    }
        return result;
    }

    private static Dictionary<Square, Stayable> GetPositionsCircle1D(Square startSquare, int maxRadius, IReadOnlyList<Stayable> stayables)
    {
        var result = new Dictionary<Square, Stayable>() { { startSquare, Stayable.NotStay } }; 
        if (maxRadius == 0) return result;
        for (int x = -maxRadius; x <= maxRadius; x++)
        {
            if (x == 0) continue;
            int radius = Math.Abs(x);
            var square = new Square { X = x } + startSquare;
            result[square] = stayables[radius - 1];
        }
        return result;
    }
    private static Dictionary<Square, Stayable> GetPositionsCircle2D(Square startSquare, int maxRadius, IReadOnlyList<Stayable> stayables)
    {
        var result = new Dictionary<Square, Stayable>() { { startSquare, Stayable.NotStay } };
        if (maxRadius == 0) return result;
        for (int x = -maxRadius; x <= maxRadius; x++)
            for (int y = -maxRadius; y <= maxRadius; y++)
            {
                if (x == 0 && y == 0) continue;
                double distance = Math.Sqrt(x * x + y * y);
                int radius = (int)Math.Ceiling(distance);
                if (radius > maxRadius) continue;
                var square = new Square { X = x, Y = y } + startSquare;
                result[square] = stayables[radius - 1];
            }
        return result;
    }
    private static Dictionary<Square, Stayable> GetPositionsCircle3D(Square startSquare, int maxRadius, IReadOnlyList<Stayable> stayables)
    {
        var result = new Dictionary<Square, Stayable>() { { startSquare, Stayable.NotStay } };
        if (maxRadius == 0) return result;
        for (int x = -maxRadius; x <= maxRadius; x++)
            for (int y = -maxRadius; y <= maxRadius; y++)
                for (int z = -maxRadius; z <= maxRadius; z++)
                {
                    if (x == 0 && y == 0 && z == 0) continue;
                    double distance = Math.Sqrt(x * x + y * y + z * z);
                    int radius = (int)Math.Ceiling(distance);
                    if (radius > maxRadius) continue;
                    var square = new Square { X = x, Y = y, Z = z } + startSquare;
                    result[square] = stayables[radius - 1];
                }
        return result;
    }
    private static Dictionary<Square, Stayable> GetPositionsCircle4D(Square startSquare, int maxRadius, IReadOnlyList<Stayable> stayables)
    {
        var result = new Dictionary<Square, Stayable>() { { startSquare, Stayable.NotStay } };
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
                        var square = new Square { X = x, Y = y, Z = z, W = w } + startSquare;
                        result[square] = stayables[radius - 1];
                    }
        return result;
    }


    public static List<Square> AdjacentStayables(CountOfDimensions count, Square startSquare, List<Stayable> stayables) => count switch
    {
        CountOfDimensions.Zero => new List<Square>(),
        CountOfDimensions.One => GetStayables1D(stayables, startSquare),
        CountOfDimensions.Two => GetStayables2D(stayables, startSquare),
        CountOfDimensions.Three => GetStayables3D(stayables, startSquare),
        CountOfDimensions.Four => GetStayables4D(stayables, startSquare),
        _ => new List<Square>(),
    };
    public static List<Square> AdjacentStayablesCircle(CountOfDimensions count, Square startSquare, List<Stayable> stayables) => count switch
    {
        CountOfDimensions.Zero => new List<Square>(),
        CountOfDimensions.One => GetStayablesCircle1D(stayables, startSquare),
        CountOfDimensions.Two => GetStayablesCircle2D(stayables, startSquare),
        CountOfDimensions.Three => GetStayablesCircle3D(stayables, startSquare),
        CountOfDimensions.Four => GetStayablesCircle4D(stayables, startSquare),
        _ => new List<Square>(),
    };
    public static Dictionary<Square, Stayable> AdjacentPositions(CountOfDimensions count, Square startSquare, int maxRadius,
    List<Stayable> stayables) => count switch
    {
        CountOfDimensions.Zero =>  new Dictionary<Square, Stayable>(),
        CountOfDimensions.One =>   GetPositions1D(startSquare, maxRadius, stayables),
        CountOfDimensions.Two =>   GetPositions2D(startSquare, maxRadius, stayables),
        CountOfDimensions.Three => GetPositions3D(startSquare, maxRadius, stayables),
        CountOfDimensions.Four =>  GetPositions4D(startSquare, maxRadius, stayables),
        _ => new Dictionary<Square, Stayable>(),
    };
    public static Dictionary<Square, Stayable> AdjacentPositionsCircle(CountOfDimensions count, Square startSquare, int maxRadius,
    List<Stayable> stayables) => count switch
    {
        CountOfDimensions.Zero => new Dictionary<Square, Stayable>(),
        CountOfDimensions.One =>   GetPositionsCircle1D(startSquare, maxRadius, stayables),
        CountOfDimensions.Two =>   GetPositionsCircle2D(startSquare, maxRadius, stayables),
        CountOfDimensions.Three => GetPositionsCircle3D(startSquare, maxRadius, stayables),
        CountOfDimensions.Four =>  GetPositionsCircle4D(startSquare, maxRadius, stayables),
        _ => new Dictionary<Square, Stayable>(),
    };
}