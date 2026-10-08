#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

public class BoardUI : EventSubscriberBehaviour, IRunnable
{
    private const int _maxLogEntries = 12;
    private const float _panelWidth = 320f;
    private const float _panelHeight = 500f;
    private const float _panelMargin = 10f;

    private IFlow _flow = null!;
    private IEventBus _eventBus = null!;
    private ISelectionHub _selectionHub = null!;
    private UIPointerProbe _uiPointerProbe = null!;

    private readonly List<string> _logEntries = new();

    private InfoOfPiece? _selectedInfo;
    private int _pieceCount;
    private bool _initialized;

    private Rect PanelRect => new(_panelMargin, _panelMargin, _panelWidth, _panelHeight);


    private void OnGUI()
    {
        if (!_initialized) return;

        GUILayout.BeginArea(PanelRect, GUI.skin.box);

        GUILayout.Label("=== Board UI ===");
        GUILayout.Space(4);

        DrawStateSection();
        GUILayout.Space(8);

        DrawSelectedPieceSection();
        GUILayout.Space(8);

        DrawActionsSection();
        GUILayout.Space(8);

        DrawLogSection();

        GUILayout.EndArea();
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
    private void DrawStateSection()
    {
        GUILayout.Label("--- Состояние ---");
        GUILayout.Label($"Фигур на доске: {_pieceCount}");
    }
    private void DrawSelectedPieceSection()
    {
        GUILayout.Label("--- Выбранная фигура ---");

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

    private void DrawLogSection()
    {
        GUILayout.Label("--- Лог ---");

        for (int i = 0; i < _logEntries.Count; i++) GUILayout.Label(_logEntries[i]);
    }
    protected override void OnCleanup()
    {
        if (_uiPointerProbe != null)
            _uiPointerProbe.UnregisterRegion(PanelRect);
    }
    public void Initialize(IFlow flow, IEventBus eventBus, ISelectionHub selectionHub, UIPointerProbe uiPointerProbe)
    {
        _flow = flow ?? throw new ArgumentNullException(nameof(flow));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _selectionHub = selectionHub ?? throw new ArgumentNullException(nameof(selectionHub));
        _uiPointerProbe = uiPointerProbe != null ? uiPointerProbe : throw new ArgumentNullException(nameof(uiPointerProbe));
    }
    public void Run()
    {
        if (_flow == null) throw new InvalidOperationException("BoardUI не инициализирован");

        _eventBus.Subscribe<PieceDeployedEvent<Square>>(OnPieceDeployed);
        AddSubscription(() => _eventBus.Unsubscribe<PieceDeployedEvent<Square>>(OnPieceDeployed));

        _eventBus.Subscribe<PieceMovedEvent<Square>>(OnPieceMoved);
        AddSubscription(() => _eventBus.Unsubscribe<PieceMovedEvent<Square>>(OnPieceMoved));

        _eventBus.Subscribe<PieceDiedEvent>(OnPieceDied);
        AddSubscription(() => _eventBus.Unsubscribe<PieceDiedEvent>(OnPieceDied));

        _selectionHub.OnSelectionChanged += OnSelectionChanged;
        AddSubscription(() => _selectionHub.OnSelectionChanged -= OnSelectionChanged);

        _uiPointerProbe.RegisterRegion(PanelRect);

        _initialized = true;
        AddLog("BoardUI готов");
    }
}