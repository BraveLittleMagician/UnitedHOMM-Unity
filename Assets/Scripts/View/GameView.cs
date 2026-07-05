#nullable enable

using System;
using UnityEngine;

public class GameView : MonoBehaviour, IGameView
{
    [SerializeField] private GridRenderer _gridRenderer = null!;
    [SerializeField] private PieceRenderer _pieceRenderer = null!;
    [SerializeField] private InputHandler _inputHandler = null!;

    private void Awake()
    {
        if (_gridRenderer == null) throw new NullReferenceException(nameof(_gridRenderer));
        if (_pieceRenderer == null) throw new NullReferenceException(nameof(_pieceRenderer));
        if (_inputHandler == null) throw new NullReferenceException(nameof(_inputHandler));
        _inputHandler.CellClicked += OnCellClicked;
        _inputHandler.PieceClicked += OnPieceClicked;
    }
    private void OnDestroy()
    {
        _inputHandler.CellClicked -= OnCellClicked;
        _inputHandler.PieceClicked -= OnPieceClicked;
    }

    private void OnCellClicked(Square square) => CellClicked?.Invoke(square);
    private void OnPieceClicked(IPiece piece) => PieceClicked?.Invoke(piece);

    public void ShowPiece(IPiece piece)
    {
        var pos = _gridRenderer.GridToWorld(new Square { X = 0, Y = 0 });
        _pieceRenderer.ShowPiece(piece, pos);
    }
    public void HidePiece(IPiece piece) => _pieceRenderer.HidePiece(piece);
    public void SelectPiece(IPiece piece) => _pieceRenderer.SelectPiece(piece);
    public void DeselectPiece() => _pieceRenderer.DeselectPiece();
    public void UpdatePiecePosition(IPiece piece, Square square)
    {
        var pos = _gridRenderer.GridToWorld(square);
        _pieceRenderer.UpdatePiecePosition(piece, pos);
    }

    public event Action<IPiece>? PieceClicked;
    public event Action<Square>? CellClicked;
}