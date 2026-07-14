#nullable enable

public sealed class PiecesSpawner : IPiecesSpawner
{
    private readonly IFlow _flow;

    public PiecesSpawner(IFlow flow)
    {
        _flow = flow;
    }

    public void SpawnPieces()
    {
        var definition = new PieceDefinition("Pawn", new IndexOfPlayer(0, 0), 10);
        _flow.AddPiece<Board, Square>(definition, Square.Zero);
    }
}