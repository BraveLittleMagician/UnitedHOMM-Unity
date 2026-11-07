#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

public sealed class PieceRegistry
{
    private readonly Dictionary<Vector3Int, PieceModel> _byCell = new();
    private readonly Dictionary<long, PieceModel> _byId = new();
    private readonly List<PieceModel> _allModels = new();

    public IReadOnlyList<PieceModel> AllModels => _allModels;
    public IEnumerable<PieceModel> AllLiveModels
    {
        get
        {
            foreach (var model in _allModels)
                if (!model.IsDead)
                    yield return model;
        }
    }

    private void OnModelDeadStateChanged(PieceModel model, bool isDead)
    {
        if (isDead)
        {
            if (_byCell.TryGetValue(model.PositionInGrid, out var existing) && existing == model)
                _byCell.Remove(model.PositionInGrid);
            ModelDied?.Invoke(model);
        }
        else
        {
            if (_byCell.TryGetValue(model.PositionInGrid, out var existing) && existing != model)
                throw new InvalidOperationException(
                    $"Cannot revive '{model.Name}#{model.PieceId}': cell {model.PositionInGrid} " +
                    $"already occupied by '{existing.Name}#{existing.PieceId}'.");
            _byCell[model.PositionInGrid] = model;
            ModelRevived?.Invoke(model);
        }
    }
    private void OnModelStartedFirstAction(PieceModel model, Activity activity) => ModelStartedAction?.Invoke(model, activity);

    public void Register(PieceModel model)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));
        if (_byId.ContainsKey(model.PieceId))
            throw new InvalidOperationException(
                $"Model '{model.Name}#{model.PieceId}' already registered.");

        var cell = model.PositionInGrid;
        if (!model.IsDead && _byCell.TryGetValue(cell, out var existing) && existing != model)
            throw new InvalidOperationException(
                $"Клетка {cell} уже занята фигурой '{existing.Name}#{existing.PieceId}' " +
                $"(пытаемся поставить '{model.Name}#{model.PieceId}').");

        _allModels.Add(model);
        _byId[model.PieceId] = model;
        if (!model.IsDead)
            _byCell[cell] = model;

        model.DeadStateChanged += OnModelDeadStateChanged;
        model.FirstActionOfGameTaken += OnModelStartedFirstAction;

        ModelRegistered?.Invoke(model);
    }
    public void Unregister(PieceModel model)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));
        if (!_byId.ContainsKey(model.PieceId))
            throw new InvalidOperationException(
                $"PieceRegistry.Unregister: модель '{model.Name}#{model.PieceId}' не зарегистрирована.");

        _allModels.Remove(model);
        _byId.Remove(model.PieceId);

        if (_byCell.TryGetValue(model.PositionInGrid, out var atCell) && atCell == model)
            _byCell.Remove(model.PositionInGrid);

        model.DeadStateChanged -= OnModelDeadStateChanged;
        model.FirstActionOfGameTaken -= OnModelStartedFirstAction;

        ModelUnregistered?.Invoke(model);
    }
    public void Move(PieceModel model, Vector3Int newCell)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));
        var oldCell = model.PositionInGrid;
        if (oldCell == newCell) return;

        if (!model.IsDead)
        {
            if (!_byCell.TryGetValue(oldCell, out var atOld) || atOld != model)
                throw new InvalidOperationException(
                    $"PieceRegistry.Move: '{model.Name}#{model.PieceId}' не найдена на клетке {oldCell}.");
            if (_byCell.TryGetValue(newCell, out var atNew) && atNew != model)
                throw new InvalidOperationException(
                    $"PieceRegistry.Move: клетка {newCell} уже занята '{atNew.Name}#{atNew.PieceId}'.");

            _byCell.Remove(oldCell);
            _byCell[newCell] = model;
        }
        model.PositionInGrid = newCell;
    }
    public void Replace(PieceModel oldModel, PieceModel newModel)
    {
        if (oldModel == null) throw new ArgumentNullException(nameof(oldModel));
        if (newModel == null) throw new ArgumentNullException(nameof(newModel));
        if (oldModel.PieceId != newModel.PieceId)
            throw new InvalidOperationException(
                $"PieceRegistry.Replace: PieceId mismatch ({oldModel.PieceId} != {newModel.PieceId}).");
        if (!_byId.TryGetValue(oldModel.PieceId, out var existing) || existing != oldModel)
            throw new InvalidOperationException(
                $"PieceRegistry.Replace: '{oldModel.Name}#{oldModel.PieceId}' is not registered.");
        if (oldModel.Player != newModel.Player)
            throw new InvalidOperationException(
                $"PieceRegistry.Replace: cannot change Player ({oldModel.Player} != {newModel.Player}).");
        if (oldModel.PositionInGrid != newModel.PositionInGrid)
            throw new InvalidOperationException(
                $"PieceRegistry.Replace: use Move for position changes; Replace keeps cell stable.");

        var cell = oldModel.PositionInGrid;
        var wasLive = !oldModel.IsDead;
        var willBeLive = !newModel.IsDead;

        oldModel.DeadStateChanged -= OnModelDeadStateChanged;
        oldModel.FirstActionOfGameTaken -= OnModelStartedFirstAction;
        _allModels.Remove(oldModel);
        _byId[newModel.PieceId] = newModel;
        _allModels.Add(newModel);

        if (wasLive && _byCell.TryGetValue(cell, out var atCell) && atCell == oldModel)
            _byCell.Remove(cell);
        if (willBeLive)
        {
            if (_byCell.TryGetValue(cell, out var atNewCell) && atNewCell != newModel)
                throw new InvalidOperationException(
                    $"PieceRegistry.Replace: cell {cell} already occupied by '{atNewCell.Name}#{atNewCell.PieceId}'.");
            _byCell[cell] = newModel;
        }

        newModel.DeadStateChanged += OnModelDeadStateChanged;
        newModel.FirstActionOfGameTaken += OnModelStartedFirstAction;
    }
    public void Clear()
    {
        foreach (var model in _allModels)
        {
            model.DeadStateChanged -= OnModelDeadStateChanged;
            model.FirstActionOfGameTaken -= OnModelStartedFirstAction;
        }
        _allModels.Clear();
        _byId.Clear();
        _byCell.Clear();
    }
    public bool Remove(PieceModel model)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));
        if (!_byId.ContainsKey(model.PieceId)) return false;
        Unregister(model);
        return true;
    }
    public bool RemoveAt(Vector3Int cell)
    {
        if (!_byCell.TryGetValue(cell, out var model)) return false;
        return Remove(model);
    }
    public bool TryGetAt(Vector3Int cell, [NotNullWhen(true)] out PieceModel? model)
    {
        if (_byCell.TryGetValue(cell, out model) && model != null) return true;
        model = null;
        return false;
    }
    public bool IsOccupied(Vector3Int cell) => _byCell.ContainsKey(cell);
    public bool IsOccupiedByPlayer(Vector3Int cell, IdOfPlayer player) => _byCell.TryGetValue(cell, out var m) && m.Player == player;
    public PieceModel? GetAt(Vector3Int cell) => TryGetAt(cell, out var m) ? m : null;
    public PieceModel? GetByPieceId(long pieceId) => _byId.TryGetValue(pieceId, out var m) ? m : null;

    public event Action<PieceModel>? ModelRegistered;
    public event Action<PieceModel>? ModelUnregistered;
    public event Action<PieceModel>? ModelDied;
    public event Action<PieceModel>? ModelRevived;
    public event Action<PieceModel, Activity>? ModelStartedAction;
}