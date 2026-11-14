#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class LayerPresenter : EventSubscriberBehaviour, IRunnable
{
    [SerializeField] private string _layersLayerName = "Layers";
    [SerializeField] private string _activeLayerName = "ActiveLayer";

    private int _layersLayer;
    private int _activeLayerLayer;
    private Material _layerMaterial = null!;
    private ViewState _viewState = null!;
    private LoaderOfBoard _boardLoader = null!;
    private PieceViewRegistry _viewRegistry = null!;
    private IEventBus _eventBus = null!;
    private ILogger _logger = null!;

    private readonly BoardViewFactory _viewFactory = new();
    private BoardViewHierarchy _view = BoardViewHierarchy.Empty;
    private LayerPositioner _positioner = new(Array.Empty<LayerContainer>());
    private LayerVisibilityPresenter _visibility = new(Array.Empty<LayerContainer>());
    private PieceDistributor _distributor = null!;

    private bool _initialized;

    public IReadOnlyList<LayerContainer> Containers => _view.Containers;

    public void Initialize(
        LoaderOfBoard boardLoader,
        ViewState viewState,
        PieceViewRegistry viewRegistry,
        Material layerMaterial,
        IEventBus eventBus,
        ILogger logger)
    {
        _boardLoader = boardLoader ?? throw new ArgumentNullException(nameof(boardLoader));
        _viewState = viewState ?? throw new ArgumentNullException(nameof(viewState));
        _viewRegistry = viewRegistry ?? throw new ArgumentNullException(nameof(viewRegistry));
        _layerMaterial = layerMaterial ?? throw new ArgumentNullException(nameof(layerMaterial));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _distributor = new PieceDistributor(Array.Empty<LayerContainer>(), _viewRegistry, _boardLoader);
    }

    public void Run()
    {
        if (_initialized) return;
        _layersLayer = LayerMask.NameToLayer(_layersLayerName);
        if (_layersLayer < 0) throw new InvalidOperationException($"Слой '{_layersLayerName}' не найден.");
        _activeLayerLayer = LayerMask.NameToLayer(_activeLayerName);
        if (_activeLayerLayer < 0) throw new InvalidOperationException($"Слой '{_activeLayerName}' не найден.");

        _viewState.ActiveWIndexChanged += OnViewChanged;
        AddSubscription(() => _viewState.ActiveWIndexChanged -= OnViewChanged);
        _viewState.ActiveLayerChanged += OnViewChanged;
        AddSubscription(() => _viewState.ActiveLayerChanged -= OnViewChanged);
        _viewState.AxisChanged += OnViewChanged;
        AddSubscription(() => _viewState.AxisChanged -= OnViewChanged);
        _viewState.ViewFlipped += OnViewChanged;
        AddSubscription(() => _viewState.ViewFlipped -= OnViewChanged);
        _viewState.LayerGapChanged += OnLayerGapChanged;
        AddSubscription(() => _viewState.LayerGapChanged -= OnLayerGapChanged);
        _viewState.LayerRollChanged += OnLayerRollChanged;
        AddSubscription(() => _viewState.LayerRollChanged -= OnLayerRollChanged);

        _eventBus.Subscribe<PieceDeployedEvent<Square>>(OnPieceDeployed);
        AddSubscription(() => _eventBus.Unsubscribe<PieceDeployedEvent<Square>>(OnPieceDeployed));
        _eventBus.Subscribe<PieceMovedEvent<Square>>(OnPieceMoved);
        AddSubscription(() => _eventBus.Unsubscribe<PieceMovedEvent<Square>>(OnPieceMoved));
        _eventBus.Subscribe<PieceRevivedEvent>(OnPieceRevived);
        AddSubscription(() => _eventBus.Unsubscribe<PieceRevivedEvent>(OnPieceRevived));

        Rebuild();
        _initialized = true;
    }

    private void OnViewChanged()
    {
        RefreshVisibility();
        RecalculateTargets();
    }

    private void OnLayerGapChanged() => ReflowLayers();
    private void OnLayerRollChanged() => LayoutChanged?.Invoke();

    private void OnPieceDeployed(PieceDeployedEvent<Square> e)
    {
        if (!_viewRegistry.TryGetByIndex(e.Piece.IndexInHouse, out var view) || view == null) return;
        _distributor.DistributeView(view, GridAxes.W);
        RefreshVisibility();
        RecalculateTargets();
    }

    private void OnPieceMoved(PieceMovedEvent<Square> e)
    {
        if (!_viewRegistry.TryGetByIndex(e.Piece.IndexInHouse, out var view) || view == null) return;
        _distributor.DistributeView(view, GridAxes.W);
        RefreshVisibility();
        RecalculateTargets();
    }

    private void OnPieceRevived(PieceRevivedEvent e)
    {
        if (!_viewRegistry.TryGetByIndex(e.Piece.IndexInHouse, out var view) || view == null) return;
        if (view.IsDead) return;
        _distributor.DistributeView(view, GridAxes.W);
        RecalculateTargets();
    }

    private void RefreshVisibility()
    {
        _visibility.Apply(
            _viewState.ActiveWIndex,
            _viewState.AxisIndex,
            _viewState.ActiveLayerIndex,
            _boardLoader.WWOffset,
            _layersLayer,
            _activeLayerLayer);
    }

    private void RecalculateTargets()
    {
        Square gridSize = _boardLoader.GridSize;
        float step = _boardLoader.Step;
        float layerGap = _viewState.LayerGap;

        _positioner.ApplyPositions(gridSize, step, layerGap);
        _positioner.UpdateSizes(gridSize, step);
        LayoutChanged?.Invoke();
    }

    public void Rebuild()
    {
        DestroyView();

        var ctx = new BoardViewFactoryContext
        {
            Parent = transform,
            Axis = GridAxes.W,
            AxisName = GridAxes.GetAxisName(GridAxes.W),
            GridSize = _boardLoader.GridSize,
            Step = _boardLoader.Step,
            LayerGap = _viewState.LayerGap,
            LayerMaterial = _layerMaterial,
            LayersLayerIndex = _layersLayer,
        };
        _view = _viewFactory.Create(ctx);
        var containers = _view.Containers;

        _positioner = new LayerPositioner(containers);
        _visibility = new LayerVisibilityPresenter(containers);
        _distributor = new PieceDistributor(containers, _viewRegistry, _boardLoader);

        _distributor.DistributeAll(GridAxes.W);
        RefreshVisibility();
        RecalculateTargets();

        Rebuilt?.Invoke();
        _logger.LogDebug($"LayerPresenter rebuilt with {containers.Count} W-containers");
    }

    public void RedistributePieces()
    {
        if (_view.Containers.Count == 0 || _view.PiecesSource == null) return;
        int axis = _view.Containers[0].Axis;
        _distributor.ReattachAll(_view.PiecesSource.transform, axis);
        RefreshVisibility();
        RecalculateTargets();
        PiecesRepositioned?.Invoke();
    }

    public void ReflowLayers()
    {
        Square gridSize = _boardLoader.GridSize;
        float step = _boardLoader.Step;
        float layerGap = _viewState.LayerGap;
        _positioner.UpdateSizes(gridSize, step);
        _positioner.ApplyPositions(gridSize, step, layerGap);
        _distributor.RepositionInContainers();
        RefreshVisibility();
        PiecesRepositioned?.Invoke();
        LayoutChanged?.Invoke();
    }

    public Transform? GetMoveTargetLayer(int layerIndex)
    {
        foreach (var c in _view.Containers)
            if (c.LayerIndex == layerIndex)
                return c.TargetTwin;
        return null;
    }

    private void DestroyView()
    {
        if (_boardLoader != null && _view.PiecesSource != null)
        {
            foreach (var view in _viewRegistry.AllViews)
                if (view != null) view.transform.SetParent(_boardLoader.transform, false);
        }
        _viewFactory.Destroy(_view);
        _view = BoardViewHierarchy.Empty;
    }

    public event Action? Rebuilt;
    public event Action? LayoutChanged;
    public event Action? PiecesRepositioned;
}