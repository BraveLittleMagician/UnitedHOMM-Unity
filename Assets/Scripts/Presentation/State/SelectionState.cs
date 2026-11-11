#nullable enable

using System;
using System.Collections.Generic;

public sealed class SelectionState
{
    private readonly List<Square> _availableMoves = new();
    private readonly List<Square> _stayMoves = new();
    private readonly HashSet<Square> _stayMovesSet = new();
    private readonly List<ShapeCell> _availableShape = new();

    public InfoOfPiece? SelectedInfo { get; private set; }
    public IReadOnlyList<Square> AvailableMoves => _availableMoves;
    public IReadOnlyList<Square> StayMoves => _stayMoves;
    public IReadOnlyList<ShapeCell> AvailableShape => _availableShape;

    public event Action? Changed;

    public bool HasMove(Square cell) => _stayMovesSet.Contains(cell);

    public void Enter(InfoOfPiece info)
    {
        if (info == null) throw new ArgumentNullException(nameof(info));
        if (ReferenceEquals(SelectedInfo, info)) return;
        SelectedInfo = info;
        ClearAll();
        Changed?.Invoke();
    }

    public void Exit()
    {
        if (SelectedInfo == null) return;
        SelectedInfo = null;
        ClearAll();
        Changed?.Invoke();
    }

    public void UpdateShape(IReadOnlyList<ShapeCell> shape)
    {
        if (SelectedInfo == null) return;
        FillShape(shape);
        Changed?.Invoke();
    }

    public void ClearSelectionSilent()
    {
        SelectedInfo = null;
        ClearAll();
    }

    private void ClearAll()
    {
        _availableMoves.Clear();
        _stayMoves.Clear();
        _stayMovesSet.Clear();
        _availableShape.Clear();
    }

    private void FillShape(IReadOnlyList<ShapeCell>? shape)
    {
        ClearAll();
        if (shape == null) return;
        for (int i = 0; i < shape.Count; i++)
        {
            var s = shape[i];
            _availableMoves.Add(s.Cell);
            _availableShape.Add(s);
            if (s.Stayable == Stayable.Stay)
            {
                _stayMoves.Add(s.Cell);
                _stayMovesSet.Add(s.Cell);
            }
        }
    }
}