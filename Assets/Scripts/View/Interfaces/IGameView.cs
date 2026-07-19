#nullable enable

using System;

public interface IGameView
{
    void CreatePiece(IPiece piece, Square position);
    void DestroyPiece(IPiece piece);
    void UpdatePiecePosition(IPiece piece, Square newPosition);
    void CreatePieceInDeck(IPiece piece, int position);
    void UpdatePieceInDeckPosition(IPiece piece, int toPosition);
    void SetBoardSize(int size);

    event Action<int>? BoardSizeInputChanged;
}