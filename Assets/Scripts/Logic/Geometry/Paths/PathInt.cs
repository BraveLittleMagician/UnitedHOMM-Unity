#nullable enable

using System;

public class PathInt : Path<int>
{
    public PathInt(int[] path) : base(path, (a, b) => Math.Abs(a - b) == 1) { }
}