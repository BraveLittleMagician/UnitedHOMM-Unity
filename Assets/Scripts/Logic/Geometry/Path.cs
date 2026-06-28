#nullable enable


using System;
using System.Collections.Generic;

public abstract record Path<TPositions> : IPath<TPositions> where TPositions : struct
{
    public Path(TPositions[] path)
    {
        if (path.Length < 2) throw new Exception($"Путь не содержит начальную и конечные точки");
        if (path[0].Equals(path[^1])) throw new Exception($"Начальная и конечная точки одинаковый");
        if (path is int[] intPath)
        {
            for (int i = 1; i < intPath.Length; i++)
            {
                if (!(Math.Abs(intPath[i] - intPath[i - 1]) == 1))
                    throw new Exception($"Позиция {path[i]} не является соседней для предыдущей {path[i - 1]}");
            }
        }
        else if (path is Square[] squarePath)
        {
            for (int i = 1; i < squarePath.Length; i++)
            {
                if (!squarePath[i].IsAdjacent(squarePath[i - 1]))
                    throw new Exception($"Квадрат {path[i]} не является соседней предыдущему квадрату {path[i - 1]}");
            }
        }

        Positions = path;
    }
    public TPositions[] Positions { get; }
    IReadOnlyList<TPositions> IPath<TPositions>.Positions => Positions;
}