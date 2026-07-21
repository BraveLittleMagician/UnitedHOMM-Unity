#nullable enable

using System;
using UnityEngine;
using VContainer;
using static GridRenderer;

public class GridRenderer : MonoBehaviour, Instantiater
{
    private IEventBus _eventBus = null!;
    private IGlobalGeneratedSurroundingsHolder _holder = null!;
    private IObjectResolver _resolver = null!;
    private IGameStateManager _stateManager = null!;
    private IGridBuilder _gridBuilder = null!;
    private GameObject? _gridRoot = null;
    private AxisAlignedBox? _pendingBox = null;

    [SerializeField] private GameObject _layerPrefab = null!;
    [SerializeField] private float _cellSize = 1f;
    [SerializeField] private float _wGroupSpacing = 1.5f;

    private void Awake()
    {
        if (_layerPrefab == null) throw new ArgumentNullException(nameof(_layerPrefab));
        _gridBuilder = new GridBuilder(this, _resolver, _layerPrefab);
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
        ClearGrid();
        _gridBuilder.BuildGrid(box, _holder.Transform, _cellSize, _wGroupSpacing);
    }

    public GameObject Instance(GameObject layerPrefab, Transform wGroup) => Instantiate(_layerPrefab, wGroup);

    [Inject]
    public void Construct(IEventBus eventBus, IGlobalGeneratedSurroundingsHolder holder, IObjectResolver resolver, IGameStateManager stateManager)
    {
        _holder = holder ?? throw new ArgumentNullException(nameof(holder));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        _stateManager = stateManager ?? throw new ArgumentNullException(nameof(resolver));

        _stateManager.GameStarted += OnGameStarted;
        _eventBus.Subscribe<BoardConfigChangedEvent>(OnBoardConfigChanged);

        if (_pendingBox != null && _stateManager.IsGameStarted)
        {
            BuildGridInternal(_pendingBox);
            _pendingBox = null;
        }
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

    public interface Instantiater
    {
        public GameObject Instance(GameObject layerPrefab, Transform wGroup);
    }
}