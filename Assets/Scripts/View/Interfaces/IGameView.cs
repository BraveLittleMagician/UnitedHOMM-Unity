#nullable enable

public interface IGameView
{
    void ShowPiece(IPiece piece, Square position);
    void HidePiece(IPiece piece);
    void UpdatePiecePosition(IPiece piece, Square newPosition);
    void ShowPieceInDeck(IPiece piece, int position);
    void UpdatePieceInDeckPosition(IPiece piece, int toPosition);
}