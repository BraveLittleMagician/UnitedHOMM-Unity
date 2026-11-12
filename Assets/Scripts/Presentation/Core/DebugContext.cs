#nullable enable
public sealed record DebugContext(PieceDebugLogger? PieceDebugLogger)
{
    public void Run()
    {
        PieceDebugLogger?.Run();
    }
}