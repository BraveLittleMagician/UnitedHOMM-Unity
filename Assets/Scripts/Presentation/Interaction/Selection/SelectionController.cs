#nullable enable

using System;

public sealed class SelectionController : EventSubscriberBehaviour, IRunnable
{
    private IFlow _flow = null!;
    private IRegistry _registry = null!;
    private LoaderOfBoard _boardLoader = null!;
    private ViewState _viewState = null!;
    private LayerPresenter _layerPresenter = null!;
    private SelectionState _selectionState = null!;
    private HoverState _hoverState = null!;
    private ISelectionHub _selectionHub = null!;
    private MatchState _matchState = null!;
    private TurnManager _turnManager = null!;
    private PendingModalManager _modalManager = null!;
    private IMarkerCoordinator _markerCoordinator = null!;
    private ILogger _logger = null!;
    private bool _initialized;

    public void Initialize(SelectionControllerContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        _flow = context.Flow;
        _registry = context.Registry;
        _boardLoader = context.BoardLoader;
        _viewState = context.ViewState;
        _layerPresenter = context.LayerPresenter;
        _selectionState = context.SelectionState;
        _hoverState = context.HoverState;
        _selectionHub = context.SelectionHub;
        _matchState = context.MatchState;
        _turnManager = context.TurnManager;
        _modalManager = context.PendingModalManager;
        _markerCoordinator = context.MarkerCoordinator;
        _logger = context.Logger;
    }

    public void Run()
    {
        if (_initialized) return;
        if (_markerCoordinator == null) throw new InvalidOperationException("SelectionController не инициализирован.");

        _layerPresenter.Rebuilt += OnLayersRebuilt;
        AddSubscription(() => _layerPresenter.Rebuilt -= OnLayersRebuilt);
        _layerPresenter.PiecesRepositioned += OnPiecesRepositioned;
        AddSubscription(() => _layerPresenter.PiecesRepositioned -= OnPiecesRepositioned);

        _turnManager.TurnStarted += OnTurnStarted;
        AddSubscription(() => _turnManager.TurnStarted -= OnTurnStarted);

        _matchState.Changed += OnMatchStateChanged;
        AddSubscription(() => _matchState.Changed -= OnMatchStateChanged);

        _initialized = true;
    }

    protected override void OnCleanup() { _markerCoordinator?.ClearAll(); }

    public void RefreshMarkers() => _markerCoordinator?.RefreshMarkers();

    public void HandlePieceClick(InfoOfPiece info)
    {
        if (_matchState.IsOver) return;
        if (_modalManager.HasPending) return;
        PieceClicked?.Invoke(info);

        if (ReferenceEquals(_selectionState.SelectedInfo, info)) return;

        int layerIndex = GridAxes.GetGridCoordinate(_viewState.AxisIndex, info.PositionInGrid);
        _viewState.SetActiveLayer(layerIndex);

        _hoverState.Clear();
        _selectionHub.Select(info);
        _selectionState.Enter(info);
        RefreshMarkers();
    }

    public void HandleMoveTargetClick(MoveTargetMarker marker)
    {
        if (_matchState.IsOver) return;
        if (_modalManager.HasPending) return;
        MoveTargetClicked?.Invoke(marker);

        var selected = _selectionState.SelectedInfo;
        if (selected == null || selected.IsDead) return;
        if (!_selectionState.HasMove(marker.Cell)) return;

        var piece = selected.Piece;
        if (!_registry.TryToGetRoom(piece.IndexInHouse, out var room) || room == null) return;
        if (!(room is ISquarePositionRoom sqRoom)) return;
        if (!sqRoom.TryToGetPosition<Square>(piece.IndexInHouse, out var fromPos)) return;

        IPath<Square>? path = null;
        foreach (var m in piece.Movements)
        {
            if (!m.IsApplicableToRoom(room)) continue;
            if (m.TryFindPath(piece, fromPos, marker.Cell, room, out path) && path != null) break;
        }
        if (path == null)
        {
            MoveFailed?.Invoke(selected, marker.Cell);
            return;
        }

        MoveStarted?.Invoke(selected, path);

        _hoverState.Clear();
        _markerCoordinator.ClearAll();
        _selectionState.Exit();
        _selectionHub.DropSelection();

        var result = _flow.MovePiece(piece, marker.Cell);
        if (!result.IsSuccess)
        {
            _logger.LogWarning($"[MOVE] Failed: {result.Error}");
            MoveFailed?.Invoke(selected, marker.Cell);
            return;
        }

        MoveCompleted?.Invoke(selected, path);
    }

    public void HandleEmptyClick()
    {
        if (_matchState.IsOver) return;
        if (_modalManager.HasPending) return;
        EmptyClicked?.Invoke();
        Deselect();
    }

    private void Deselect()
    {
        _hoverState.Clear();
        _markerCoordinator.ClearAll();
        _selectionHub.DropSelection();
        _selectionState.Exit();
    }

    private void ClearSelectionAndMarkers()
    {
        _hoverState.Clear();
        _markerCoordinator.ClearAll();
        _selectionHub.DropSelection();
        _selectionState.Exit();
        _modalManager.Clear();
    }

    private void OnLayersRebuilt() => RefreshMarkers();
    private void OnPiecesRepositioned() => _markerCoordinator.RepositionAll();
    private void OnTurnStarted(IdOfPlayer _) => ClearSelectionAndMarkers();
    private void OnMatchStateChanged() { if (_matchState.IsOver) ClearSelectionAndMarkers(); }

    public event Action? EmptyClicked;
    public event Action<InfoOfPiece>? PieceClicked;
    public event Action<MoveTargetMarker>? MoveTargetClicked;
    public event Action<InfoOfPiece, IPath<Square>>? MoveStarted;
    public event Action<InfoOfPiece, IPath<Square>>? MoveCompleted;
    public event Action<InfoOfPiece, Square>? MoveFailed;
}