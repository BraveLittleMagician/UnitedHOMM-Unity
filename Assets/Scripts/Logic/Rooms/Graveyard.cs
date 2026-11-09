#nullable enable

using System;
using System.Collections.Generic;

public sealed class Graveyard
{
    private readonly List<PieceModel> _deadInOrder = new();
    private readonly PieceRegistry _registry;

    public Graveyard(PieceRegistry registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _registry.ModelDied += OnModelDied;
        _registry.ModelRevived += OnModelRevived;
        _registry.ModelUnregistered += OnModelUnregistered;
    }

    public IReadOnlyList<PieceModel> AllDead => _deadInOrder;

    public IReadOnlyList<PieceModel> GetDeadFor(IdOfPlayer player)
    {
        if (_deadInOrder.Count == 0) return Array.Empty<PieceModel>();
        var result = new List<PieceModel>();
        for (int i = _deadInOrder.Count - 1; i >= 0; i--)
            if (_deadInOrder[i].Player == player)
                result.Add(_deadInOrder[i]);
        return result;
    }

    private void OnModelDied(PieceModel model)
    {
        if (model == null) return;
        if (_deadInOrder.Contains(model)) return;
        _deadInOrder.Add(model);
    }

    private void OnModelRevived(PieceModel model)
    {
        if (model == null) return;
        _deadInOrder.Remove(model);
    }

    private void OnModelUnregistered(PieceModel model)
    {
        if (model == null) return;
        _deadInOrder.Remove(model);
    }
}