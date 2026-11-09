#nullable enable
using System;
using UnityEngine;

public sealed class PieceEventBus
{
    private readonly PieceRegistry _registry;

    public event Action<PieceModel, Activity>? PiecePlaced;
    public event Action<PieceModel, Activity>? PieceDied;
    public event Action<PieceModel, Activity>? PieceRevived;
    public event Action<PieceModel, Activity>? PieceStartedAction;
    public event Action<PieceModel, Vector3Int, Vector3Int>? PieceMoved;
    public event Action<PieceModel, Vector3Int, PieceModel?, bool>? PieceAttacked;

    public PieceEventBus(PieceRegistry registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _registry.ModelDied += OnModelDied;
        _registry.ModelRevived += OnModelRevived;
        _registry.ModelStartedAction += OnModelStartedAction;
    }

    public int PieceAttackedDepth { get; private set; }

    public void PublishPlaced(PieceModel model) => PiecePlaced?.Invoke(model, EmptyActivity.Instance);
    public void PublishMoved(PieceModel model, Vector3Int from, Vector3Int to) => PieceMoved?.Invoke(model, from, to);
    public void PublishAttacked(PieceModel attacker, Vector3Int cell, PieceModel? targetBefore, bool isMelee)
    {
        PieceAttackedDepth++;
        try
        {
            PieceAttacked?.Invoke(attacker, cell, targetBefore, isMelee);
        }
        finally
        {
            PieceAttackedDepth--;
        }
    }

    private void OnModelDied(PieceModel model) => PieceDied?.Invoke(model, EmptyActivity.Instance);
    private void OnModelRevived(PieceModel model) => PieceRevived?.Invoke(model, EmptyActivity.Instance);
    private void OnModelStartedAction(PieceModel model, Activity activity) => PieceStartedAction?.Invoke(model, activity);
}