#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class LoaderOfBoard : MonoBehaviour, IRunnable, IDisposable
{
    [SerializeField] private string _piecesLayerName = "Pieces";
    [SerializeField] private float _spacing = 0f;
    [SerializeField] private float _wSpread = 1f;

    private PieceSpawner _spawner = null!;
    private PieceViewRegistry _viewRegistry = null!;
    private IEventBus _eventBus = null!;
    private ILogger _logger = null!;
    private Square _gridSize;
    private bool _subscribed;
    private bool _disposed;

    public Square GridSize => _gridSize;
    public float Step => 1f + _spacing;
    public float Spacing => _spacing;
    public float WSpread => _wSpread;
    public PieceViewRegistry ViewRegistry => _viewRegistry;
    public IReadOnlyCollection<InfoOfPiece> AllPieceViews => _viewRegistry.AllViews;

    private int GetPiecesLayer()
    {
        int layer = LayerMask.NameToLayer(_piecesLayerName);
        return layer >= 0 ? layer : 0;
    }
    private void OnPieceDeployed(PieceDeployedEvent<Square> e)
    {
        if (_viewRegistry.TryGetByIndex(e.Piece.IndexInHouse, out var existing) && existing != null)
        {
            _viewRegistry.Unregister(e.Piece.IndexInHouse);
            UnityObjectCleanup.DestroySafe(existing.gameObject);
        }

        var worldLocal = ComputeWorldLocal(e.Position);
        var view = _spawner.Spawn(e.Piece, e.Position, worldLocal, GetPiecesLayer());
        _viewRegistry.Register(view);

        _logger.LogDebug($"Spawned view for {e.Piece} at {e.Position}");
    }
    private void OnPieceMoved(PieceMovedEvent<Square> e)
    {
        if (!_viewRegistry.TryGetByIndex(e.Piece.IndexInHouse, out var view) || view == null) return;

        view.transform.localPosition = ComputeWorldLocal(e.ToPosition);
        _viewRegistry.Move(e.Piece.IndexInHouse, e.FromPosition, e.ToPosition);
    }
    private void OnPieceDied(PieceDiedEvent e)
    {
        if (!_viewRegistry.TryGetByIndex(e.Piece.IndexInHouse, out var view) || view == null) return;
        view.SetDead(true);
    }
    private void OnPieceRevived(PieceRevivedEvent e)
    {
        if (!_viewRegistry.TryGetByIndex(e.Piece.IndexInHouse, out var view) || view == null) return;
        view.SetDead(false);
    }
    private void OnFacingChanged(FacingChangedEvent e)
    {
        if (!_viewRegistry.TryGetByIndex(e.Piece.IndexInHouse, out var view) || view == null) return;
        view.SetFacing(e.NewFacing);
    }
    private Vector3 ComputeWorldLocal(Square pos) => GridAxes.GridToWorldLocal(pos, _gridSize, Step, _wSpread);

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_subscribed && _eventBus != null)
        {
            _eventBus.Unsubscribe<PieceDeployedEvent<Square>>(OnPieceDeployed);
            _eventBus.Unsubscribe<PieceMovedEvent<Square>>(OnPieceMoved);
            _eventBus.Unsubscribe<PieceDiedEvent>(OnPieceDied);
            _eventBus.Unsubscribe<PieceRevivedEvent>(OnPieceRevived);
            _eventBus.Unsubscribe<FacingChangedEvent>(OnFacingChanged);
            _subscribed = false;
        }

        if (_viewRegistry != null)
        {
            foreach (var view in _viewRegistry.AllViews)
                if (view != null) UnityObjectCleanup.DestroySafe(view.gameObject);
            _viewRegistry.Clear();
        }
    }
    public void Run()
    {
        if (_spawner == null) throw new InvalidOperationException("LoaderOfBoard not initialized.");
        if (_subscribed) return;

        _eventBus.Subscribe<PieceDeployedEvent<Square>>(OnPieceDeployed);
        _eventBus.Subscribe<PieceMovedEvent<Square>>(OnPieceMoved);
        _eventBus.Subscribe<PieceDiedEvent>(OnPieceDied);
        _eventBus.Subscribe<PieceRevivedEvent>(OnPieceRevived);
        _eventBus.Subscribe<FacingChangedEvent>(OnFacingChanged);
        _subscribed = true;

        int layer = LayerMask.NameToLayer(_piecesLayerName);
        if (layer < 0) throw new InvalidOperationException($"Слой '{_piecesLayerName}' не найден.");
    }
    public void SetSpacing(float spacing) => _spacing = Mathf.Max(0f, spacing);
    public void Initialize(PieceSpawner spawner, PieceViewRegistry viewRegistry, IEventBus eventBus, ILogger logger, Square gridSize)
    {
        _spawner = spawner ?? throw new ArgumentNullException(nameof(spawner));
        _viewRegistry = viewRegistry ?? throw new ArgumentNullException(nameof(viewRegistry));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _gridSize = gridSize;
    }
    public void SetGridSize(Square gridSize) => _gridSize = gridSize;

    private void OnDestroy() => Dispose();
}