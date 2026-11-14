#nullable enable

using System;
using System.Collections.Generic;

public sealed class SelectionMarkerCoordinator : IMarkerCoordinator
{
    private readonly ViewState _viewState;
    private readonly LayerPresenter _layerPresenter;
    private readonly LoaderOfBoard _boardLoader;
    private readonly SelectionState _selectionState;
    private readonly IRegistry _registry;
    private readonly PendingModalManager _modalManager;
    private readonly MatchState _matchState;

    private readonly MoveTargetPresenter _movePresenter;
    private readonly AttackTargetPresenter _attackPresenter;
    private readonly MarkerOrchestrator _markers;

    public SelectionMarkerCoordinator(SelectionMarkerCoordinatorContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        _viewState = context.ViewState;
        _layerPresenter = context.LayerPresenter;
        _boardLoader = context.BoardLoader;
        _selectionState = context.SelectionState;
        _registry = context.Registry;
        _modalManager = context.PendingModalManager;
        _matchState = context.MatchState;

        _movePresenter = new MoveTargetPresenter(_layerPresenter, _boardLoader, context.MarkerMaterial, markerSizeFraction: 0.25f);
        _attackPresenter = new AttackTargetPresenter(_layerPresenter, _boardLoader, context.MarkerMaterial, context.CellActionProviders, markerSizeFraction: 0.75f);

        _markers = new MarkerOrchestrator(_movePresenter, _attackPresenter);
    }

    public void RefreshMarkers()
    {
        _markers.ClearAll();
        if (_matchState.IsOver) return;
        if (_modalManager.HasPending) return;

        var selected = _selectionState.SelectedInfo;
        if (selected == null || selected.IsDead) return;

        var piece = selected.Piece;
        if (!_registry.TryToGetRoom(piece.IndexInHouse, out var room) || room == null) return;
        if (!(room is ISquarePositionRoom sqRoom)) return;
        if (!sqRoom.TryToGetPosition<Square>(piece.IndexInHouse, out var fromPos)) return;

        var shape = new List<ShapeCell>();
        foreach (var m in piece.Movements)
        {
            if (!m.IsApplicableToRoom(room)) continue;
            var cells = m.GetShape<Square>(piece, fromPos, room, includeOccupied: false);
            for (int i = 0; i < cells.Count; i++) shape.Add(cells[i]);
        }

        var moveShape = FilterMoveCells(shape);
        _movePresenter.Show(moveShape);
        _selectionState.UpdateShape(moveShape);
        _attackPresenter.Show(piece);
    }

    public void ClearAll() => _markers.ClearAll();
    public void RepositionAll() => _markers.RepositionAll();

    private static IReadOnlyList<ShapeCell> FilterMoveCells(IReadOnlyList<ShapeCell> shape)
    {
        var result = new List<ShapeCell>(shape.Count);
        for (int i = 0; i < shape.Count; i++)
        {
            var sc = shape[i];
            if (sc.Stayable == Stayable.Stay) result.Add(sc);
            else if (sc.Attackable == Attackable.NotAttack) result.Add(sc);
        }
        result.Sort((a, b) =>
        {
            int c = a.Cell.X.CompareTo(b.Cell.X);
            if (c != 0) return c;
            c = a.Cell.Y.CompareTo(b.Cell.Y);
            if (c != 0) return c;
            c = a.Cell.Z.CompareTo(b.Cell.Z);
            if (c != 0) return c;
            return a.Cell.W.CompareTo(b.Cell.W);
        });
        return result;
    }
}
