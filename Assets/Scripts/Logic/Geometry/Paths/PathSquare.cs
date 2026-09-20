#nullable enable

using System;
using System.Linq;

public record PathSquare : Path<Square>
{
    public PathSquare(Square[] path) : base(path)
    {
        for (int i = 1; i < path.Length; i++)
        {
            if (!path[i].IsAdjacent(path[i - 1]))
                throw new Exception($"Квадрат {path[i]} не является соседней предыдущему квадрату {path[i - 1]}");
        }
    }

    public override string ToString() => Positions.Aggregate("", (s, square) => $"{s} {square}");
}