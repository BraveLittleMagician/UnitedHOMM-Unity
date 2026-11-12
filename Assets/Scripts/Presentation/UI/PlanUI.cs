#nullable enable
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class PlanUI : GuiPanelBehaviour, IRunnable
{
    [SerializeField] private float _panelWidth = 460f;
    [SerializeField] private float _panelHeight = 420f;
    [SerializeField] private float _panelMargin = 10f;

    private IFlow _flow = null!;
    private TurnManager _turnManager = null!;
    private SelectionState _selectionState = null!;
    private SelectionController _selectionController = null!;
    private TargetSelectionController _targetSelectionController = null!;
    private ModifierActivationHandler _modifierActivationHandler = null!;
    private PendingModalManager _pendingModalManager = null!;
    private MatchState _matchState = null!;
    private ModifierHost _modifierHost = null!;
    private ILogger _logger = null!;

    private Vector2 _scroll;

    protected override Rect ComputePanelRect(int screenWidth, int screenHeight)
    {
        float leftViewportWidth = screenWidth * ViewportLayout.LeftCameraWidthFraction;
        float width = Mathf.Min(_panelWidth, leftViewportWidth - 2f * _panelMargin);
        float maxHeight = Mathf.Max(1f, screenHeight - 2f * _panelMargin);
        float height = Mathf.Min(_panelHeight, maxHeight);
        float x = leftViewportWidth - width - _panelMargin;
        float y = _panelMargin;
        return new Rect(x, y, width, height);
    }

    public void Initialize(
        IFlow flow,
        TurnManager turnManager,
        SelectionState selectionState,
        SelectionController selectionController,
        TargetSelectionController targetSelectionController,
        ModifierActivationHandler modifierActivationHandler,
        PendingModalManager pendingModalManager,
        UIPointerProbe uiPointerProbe,
        MatchState matchState,
        ModifierHost modifierHost,
        ILogger logger)
    {
        _flow = flow ?? throw new ArgumentNullException(nameof(flow));
        _turnManager = turnManager ?? throw new ArgumentNullException(nameof(turnManager));
        _selectionState = selectionState ?? throw new ArgumentNullException(nameof(selectionState));
        _selectionController = selectionController ?? throw new ArgumentNullException(nameof(selectionController));
        _targetSelectionController = targetSelectionController ?? throw new ArgumentNullException(nameof(targetSelectionController));
        _modifierActivationHandler = modifierActivationHandler ?? throw new ArgumentNullException(nameof(modifierActivationHandler));
        _pendingModalManager = pendingModalManager ?? throw new ArgumentNullException(nameof(pendingModalManager));
        _matchState = matchState ?? throw new ArgumentNullException(nameof(matchState));
        _modifierHost = modifierHost ?? throw new ArgumentNullException(nameof(modifierHost));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        InitializePanel(uiPointerProbe);
    }

    public void Run() => RegisterPanel();

    private void OnGUI()
    {
        if (!IsRegistered || _flow == null) return;
        EnsurePanelRectCurrent();

        GUILayout.BeginArea(PanelRect);
        _scroll = GUILayout.BeginScrollView(_scroll);
        DrawContent();
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    private void DrawContent()
    {
        GUILayout.Box("Turn & Plan");

        if (_matchState.IsOver)
        {
            if (_matchState.Winner.HasValue)
                GUILayout.Label($"GAME OVER — Winner: {_matchState.Winner.Value}");
            else
                GUILayout.Label("GAME OVER — Draw");
            GUILayout.Space(8);
            if (GUILayout.Button("Restart", GUILayout.Height(40)))
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            return;
        }

        GUILayout.Label($"Active team: {_turnManager.ActivePlayer}");
        GUILayout.Label($"Plan actions: {_flow.UndoCount} (redo: {_flow.RedoCount})");

        var selected = _selectionState.SelectedInfo;
        if (selected != null && !selected.IsDead) DrawSelectedPiece(selected.Piece);
        else GUILayout.Label("Selected: (none)");

        DrawActiveModifiers(selected);
        DrawFooter();
    }

    private void DrawSelectedPiece(IPiece piece)
    {
        int effectiveDmg = piece.AttackDamage + _modifierHost.GetEffectiveDamageBonus(piece);
        string dmgLabel = effectiveDmg > piece.AttackDamage
            ? $"{piece.AttackDamage}(+{effectiveDmg - piece.AttackDamage})"
            : piece.AttackDamage.ToString();

        string status = string.Empty;
        if (_modifierHost.IsAttackBlocked(piece)) status += " [Cannot attack]";
        if (_modifierHost.IsMovementBlocked(piece)) status += " [Cannot move]";

        GUILayout.Label(
            $"Selected: {piece.Name} [{piece.Owner}] " +
            $"HP={piece.Health}/{piece.HealthNormal}  " +
            $"Charges={piece.AttackCharges}/{piece.MaxAttackCharges}  " +
            $"Dmg={dmgLabel}  Facing={piece.Facing}{status}");

        var modifiers = _modifierHost.GetModifiers(piece.IndexInHouse);
        if (modifiers.Count == 0) return;

        GUILayout.Label("All modifiers:");
        for (int i = 0; i < modifiers.Count; i++)
        {
            var m = modifiers[i];
            if (ShapeModifierExtensions.IsPureShape(m)) continue;
            string dur = m.CurrentDuration < 0 ? "∞" : m.CurrentDuration.ToString();
            string ch = m.CurrentCharges < 0 ? "∞" : m.CurrentCharges.ToString();
            GUILayout.Label($"  {GetDisplayName(m.ModifierName)} (dur:{dur}, ch:{ch})");
        }
    }

    private void DrawActiveModifiers(InfoOfPiece? selected)
    {
        if (selected == null || selected.IsDead) return;

        var piece = selected.Piece;
        var activeModifiers = _modifierActivationHandler.GetSelectedActiveModifiers(piece);
        if (activeModifiers.Count == 0) return;

        bool prevEnabled = GUI.enabled;
        bool busy = _pendingModalManager.HasPending;
        bool targeting = _targetSelectionController.IsActive;

        GUILayout.Space(4);
        GUILayout.Label("Active Modifiers:");

        for (int i = 0; i < activeModifiers.Count; i++)
        {
            var modifier = activeModifiers[i];
            if (modifier is ITargetedModifier targeted)
            {
                bool isAimingThis = targeting
                    && ReferenceEquals(_targetSelectionController.ActiveModifier, targeted);

                string displayName = GetDisplayName(modifier.ModifierName);
                string chargeText = FormatCharges(modifier);
                string label = isAimingThis
                    ? $"{displayName} ({chargeText}) [Cancel Aim]"
                    : $"{displayName} ({chargeText}) [Aim]";

                bool canAim = !busy
                    && (!targeting || isAimingThis)
                    && _modifierActivationHandler.CanActivateModifier(piece, modifier);

                GUI.enabled = prevEnabled && canAim;
                if (GUILayout.Button(label, GUILayout.Height(28)))
                {
                    if (isAimingThis)
                        _targetSelectionController.Cancel();
                    else
                        _targetSelectionController.Begin(selected, targeted);
                }
                GUI.enabled = prevEnabled;
            }
            else
            {
                bool canUse = !busy && !targeting
                    && _modifierActivationHandler.CanActivateModifier(piece, modifier);

                string label = $"{GetDisplayName(modifier.ModifierName)} ({FormatCharges(modifier)})";

                GUI.enabled = prevEnabled && canUse;
                if (GUILayout.Button(label, GUILayout.Height(28)))
                    _modifierActivationHandler.HandleModifierActivate(piece, modifier);
                GUI.enabled = prevEnabled;
            }
        }
    }

    private void DrawFooter()
    {
        bool prevEnabled = GUI.enabled;
        bool busy = _pendingModalManager.HasPending;

        GUILayout.Space(6);
        GUILayout.BeginHorizontal();

        GUI.enabled = prevEnabled && !busy && _flow.UndoCount > 0;
        if (GUILayout.Button("Undo", GUILayout.Height(32)))
        {
            _targetSelectionController.Cancel();
            var res = _flow.Undo();
            if (res.IsSuccess) _selectionController.RefreshMarkers();
            else _logger.LogWarning($"[UNDO] {res.Error}");
        }

        GUI.enabled = prevEnabled && !busy && _flow.UndoCount > 0;
        if (GUILayout.Button("Undo All", GUILayout.Height(32)))
        {
            _targetSelectionController.Cancel();
            var res = _flow.UndoAll();
            if (res.IsSuccess) _selectionController.RefreshMarkers();
            else _logger.LogWarning($"[UNDO-ALL] {res.Error}");
        }

        GUI.enabled = prevEnabled && !busy && _flow.RedoCount > 0;
        if (GUILayout.Button("Redo", GUILayout.Height(32)))
        {
            _targetSelectionController.Cancel();
            var res = _flow.Redo();
            if (res.IsSuccess) _selectionController.RefreshMarkers();
            else _logger.LogWarning($"[REDO] {res.Error}");
        }

        GUI.enabled = prevEnabled && !busy;
        if (GUILayout.Button("Confirm Turn", GUILayout.Height(32)))
        {
            _targetSelectionController.Cancel();
            var res = _flow.Commit();
            if (!res.IsSuccess) _logger.LogWarning($"[COMMIT] {res.Error}");
        }

        GUI.enabled = prevEnabled;
        GUILayout.EndHorizontal();
    }

    private static string FormatCharges(IActiveModifier modifier)
    {
        return modifier.MaxCharges < 0
            ? "∞"
            : $"{modifier.CurrentCharges}/{modifier.MaxCharges}";
    }

    private static string GetDisplayName(string modifierName) => modifierName switch
    {
        "RotateRight" => "Rotate Right",
        "RotateLeft" => "Rotate Left",
        "AttackBoost" => "Attack Boost",
        "Blind" => "Blind Self",
        "Stun" => "Stun Self",
        "Fireball" => "Fireball",
        "SelfHeal" => "Self Heal",
        _ => modifierName,
    };
}