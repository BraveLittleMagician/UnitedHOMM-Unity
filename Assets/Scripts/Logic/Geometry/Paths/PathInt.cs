#nullable enable

using System;

public record PathInt : Path<int>
{
    public PathInt(int[] path) : base(path, (a, b) => Math.Abs(a - b) == 1) { }
}