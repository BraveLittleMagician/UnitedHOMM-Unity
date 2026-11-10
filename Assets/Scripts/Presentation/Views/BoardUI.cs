#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

public class BoardUI : EventSubscriberBehaviour, IRunnable
{
    private const int _maxLogEntries = 12;
    private const float _panelWidth = 420f;
    private const float _panelHeight = 720f;
    private const float _panelMargin = 10f;

    private IFlow _flow = null!;
    private IEventBus _eventBus = null!;
    private ISelectionHub _selectionHub = null!;
    private UIPointerProbe _uiPointerProbe = null!;
    private ActionPlan _actionPlan = null!;
    private PieceRegistry _pieceRegistry = null!;
    private TurnManager _turnManager = null!;
    private IHouse _house = null!;
    private IMovementResolver _movementValidator = null!;
    private IAttackTargetProvider _attackTargetProvider = null!;
    private MatchState _matchState = null!;
    private Func<Vector3Int> _gridSizeProvider = null!;

    private readonly List<string> _logEntries = new();
    private InfoOfPiece? _selectedInfo;
    private int _pieceCount;
    private bool _initialized;
    private Vector2 _scroll;

    private Rect PanelRect
    {
        get
        {
            float w = Mathf.Min(_panelWidth, Screen.width * 0.95f);
            float h = Mathf.Min(_panelHeight, Screen.height * 0.95f);
            return new Rect(_panelMargin, _panelMargin, w, h);
        }
    }



    private void OnGUI()
    {
        if (!_initialized) return;

        GUILayout.BeginArea(PanelRect, GUI.skin.box);
        _scroll = GUILayout.BeginScrollView(_scroll);

        GUILayout.Label("=== Board UI ===");
        GUILayout.Space(4);

        DrawStateSection();
        GUILayout.Space(8);

        DrawSelectedPieceSection();
        GUILayout.Space(8);

        DrawActionsSection();
        GUILayout.Space(8);

        DrawUndoSection();
        GUILayout.Space(8);

        DrawSmokeSection();
        GUILayout.Space(8);

        DrawRegistrySection();
        GUILayout.Space(8);

        DrawLogSection();

        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    private void DrawStateSection()
    {
        GUILayout.Label("--- Состояние ---");
        GUILayout.Label($"Фигур на доске: {_pieceCount}");
        GUILayout.Label($"Активный игрок: {_turnManager.ActivePlayer}");
        GUILayout.Label($"Действий в плане: {_actionPlan.Count}");

        if (_matchState.IsOver)
        {
            string winner = _matchState.Winner.HasValue
                ? _matchState.Winner.Value.ToString()
                : "draw";
            GUILayout.Label($"=== ИГРА ОКОНЧЕНА: {winner} ===");
        }
    }
    private void DrawSelectedPieceSection()
    {
        GUILayout.Label("--- Выбранная фигура (P1 selection) ---");
        if (_selectedInfo == null)
        {
            GUILayout.Label("Ничего не выбрано");
            return;
        }
        GUILayout.Label($"Имя: {_selectedInfo.Name}");
        GUILayout.Label($"Игрок: {_selectedInfo.Player}");
        GUILayout.Label($"Индекс: {_selectedInfo.IndexInHouse}");
        var pos = _selectedInfo.PositionInGrid;
        GUILayout.Label($"Позиция: ({pos.x}, {pos.y}, {pos.z})");
    }
    private void DrawActionsSection()
    {
        GUILayout.Label("--- Действия ---");
        GUI.enabled = _selectedInfo != null;
        if (GUILayout.Button("Снять выделение")) _selectionHub.DropSelection();
        GUI.enabled = true;
        if (GUILayout.Button("Очистить лог")) _logEntries.Clear();
    }
    private void DrawUndoSection()
    {
        GUILayout.Label("--- Ход ---");

        GUI.enabled = !_actionPlan.IsEmpty && !_matchState.IsOver;
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Undo")) TryUndo();
        if (GUILayout.Button("Undo All")) TryUndoAll();
        GUILayout.EndHorizontal();

        GUI.enabled = !_matchState.IsOver;
        if (GUILayout.Button("Commit Turn")) TryCommit();
        GUI.enabled = true;

        if (_actionPlan.IsEmpty)
            GUILayout.Label("(план пуст)");
    }
    private void DrawSmokeSection()
    {
        GUILayout.Label("--- Phase 7 Smoke ---");
        GUI.enabled = !_matchState.IsOver;
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Move Random")) TryMoveRandom();
        if (GUILayout.Button("Attack Random")) TryAttackRandom();
        GUILayout.EndHorizontal();
        GUI.enabled = true;
    }
    private void DrawRegistrySection()
    {
        GUILayout.Label("--- Registry (проверка Undo) ---");
        int shown = 0;
        foreach (var model in _pieceRegistry.AllLiveModels)
        {
            var pos = model.PositionInGrid;
            string flags = string.Empty;
            if (model.HasMovedThisTurn) flags += " [moved]";
            if (model.HasPromotedThisTurn) flags += " [promoted]";

            GUILayout.Label(
                $"  {model.Name} [{model.Player}] @({pos.x},{pos.y},{pos.z}) " +
                $"HP={model.Hp}/{model.MaxHp} AC={model.AttackCharges}/{model.MaxAttackCharges}{flags}");

            if (++shown >= 20)
            {
                GUILayout.Label("  ...");
                break;
            }
        }
        if (shown == 0) GUILayout.Label("  (пусто)");
    }
    private void DrawLogSection()
    {
        GUILayout.Label("--- Лог ---");
        for (int i = 0; i < _logEntries.Count; i++)
            GUILayout.Label(_logEntries[i]);
    }
    private void TryUndo()
    {
        if (_actionPlan.Undo()) AddLog("UNDO: восстановлено");
        else AddLog("UNDO: нечего откатывать");
    }
    private void TryUndoAll()
    {
        if (_actionPlan.UndoAll()) AddLog("UNDO ALL: полный откат");
        else AddLog("UNDO ALL: нечего откатывать");
    }
    private void TryCommit()
    {
        bool wasEmpty = _actionPlan.IsEmpty;
        _actionPlan.Commit();
        AddLog(wasEmpty
            ? $"COMMIT: план был пуст, ход передан {_turnManager.ActivePlayer}"
            : $"COMMIT: ход передан {_turnManager.ActivePlayer}");
    }
    private void TryMoveRandom()
    {
        var active = _turnManager.ActivePlayer;
        var gridSize = _gridSizeProvider();

        foreach (var model in _pieceRegistry.AllLiveModels)
        {
            if (model.Player != active) continue;
            if (model.HasMovedThisTurn) continue;

            var piece = FindPieceByPieceId(model.PieceId);
            if (piece is null) continue;

            var moves = _movementValidator.GetValidMoves(model, gridSize);
            if (moves.Count == 0) continue;

            foreach (var target in moves)
            {
                if (!_movementValidator.TryFindPath(model, target, gridSize, out var path))
                    continue;
                if (path is null) continue;

                var action = new MoveAction(piece, path, _flow);
                if (!_actionPlan.TryAdd(action)) continue;

                AddLog($"SMOKE MOVE: {piece.Name} → ({target.x},{target.y},{target.z})");
                return;
            }
        }

        AddLog("SMOKE MOVE: нет доступных ходов");
    }
    private void TryAttackRandom()
    {
        var active = _turnManager.ActivePlayer;
        var gridSize = _gridSizeProvider();

        foreach (var model in _pieceRegistry.AllLiveModels)
        {
            if (model.Player != active) continue;
            if (model.AttackCharges <= 0) continue;

            var targets = _attackTargetProvider.GetAttackTargets(model, gridSize);
            if (targets.Count == 0) continue;

            var piece = FindPieceByPieceId(model.PieceId);
            if (piece is null) continue;

            foreach (var target in targets)
            {
                if (!_attackTargetProvider.TryFindAttackPath(model, target, gridSize, out var path))
                    continue;
                if (path is null) continue;

                var action = new AttackAction(piece, path, _flow, AttackKind.Melee);
                if (!_actionPlan.TryAdd(action)) continue;

                AddLog($"SMOKE ATTACK: {piece.Name} → ({target.x},{target.y},{target.z})");
                return;
            }
        }

        AddLog("SMOKE ATTACK: нет доступных атак");
    }
    private IPiece? FindPieceByPieceId(long pieceId)
    {
        foreach (var room in _house.Rooms.Values)
        {
            foreach (var piece in room.GetAllPieces())
                if (piece.PieceId == pieceId) return piece;
        }
        return null;
    }
    private void OnSelectionChanged(ISelectable? oldSelection, ISelectable? newSelection)
    {
        if (newSelection is MonoBehaviour mono &&
            mono.TryGetComponent<InfoOfPiece>(out var info))
        {
            _selectedInfo = info;
            AddLog($"Выбрана {info.Name}");
        }
        else
        {
            _selectedInfo = null;
        }
    }
    private void OnPieceDeployed(PieceDeployedEvent<Square> e)
    {
        _pieceCount++;
        AddLog($"Развёрнута {e.Piece.Name} в {e.Position}");
    }
    private void OnPieceMoved(PieceMovedEvent<Square> e)
    {
        AddLog($"{e.Piece.Name}: {e.FromPosition} → {e.ToPosition}");
    }
    private void OnPieceAttacked(PieceAttackedEvent e)
    {
        AddLog($"{e.Kind}: {e.Attacker.Name} → {e.Target.Name} " +
               $"(HP={e.Target.Health})");
    }
    private void OnPieceDied(PieceDiedEvent e)
    {
        _pieceCount = Math.Max(0, _pieceCount - 1);
        AddLog($"Погибла {e.Piece.Name}");
        if (_selectedInfo != null && _selectedInfo.IndexInHouse == e.Piece.IndexInHouse)
            _selectedInfo = null;
    }
    private void AddLog(string message)
    {
        _logEntries.Add(message);
        if (_logEntries.Count > _maxLogEntries)
            _logEntries.RemoveAt(0);
    }

    protected override void OnCleanup()
    {
        if (_uiPointerProbe != null)
            _uiPointerProbe.UnregisterRegion(PanelRect);
    }

    public void Initialize(IFlow flow, IEventBus eventBus, ISelectionHub selectionHub, UIPointerProbe uiPointerProbe, ActionPlan actionPlan, PieceRegistry pieceRegistry, TurnManager turnManager, IHouse house, IMovementResolver movementValidator, IAttackTargetProvider attackTargetProvider, MatchState matchState, Func<Vector3Int> gridSizeProvider)
    {
        _flow = flow ?? throw new ArgumentNullException(nameof(flow));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _selectionHub = selectionHub ?? throw new ArgumentNullException(nameof(selectionHub));
        _uiPointerProbe = uiPointerProbe ?? throw new ArgumentNullException(nameof(uiPointerProbe));

        _actionPlan = actionPlan ?? throw new ArgumentNullException(nameof(actionPlan));
        _pieceRegistry = pieceRegistry ?? throw new ArgumentNullException(nameof(pieceRegistry));
        _turnManager = turnManager ?? throw new ArgumentNullException(nameof(turnManager));
        _house = house ?? throw new ArgumentNullException(nameof(house));
        _movementValidator = movementValidator ?? throw new ArgumentNullException(nameof(movementValidator));
        _attackTargetProvider = attackTargetProvider ?? throw new ArgumentNullException(nameof(attackTargetProvider));
        _matchState = matchState ?? throw new ArgumentNullException(nameof(matchState));
        _gridSizeProvider = gridSizeProvider ?? throw new ArgumentNullException(nameof(gridSizeProvider));

        _initialized = true;
    }
    public void Run()
    {

        _eventBus.Subscribe<PieceDeployedEvent<Square>>(OnPieceDeployed);
        AddSubscription(() => _eventBus.Unsubscribe<PieceDeployedEvent<Square>>(OnPieceDeployed));

        _eventBus.Subscribe<PieceMovedEvent<Square>>(OnPieceMoved);
        AddSubscription(() => _eventBus.Unsubscribe<PieceMovedEvent<Square>>(OnPieceMoved));

        _eventBus.Subscribe<PieceDiedEvent>(OnPieceDied);
        AddSubscription(() => _eventBus.Unsubscribe<PieceDiedEvent>(OnPieceDied));

        _eventBus.Subscribe<PieceAttackedEvent>(OnPieceAttacked);
        AddSubscription(() => _eventBus.Unsubscribe<PieceAttackedEvent>(OnPieceAttacked));

        _selectionHub.OnSelectionChanged += OnSelectionChanged;
        AddSubscription(() => _selectionHub.OnSelectionChanged -= OnSelectionChanged);

        _uiPointerProbe.RegisterRegion(PanelRect);
        _initialized = true;
        AddLog("BoardUI готов");
    }
}