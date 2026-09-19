#nullable enable

using System;
using UnityEngine;

public class GridRenderer : MonoBehaviour
{
    private IEventBus _eventBus = null!;
    private IGlobalGeneratedSurroundingsHolder _holder = null!;
    private IGameStateManager _stateManager = null!;
    private IGridBuilder _gridBuilder = null!;
    private GameObject? _gridRoot = null;
    private AxisAlignedBox? _lastBuiltBox = null;
    private AxisAlignedBox? _pendingBox = null;

    [SerializeField] private float _cellSize = 1f;
    [SerializeField] private float _wGroupSpacing = 1.5f;

    public void Initialize(IEventBus eventBus, IGlobalGeneratedSurroundingsHolder holder, IGameStateManager stateManager, IGridBuilder builder)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _holder = holder ?? throw new ArgumentNullException(nameof(holder));
        _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
        _gridBuilder = builder ?? throw new ArgumentNullException(nameof(builder));

        _stateManager.GameStarted += OnGameStarted;
        _eventBus.Subscribe<BoardConfigChangedEvent>(OnBoardConfigChanged);
    }

    private void OnDestroy()
    {
        _eventBus?.Unsubscribe<BoardConfigChangedEvent>(OnBoardConfigChanged);
        _stateManager.GameStarted -= OnGameStarted;
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
        BuildGridInternal(box);
    }
    private void OnGameStarted()
    {
        if (_pendingBox != null)
        {
            BuildGridInternal(_pendingBox);
            _pendingBox = null;
        }
    }
    private void BuildGridInternal(AxisAlignedBox box)
    {
        if (_lastBuiltBox == box) return;

        ClearGrid();
        _gridRoot = _gridBuilder.BuildGrid(box, _holder.Transform, _cellSize, _wGroupSpacing);
    }

    public void BuildGrid(AxisAlignedBox box)
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