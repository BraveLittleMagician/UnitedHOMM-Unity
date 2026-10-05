#nullable enable

using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public sealed class Presenter : IDisposable
{
    private const float _cellSize = 1f;

    private readonly IEventBus _eventBus;
    private readonly PiecePrefabRegistry _prefabs;
    private readonly Transform _piecesRoot;
    private readonly ILogger _logger;

    private readonly Dictionary<BigInteger, GameObject> _views = new();
    private readonly Dictionary<BigInteger, IPiece> _pieces = new();

    private Material _material = null!;
    private bool _disposed;

    public Presenter(IEventBus eventBus, ILogger logger, PiecePrefabRegistry prefabs, Transform piecesRoot)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _prefabs = prefabs ?? throw new ArgumentNullException(nameof(prefabs));
        _piecesRoot = piecesRoot ?? throw new ArgumentNullException(nameof(piecesRoot));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _eventBus.Subscribe<PieceDeployedEvent<Square>>(OnPieceDeployed);
        _eventBus.Subscribe<PieceMovedEvent<Square>>(OnPieceMoved);
        _eventBus.Subscribe<PieceDiedEvent>(OnPieceDied);
        _eventBus.Subscribe<PieceSelectedEvent<Square>>(OnPieceSelected);

        _logger.Log("Presenter инициализирован");
    }

    public void SetMaterial(Material material)
    {
        _material = material != null ? material : throw new ArgumentNullException(nameof(material));
    }

    private void OnPieceDeployed(PieceDeployedEvent<Square> e)
    {
        var piece = e.Piece;
        var position = e.Position;

        if (_views.ContainsKey(piece.IndexInHouse))
        {
            _logger.LogWarning($"Визуал для {piece} уже существует, пропускаю создание");
            return;
        }

        if (_material == null)
            throw new InvalidOperationException("Presenter.SetMaterial не был вызван");

        var prefab = _prefabs.GetPrefab(piece.Name);
        var go = UnityEngine.Object.Instantiate(prefab, _piecesRoot);
        go.name = $"{position.X}_{position.Y}_{position.Z}_{piece.Name}";
        go.transform.localPosition = GridToWorld(position);

        var info = go.GetOrAddComponent<InfoOfPiece>();
        info.Initialize(piece.Name, piece.Owner, piece.IndexInHouse);
        info.PositionInGrid = new Vector3Int(position.X, position.Y, position.Z);

        PieceColorizer.ApplyInitialColor(go, info, _material, piece.Owner, _teamCount: GetMaxSide() + 1, _subteamCount: GetMaxSubteam() + 1);

        _views[piece.IndexInHouse] = go;
        _pieces[piece.IndexInHouse] = piece;

        _logger.LogDebug($"Создан визуал для {piece} в {position}");
    }
    private void OnPieceMoved(PieceMovedEvent<Square> e)
    {
        if (!_views.TryGetValue(e.Piece.IndexInHouse, out var go))
        {
            _logger.LogWarning($"Визуал для перемещённой {e.Piece} не найден");
            return;
        }

        go.transform.localPosition = GridToWorld(e.ToPosition);

        if (go.TryGetComponent<InfoOfPiece>(out var info))
            info.PositionInGrid = new Vector3Int(e.ToPosition.X, e.ToPosition.Y, e.ToPosition.Z);

        _logger.LogDebug($"{e.Piece.Name}: {e.FromPosition} → {e.ToPosition}");
    }
    private void OnPieceDied(PieceDiedEvent e)
    {
        if (!_views.TryGetValue(e.Piece.IndexInHouse, out var go))
            return;

        if (go != null)
            UnityEngine.Object.Destroy(go);

        _views.Remove(e.Piece.IndexInHouse);
        _pieces.Remove(e.Piece.IndexInHouse);

        _logger.LogDebug($"Визуал {e.Piece} уничтожен");
    }
    private void OnPieceSelected(PieceSelectedEvent<Square> e)
    {
        foreach (var (index, view) in _views)
        {
            if (view == null) continue;
            if (!view.TryGetComponent<InfoOfPiece>(out var info)) continue;

            bool isSelected = index == e.Piece.IndexInHouse;
            PieceColorizer.SetVisualState(info, isSelected, highlighted: false, hovered: false);
        }
    }

    private static UnityEngine.Vector3 GridToWorld(Square position)
    {
        return new UnityEngine.Vector3(
            position.X * _cellSize,
            position.Z * _cellSize,
            position.Y * _cellSize);
    }

    private int GetMaxSide()
    {
        int maxSide = 0;
        foreach (var piece in _pieces.Values)
        {
            if (piece.Owner.IndexOfSide > maxSide)
                maxSide = piece.Owner.IndexOfSide;
        }
        return maxSide;
    }

    private int GetMaxSubteam()
    {
        int maxSub = 0;
        foreach (var piece in _pieces.Values)
        {
            if (piece.Owner.IndexOfPlayerOnSide > maxSub)
                maxSub = piece.Owner.IndexOfPlayerOnSide;
        }
        return maxSub;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _eventBus.Unsubscribe<PieceDeployedEvent<Square>>(OnPieceDeployed);
        _eventBus.Unsubscribe<PieceMovedEvent<Square>>(OnPieceMoved);
        _eventBus.Unsubscribe<PieceDiedEvent>(OnPieceDied);
        _eventBus.Unsubscribe<PieceSelectedEvent<Square>>(OnPieceSelected);

        foreach (var go in _views.Values)
        {
            if (go != null) UnityEngine.Object.Destroy(go);
        }
        _views.Clear();
        _pieces.Clear();

        _logger.Log("Presenter уничтожен");
    }
}