#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

public class GridRenderer : MonoBehaviour
{
    private IEventBus _eventBus = null!;
    private IGameState _stateManager = null!;
    private IGridBuilder _gridBuilder = null!;
    private GameObject? _gridRoot = null;
    private IReadOnlyDictionary<Axis, (int Min, int Max)>? _lastBuiltBox = null;
    private IReadOnlyDictionary<Axis, (int Min, int Max)>? _pendingBox = null;

    [SerializeField] private float _cellSize = 1f;
    [SerializeField] private float _wGroupSpacing = 1.5f;

    public void Initialize(IEventBus eventBus, IGameState stateManager, IGridBuilder builder)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
        _gridBuilder = builder ?? throw new ArgumentNullException(nameof(builder));

        _stateManager.GameStarted += OnGameStarted;
        _eventBus.Subscribe<BoardConfigChangedEvent>(OnBoardConfigChanged);
    }

    private void OnDestroy()
    {
        _eventBus?.Unsubscribe<BoardConfigChangedEvent>(OnBoardConfigChanged);
        if (_stateManager != null) _stateManager.GameStarted -= OnGameStarted;
        ClearGrid();
    }

    private void ClearGrid()
    {
        if (_gridRoot != null)
        {
            Destroy(_gridRoot);
            _gridRoot = null;
        }
        _lastBuiltBox = null;
    }
    private void OnBoardConfigChanged(BoardConfigChangedEvent e)
    {
        if (!_stateManager.IsGameStarted) return;
        var newConfig = e.NewConfig;
        var axes = (MultipleAxes)newConfig.Axes;
        var box = new AxisAlignedBox(axes, newConfig.FieldSize, newConfig.WUp, newConfig.WDown);
        BuildGridInternal(box.Bounds);
    }
    private void OnGameStarted()
    {
        if (_pendingBox != null)
        {
            BuildGridInternal(_pendingBox);
            _pendingBox = null;
        }
    }
    private void BuildGridInternal(IReadOnlyDictionary<Axis, (int Min, int Max)> box)
    {
        if (_lastBuiltBox != null && BoundsEqual(_lastBuiltBox, box)) return;

        ClearGrid();
        _gridRoot = _gridBuilder.BuildGrid(box, transform, _cellSize, _wGroupSpacing);
    }
    private static bool BoundsEqual(
    IReadOnlyDictionary<Axis, (int Min, int Max)> a,
    IReadOnlyDictionary<Axis, (int Min, int Max)> b)
    {
        if (ReferenceEquals(a, b)) return true;
        if (a.Count != b.Count) return false;
        foreach (var (axis, range) in a)
        {
            if (!b.TryGetValue(axis, out var other)) return false;
            if (other.Min != range.Min || other.Max != range.Max) return false;
        }
        return true;
    }

    public void BuildGrid(IReadOnlyDictionary<Axis, (int Min, int Max)> box)
    {
        if (!_stateManager.IsGameStarted)
        {
            _pendingBox = box;
            return;
        }
        BuildGridInternal(box);
    }
    public Vector3 GridToWorld(Square square) => _gridBuilder.GridToWorldLocal(square.X, square.Y, square.Z, _cellSize);
}