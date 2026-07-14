#nullable enable

using System;
using UnityEngine;

public class GameView : MonoBehaviour, IGameView
{
    [SerializeField] private GridRenderer _gridRenderer = null!;
    [SerializeField] private PieceRenderer _pieceRenderer = null!;
    [SerializeField] private BoardSizeInput _boardSizeInput = null!;

    private void Awake()
    {
        if (_gridRenderer == null) throw new NullReferenceException(nameof(_gridRenderer));
        if (_pieceRenderer == null) throw new NullReferenceException(nameof(_pieceRenderer));
        _boardSizeInput.OnValueChanged += OnBoardSizeInputChanged;
    }

    private void OnBoardSizeInputChanged(int newSize)
    {
        BoardSizeInputChanged?.Invoke(newSize);
    }

    public void BuildGrid(AxisAlignedBox box, MultipleAxes axes) => _gridRenderer.BuildGrid(box, axes);
    public void ShowPiece(IPiece piece, Square position)
    {
        var worldPos = _gridRenderer.GridToWorld(position);
        _pieceRenderer.ShowPiece(piece, worldPos);
    }
    public void UpdatePiecePosition(IPiece piece, Square newPosition)
    {
        var worldPos = _gridRenderer.GridToWorld(newPosition);
        _pieceRenderer.UpdatePiecePosition(piece, worldPos);
    }
    public void ShowPieceInDeck(IPiece piece, int position) { }
    public void UpdatePieceInDeckPosition(IPiece piece, int toPosition) { }
    public void HidePiece(IPiece piece) => _pieceRenderer.HidePiece(piece);
    public void SetBoardSize(int size) => _boardSizeInput.SetValue(size);

    public event Action<int>? BoardSizeInputChanged;
}