#nullable enable

using System;
using System.Collections.Generic;

public abstract class Path<TPositions> : IPath<TPositions> where TPositions : struct
{
    protected Path(TPositions[] path, Func<TPositions, TPositions, bool> areAdjacent)
    {
        if (path.Length < 2)
            throw new ArgumentException("Путь не содержит начальную и конечную точки");
        if (path[0].Equals(path[^1]))
            throw new ArgumentException("Начальная и конечная точки совпадают");

        for (int i = 1; i < path.Length; i++)
        {
            if (!areAdjacent(path[i - 1], path[i]))
                throw new ArgumentException($"Позиция {path[i]} не является соседней для {path[i - 1]}");
        }
        Positions = path;
    }

    public TPositions[] Positions { get; }
    IReadOnlyList<TPositions> IPath<TPositions>.Positions => Positions;
}