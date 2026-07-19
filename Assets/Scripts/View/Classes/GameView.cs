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
        if (_gridRenderer == null) throw new ArgumentNullException(nameof(_gridRenderer));
        if (_pieceRenderer == null) throw new ArgumentNullException(nameof(_pieceRenderer));
        _boardSizeInput.OnValueChanged += OnBoardSizeInputChanged;
    }
    private void OnDestroy()
    {
        _boardSizeInput.OnValueChanged -= OnBoardSizeInputChanged;
    }

    private void OnBoardSizeInputChanged(int newSize)
    {
        BoardSizeInputChanged?.Invoke(newSize);
    }

    public void BuildGrid(AxisAlignedBox box) => _gridRenderer.BuildGrid(box);
    public void CreatePiece(IPiece piece, Square position)
    {
        var worldPos = _gridRenderer.GridToWorld(position);
        _pieceRenderer.CreatePiece(piece, worldPos);
    }
    public void UpdatePiecePosition(IPiece piece, Square newPosition)
    {
        var worldPos = _gridRenderer.GridToWorld(newPosition);
        _pieceRenderer.UpdatePiecePosition(piece, worldPos);
    }
    public void CreatePieceInDeck(IPiece piece, int position) { }
    public void UpdatePieceInDeckPosition(IPiece piece, int toPosition) { }
    public void DestroyPiece(IPiece piece) => _pieceRenderer.DestroyPiece(piece);
    public void SetBoardSize(int size)
    {
        _boardSizeInput.SetValue(size);
    }

    public event Action<int>? BoardSizeInputChanged;
}