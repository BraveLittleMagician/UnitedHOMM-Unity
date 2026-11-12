#nullable enable
using System;

public sealed class TargetSelectionController : EventSubscriberBehaviour, IRunnable
{
    private const float _markerSizeFraction = 0.75f;

    private IFlow _flow = null!;
    private SelectionState _selectionState = null!;
    private ISelectionHub _selectionHub = null!;
    private MatchState _matchState = null!;
    private LoaderOfBoard _boardLoader = null!;
    private ViewState _viewState = null!;
    private PendingModalManager _modalManager = null!;
    private ILogger _logger = null!;
    private TargetTargetPresenter _presenter = null!;

    private InfoOfPiece? _activePiece;
    private ITargetedModifier? _activeModifier;
    private bool _initialized;

    public bool IsActive => _activeModifier != null;
    public ITargetedModifier? ActiveModifier => _activeModifier;
    public InfoOfPiece? ActivePiece => _activePiece;

    public event Action? StateChanged;
    public event Action<InfoOfPiece, ITargetedModifier, Square>? Completed;

    public void Initialize(TargetSelectionContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        _flow = context.Flow;
        _selectionState = context.SelectionState;
        _selectionHub = context.SelectionHub;
        _matchState = context.MatchState;
        _boardLoader = context.BoardLoader;
        _viewState = context.ViewState;
        _modalManager = context.PendingModalManager;
        _logger = context.Logger;

        _presenter = new TargetTargetPresenter(
            context.ViewState, context.LayerPresenter, context.BoardLoader,
            context.MarkerMaterial, _markerSizeFraction);
    }

    public void Run()
    {
        if (_initialized) return;
        if (_matchState == null) throw new InvalidOperationException("TargetSelectionController не инициализирован.");

        _matchState.Changed += OnMatchStateChanged;
        AddSubscription(() => _matchState.Changed -= OnMatchStateChanged);

        _selectionHub.OnSelectionChanged += OnSelectionChanged;
        AddSubscription(() => _selectionHub.OnSelectionChanged -= OnSelectionChanged);

        _viewState.AxisChanged += OnViewChanged;
        AddSubscription(() => _viewState.AxisChanged -= OnViewChanged);
        _viewState.ActiveLayerChanged += OnViewChanged;
        AddSubscription(() => _viewState.ActiveLayerChanged -= OnViewChanged);

        _initialized = true;
    }

    protected override void OnCleanup() { _presenter?.Clear(); }

    public void Begin(InfoOfPiece piece, ITargetedModifier modifier)
    {
        if (piece == null || modifier == null) return;
        if (_matchState.IsOver) return;
        if (_modalManager.HasPending) return;
        if (piece.IsDead) return;
        if (modifier.CurrentCharges == 0) return;

        if (!_boardLoaderHasGridSize()) return;

        Cancel();
        _activePiece = piece;
        _activeModifier = modifier;

        if (!_boardLoaderGridSize(out var gridSize)) return;
        var cells = modifier.GetTargetCells(piece.Piece, piece.PositionInGrid, gridSize);
        _presenter.Show(cells);
        StateChanged?.Invoke();
    }

    public void Cancel()
    {
        if (_activeModifier == null && _activePiece == null) return;
        _presenter.Clear();
        _activeModifier = null;
        _activePiece = null;
        StateChanged?.Invoke();
    }

    public void HandleTargetClick(Square cell)
    {
        if (_activeModifier == null || _activePiece == null) return;
        if (_matchState.IsOver) { Cancel(); return; }
        if (_activePiece.IsDead) { Cancel(); return; }

        var piece = _activePiece;
        var modifier = _activeModifier;

        var result = _flow.ActivateTargetedModifier(piece.Piece, modifier, cell);
        if (!result.IsSuccess)
        {
            _logger.LogWarning($"[TARGET] {modifier.ModifierName} failed: {result.Error}");
            return;
        }

        Cancel();
        Completed?.Invoke(piece, modifier, cell);
    }

    private void OnMatchStateChanged() { if (_matchState.IsOver) Cancel(); }

    private void OnSelectionChanged(ISelectable? oldSel, ISelectable? newSel)
    {
        if (_activeModifier == null) return;
        if (!ReferenceEquals(newSel, _activePiece)) Cancel();
    }

    private void OnViewChanged()
    {
        if (_activeModifier == null) return;
        _presenter.Reposition();
    }

    private bool _boardLoaderHasGridSize() => _boardLoader != null;

    private bool _boardLoaderGridSize(out Square gridSize)
    {
        gridSize = _boardLoader.GridSize;
        return true;
    }
}
