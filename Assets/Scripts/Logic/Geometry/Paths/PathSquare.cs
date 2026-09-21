#nullable enable

public record PathSquare : Path<Square>
{
    public PathSquare(Square[] path) : base(path, (a, b) => a.IsAdjacent(b)) { }
}