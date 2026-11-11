#nullable enable
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

public sealed class PieceViewRegistry
{
    private readonly Dictionary<Square, InfoOfPiece> _byCell = new();
    private readonly Dictionary<BigInteger, InfoOfPiece> _byIndex = new();

    public IReadOnlyCollection<InfoOfPiece> AllViews => _byIndex.Values;

    public IEnumerable<InfoOfPiece> AllLiveViews
    {
        get
        {
            foreach (var v in _byIndex.Values)
                if (v != null && !v.IsDead)
                    yield return v;
        }
    }

    public void Register(InfoOfPiece view)
    {
        _byIndex[view.IndexInHouse] = view;
        _byCell[view.PositionInGrid] = view;
    }

    public void Unregister(BigInteger index)
    {
        if (!_byIndex.TryGetValue(index, out var view)) return;
        if (_byCell.TryGetValue(view.PositionInGrid, out var atCell) && atCell == view)
            _byCell.Remove(view.PositionInGrid);
        _byIndex.Remove(index);
    }

    public void Move(BigInteger index, Square oldCell, Square newCell)
    {
        if (!_byIndex.TryGetValue(index, out var view)) return;
        if (_byCell.TryGetValue(oldCell, out var atOld) && atOld == view)
            _byCell.Remove(oldCell);
        view.PositionInGrid = newCell;
        _byCell[newCell] = view;
    }

    public bool TryGetAt(Square cell, [NotNullWhen(true)] out InfoOfPiece? view)
    {
        if (_byCell.TryGetValue(cell, out view) && view != null && !view.IsDead) return true;
        view = null;
        return false;
    }

    public bool TryGetByIndex(BigInteger index, [NotNullWhen(true)] out InfoOfPiece? view)
        => _byIndex.TryGetValue(index, out view);

    public void Clear()
    {
        _byCell.Clear();
        _byIndex.Clear();
    }
}