#nullable enable

using System;

public record PathInt : Path<int>
{
    public PathInt(int[] path) : base(path)
    {
        for (int i = 1; i < path.Length; i++)
        {
            if(Math.Abs(path[i] - path[i - 1]) != 1) 
                throw new Exception($"Позиция {path[i]} не является соседней для предыдущей {path[i - 1]}");
        }
    }
}