#nullable enable
using System;
using System.Text;
using UnityEngine;

public sealed class PieceDebugLogger : EventSubscriberBehaviour, IRunnable
{
    [Header("Log Categories")]
    [SerializeField] private bool _logClicks = true;
    [SerializeField] private bool _logSelections = true;
    [SerializeField] private bool _logMoves = true;
    [SerializeField] private bool _logFails = true;
    [SerializeField] private bool _logModifiers = true;
    [SerializeField] private bool _logEvents = true;

    private SelectionController _selectionController = null!;
    private SelectionHub _selectionHub = null!;
    private ModifierActivationHandler _modifierActivationHandler = null!;
    private PromotionChoiceHandler _promotionChoiceHandler = null!;
    private IEventBus _eventBus = null!;
    private ILogger _logger = null!;
    private bool _initialized;

    public void Initialize(
        SelectionController selectionController,
        SelectionHub selectionHub,
        ModifierActivationHandler modifierActivationHandler,
        PromotionChoiceHandler promotionChoiceHandler,
        IEventBus eventBus,
        ILogger logger)
    {
        _selectionController = selectionController ?? throw new ArgumentNullException(nameof(selectionController));
        _selectionHub = selectionHub ?? throw new ArgumentNullException(nameof(selectionHub));
        _modifierActivationHandler = modifierActivationHandler ?? throw new ArgumentNullException(nameof(modifierActivationHandler));
        _promotionChoiceHandler = promotionChoiceHandler ?? throw new ArgumentNullException(nameof(promotionChoiceHandler));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void Run()
    {
        if (_initialized) return;
        if (_selectionController == null) throw new InvalidOperationException("PieceDebugLogger не инициализирован.");

        _selectionController.PieceClicked += OnPieceClicked;
        AddSubscription(() => _selectionController.PieceClicked -= OnPieceClicked);

        _selectionController.MoveTargetClicked += OnMoveTargetClicked;
        AddSubscription(() => _selectionController.MoveTargetClicked -= OnMoveTargetClicked);

        _selectionController.EmptyClicked += OnEmptyClicked;
        AddSubscription(() => _selectionController.EmptyClicked -= OnEmptyClicked);

        _selectionController.MoveStarted += OnMoveStarted;
        AddSubscription(() => _selectionController.MoveStarted -= OnMoveStarted);

        _selectionController.MoveCompleted += OnMoveCompleted;
        AddSubscription(() => _selectionController.MoveCompleted -= OnMoveCompleted);

        _selectionController.MoveFailed += OnMoveFailed;
        AddSubscription(() => _selectionController.MoveFailed -= OnMoveFailed);

        _modifierActivationHandler.ModifierActivated += OnModifierActivated;
        AddSubscription(() => _modifierActivationHandler.ModifierActivated -= OnModifierActivated);

        _promotionChoiceHandler.PromotionOccurred += OnPromotionOccurred;
        AddSubscription(() => _promotionChoiceHandler.PromotionOccurred -= OnPromotionOccurred);

        _selectionHub.OnSelectionChanged += OnSelectionChanged;
        AddSubscription(() => _selectionHub.OnSelectionChanged -= OnSelectionChanged);

        _eventBus.Subscribe<PieceDeployedEvent<Square>>(OnPiecePlaced);
        AddSubscription(() => _eventBus.Unsubscribe<PieceDeployedEvent<Square>>(OnPiecePlaced));

        _eventBus.Subscribe<PieceDiedEvent>(OnPieceDied);
        AddSubscription(() => _eventBus.Unsubscribe<PieceDiedEvent>(OnPieceDied));

        _eventBus.Subscribe<PieceRevivedEvent>(OnPieceRevived);
        AddSubscription(() => _eventBus.Unsubscribe<PieceRevivedEvent>(OnPieceRevived));

        _eventBus.Subscribe<PieceStartedActionEvent>(OnPieceStartedAction);
        AddSubscription(() => _eventBus.Unsubscribe<PieceStartedActionEvent>(OnPieceStartedAction));

        _eventBus.Subscribe<PieceMovedEvent<Square>>(OnPieceMoved);
        AddSubscription(() => _eventBus.Unsubscribe<PieceMovedEvent<Square>>(OnPieceMoved));

        _eventBus.Subscribe<PieceAttackedEvent>(OnPieceAttacked);
        AddSubscription(() => _eventBus.Unsubscribe<PieceAttackedEvent>(OnPieceAttacked));

        _eventBus.Subscribe<FacingChangedEvent>(OnFacingChanged);
        AddSubscription(() => _eventBus.Unsubscribe<FacingChangedEvent>(OnFacingChanged));

        _initialized = true;
    }

    private void OnPieceClicked(InfoOfPiece view)
    {
        if (!_logClicks) return;
        var p = view.Piece;
        Debug.Log(
            $"[CLICK] {p.Name}#{p.IndexInHouse} [{p.Owner}] at {Fmt(view.PositionInGrid)} " +
            $"HP={p.Health}/{p.HealthNormal} Charges={p.AttackCharges}/{p.MaxAttackCharges} " +
            $"Dmg={p.AttackDamage} Facing={p.Facing} HasActed={p.HasTakenFirstActionOfGame} " +
            $"MovedThisTurn={p.HasMovedThisTurn}", view);
    }

    private void OnMoveTargetClicked(MoveTargetMarker marker)
    {
        if (!_logClicks) return;
        Debug.Log($"[CLICK] MoveTarget at {Fmt(marker.Cell)}", marker);
    }

    private void OnEmptyClicked()
    {
        if (!_logClicks) return;
        Debug.Log("[CLICK] Empty");
    }

    private void OnSelectionChanged(ISelectable? oldSel, ISelectable? newSel)
    {
        if (!_logSelections) return;
        Debug.Log($"[SELECT] {DescribeSelectable(oldSel)} -> {DescribeSelectable(newSel)}");
    }

    private void OnMoveStarted(InfoOfPiece view, IPath<Square> path)
    {
        if (!_logMoves) return;
        Debug.Log($"[MOVE] Start: {view.Name} at {Fmt(view.PositionInGrid)} -> {Fmt(path.Positions[^1])} " +
                  $"({path.Positions.Count} cells)\n       Path: {FormatPath(path)}", view);
    }

    private void OnMoveCompleted(InfoOfPiece view, IPath<Square> path)
    {
        if (!_logMoves) return;
        var p = view.Piece;
        Debug.Log($"[MOVE] Complete: {p.Name} arrived at {Fmt(view.PositionInGrid)} " +
                  $"HasActed={p.HasTakenFirstActionOfGame} MovedThisTurn={p.HasMovedThisTurn}", view);
    }

    private void OnMoveFailed(InfoOfPiece view, Square target)
    {
        if (!_logFails) return;
        Debug.LogWarning($"[MOVE] Failed: {view.Name} cannot reach {Fmt(target)}", view);
    }

    private void OnModifierActivated(IPiece piece, IActiveModifier modifier)
    {
        if (!_logModifiers) return;
        Debug.Log($"[MODIFIER-USE] {piece.Name}#{piece.IndexInHouse} used {modifier.ModifierName} " +
                  $"(charges: {modifier.CurrentCharges}/{modifier.MaxCharges}, HP={piece.Health}/{piece.HealthNormal})");
    }

    private void OnPromotionOccurred(IPiece piece, string fromName, string toName)
    {
        if (!_logMoves) return;
        Debug.Log($"[PROMOTION] {fromName} -> {toName} (index {piece.IndexInHouse})");
    }

    private void OnPiecePlaced(PieceDeployedEvent<Square> e)
    {
        if (!_logEvents) return;
        Debug.Log($"[EVENT] PiecePlaced: {e.Piece.Name}#{e.Piece.IndexInHouse} [{e.Piece.Owner}] at {Fmt(e.Position)}");
    }

    private void OnPieceDied(PieceDiedEvent e)
    {
        if (!_logEvents) return;
        Debug.Log($"[EVENT] PieceDied: {e.Piece.Name}#{e.Piece.IndexInHouse} [{e.Piece.Owner}]");
    }

    private void OnPieceRevived(PieceRevivedEvent e)
    {
        if (!_logEvents) return;
        Debug.Log($"[EVENT] PieceRevived: {e.Piece.Name}#{e.Piece.IndexInHouse} [{e.Piece.Owner}]");
    }

    private void OnPieceStartedAction(PieceStartedActionEvent e)
    {
        if (!_logEvents) return;
        Debug.Log($"[EVENT] PieceStartedAction: {e.Piece.Name}#{e.Piece.IndexInHouse} " +
                  $"({DescribeActivity(e.Activity)})");
    }

    private void OnPieceMoved(PieceMovedEvent<Square> e)
    {
        if (!_logEvents) return;
        Debug.Log($"[EVENT] PieceMoved: {e.Piece.Name}#{e.Piece.IndexInHouse} {Fmt(e.FromPosition)} -> {Fmt(e.ToPosition)}");
    }

    private void OnPieceAttacked(PieceAttackedEvent e)
    {
        if (!_logEvents) return;
        string target = e.Target == null ? "(none)" : $"{e.Target.Name}#{e.Target.IndexInHouse}";
        Debug.Log($"[EVENT] PieceAttacked ({e.Kind}, depth={e.Depth}): " +
                  $"{e.Attacker.Name}#{e.Attacker.IndexInHouse} -> {target}");
    }

    private void OnFacingChanged(FacingChangedEvent e)
    {
        if (!_logEvents) return;
        Debug.Log($"[EVENT] FacingChanged: {e.Piece.Name}#{e.Piece.IndexInHouse} {e.OldFacing} -> {e.NewFacing}");
    }

    private static string DescribeSelectable(ISelectable? s)
    {
        if (s is InfoOfPiece v)
        {
            var p = v.Piece;
            return $"{p.Name}#{p.IndexInHouse} [{p.Owner}] at {Fmt(v.PositionInGrid)}";
        }
        return s == null ? "(none)" : s.ToString() ?? "(unknown)";
    }

    private static string DescribeActivity(Activity a) => a switch
    {
        EmptyActivity => "Empty",
        MoveActivity m => $"Move {Fmt(m.From)} -> {Fmt(m.To)}",
        AttackActivity atk => $"Attack {Fmt(atk.TargetCell)}",
        ModifierActivity mod => $"Modifier {mod.ModifierName}",
        _ => a.GetType().Name,
    };

    private static string Fmt(Square s) => $"({s.X}, {s.Y}, {s.Z}, {s.W})";

    private static string FormatPath(IPath<Square> path)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < path.Positions.Count; i++)
        {
            if (i > 0) sb.Append(" -> ");
            sb.Append(Fmt(path.Positions[i]));
        }
        return sb.ToString();
    }
}