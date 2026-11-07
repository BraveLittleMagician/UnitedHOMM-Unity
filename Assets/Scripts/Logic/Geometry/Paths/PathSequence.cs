#nullable enable

public sealed class PathSequence<TSquare> : Path<TSquare> where TSquare : struct, ISquare<TSquare>
{
    public PathSequence(TSquare[] path) : base(path, (a, b) => a.IsAdjacent(b)) { }
}