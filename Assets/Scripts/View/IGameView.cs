#nullable enable

using System;

public interface IGameView
{
    public void ShowPiece(IPiece piece);
    public void HidePiece(IPiece piece); 
    public event Action<IPiece>? PieceClicked;
    public event Action<Square>? CellClicked;
}