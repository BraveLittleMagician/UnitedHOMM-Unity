#nullable enable

public class PathSquare : Path<Square>
{
    public PathSquare(Square[] path) : base(path, (a, b) => a.IsAdjacent(b)) { }
}