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
    private UIPointerProbe _uiPointerProbe = null!;

    private readonly List<string> _logEntries = new();

    private IPiece? _selectedPiece;
    private IRoom? _selectedPieceRoom;
    private Square? _selectedPiecePosition;
    private int _pieceCount;
    private bool _initialized;

    private Rect PanelRect => new(_panelMargin, _panelMargin, _panelWidth, _panelHeight);

    public void Initialize(IFlow flow, IEventBus eventBus, UIPointerProbe uiPointerProbe)
    {
        _flow = flow ?? throw new ArgumentNullException(nameof(flow));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _uiPointerProbe = uiPointerProbe ?? throw new ArgumentNullException(nameof(uiPointerProbe));
    }

    public void Run()
    {
        if (_flow == null) throw new InvalidOperationException("BoardUI не инициализирован");

        _eventBus.Subscribe<PieceDeployedEvent<Square>>(OnPieceDeployed);
        AddSubscription(() => _eventBus.Unsubscribe<PieceDeployedEvent<Square>>(OnPieceDeployed));

        _eventBus.Subscribe<PieceDiedEvent>(OnPieceDied);
        AddSubscription(() => _eventBus.Unsubscribe<PieceDiedEvent>(OnPieceDied));

        _eventBus.Subscribe<PieceSelectedEvent<Square>>(OnPieceSelected);
        AddSubscription(() => _eventBus.Unsubscribe<PieceSelectedEvent<Square>>(OnPieceSelected));

        _eventBus.Subscribe<PieceMovedEvent<Square>>(OnPieceMoved);
        AddSubscription(() => _eventBus.Unsubscribe<PieceMovedEvent<Square>>(OnPieceMoved));

        _uiPointerProbe.RegisterRegion(PanelRect);

        _initialized = true;
        AddLog("BoardUI готов");
    }

    protected override void OnCleanup()
    {
        _uiPointerProbe?.UnregisterRegion(PanelRect);
    }

    private void OnPieceDeployed(PieceDeployedEvent<Square> e)
    {
        _pieceCount++;
        AddLog($"Развёрнута {e.Piece.Name} в {e.Position}");
    }

    private void OnPieceDied(PieceDiedEvent e)
    {
        _pieceCount = Math.Max(0, _pieceCount - 1);
        AddLog($"Погибла {e.Piece.Name}");

        if (_selectedPiece != null && _selectedPiece.IndexInHouse == e.Piece.IndexInHouse)
        {
            _selectedPiece = null;
            _selectedPieceRoom = null;
            _selectedPiecePosition = null;
        }
    }

    private void OnPieceSelected(PieceSelectedEvent<Square> e)
    {
        _selectedPiece = e.Piece;
        _selectedPiecePosition = e.Position;
        AddLog($"Выбрана {e.Piece.Name}");
    }
    private void OnPieceMoved(PieceMovedEvent<Square> e)
    {
        if (_selectedPiece != null && _selectedPiece.IndexInHouse == e.Piece.IndexInHouse)
            _selectedPiecePosition = e.ToPosition;

        AddLog($"{e.Piece.Name}: {e.FromPosition} → {e.ToPosition}");
    }
    private void AddLog(string message)
    {
        _logEntries.Add(message);
        if (_logEntries.Count > _maxLogEntries)
            _logEntries.RemoveAt(0);
    }

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

    private void DrawStateSection()
    {
        GUILayout.Label("--- Состояние ---");
        GUILayout.Label($"Фигур на доске: {_pieceCount}");
    }

    private void DrawSelectedPieceSection()
    {
        GUILayout.Label("--- Выбранная фигура ---");

        if (_selectedPiece == null)
        {
            GUILayout.Label("Ничего не выбрано");
            return;
        }

        GUILayout.Label($"Имя: {_selectedPiece.Name}");
        GUILayout.Label($"Игрок: {_selectedPiece.Owner}");
        GUILayout.Label($"Здоровье: {_selectedPiece.Health}");
        GUILayout.Label($"Индекс: {_selectedPiece.IndexInHouse}");

        if (_selectedPiecePosition.HasValue)
            GUILayout.Label($"Позиция: {_selectedPiecePosition.Value}");
    }

    private void DrawActionsSection()
    {
        GUILayout.Label("--- Действия ---");

        GUI.enabled = _selectedPiece != null;
        if (GUILayout.Button("Снять выделение"))
        {
            _selectedPiece = null;
            _selectedPieceRoom = null;
            _selectedPiecePosition = null;
            AddLog("Выделение снято");
        }
        GUI.enabled = true;

        if (GUILayout.Button("Очистить лог"))
            _logEntries.Clear();
    }

    private void DrawLogSection()
    {
        GUILayout.Label("--- Лог ---");

        for (int i = 0; i < _logEntries.Count; i++)
            GUILayout.Label(_logEntries[i]);
    }
}