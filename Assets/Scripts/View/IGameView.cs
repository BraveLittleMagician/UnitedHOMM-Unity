#nullable enable

using System;

public interface IGameView
{
    void ShowPiece(IPiece piece, Square position);
    void HidePiece(IPiece piece);
    void UpdatePiecePosition(IPiece piece, Square newPosition);
}