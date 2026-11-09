#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class FireballModifier : ITargetedModifier
{
    private const int _radius = 3;

    private int _maxCharges;
    private int _currentCharges;

    public string ModifierName => "Fireball";
    public int MaxDuration => -1;
    public int CurrentDuration => -1;
    public int MaxCharges => _maxCharges;
    public int CurrentCharges => _currentCharges;
    public bool TriggersStartedAction => true;

    public void Attach(ModifierContext context)
    {
        _maxCharges = context.InitialCharges;
        _currentCharges = context.InitialCharges;
    }

    public void Detach() { }

    public bool TryConsumeCharge()
    {
        if (_currentCharges < 0) return true;
        if (_currentCharges == 0) return false;
        _currentCharges--;
        return true;
    }

    public void RestoreCharges(int charges)
    {
        if (_maxCharges < 0) { _currentCharges = -1; return; }
        if (charges < 0 || charges > _maxCharges)
            throw new ArgumentOutOfRangeException(nameof(charges), $"Charges must be in [0..{_maxCharges}].");
        _currentCharges = charges;
    }

    public void RestoreDuration(int duration) { }

    public bool CanActivate(ModifierActivationContext context) => !context.Owner.IsDead;

    public void Activate(ModifierActivationContext context)
    {
        throw new InvalidOperationException(
            "FireballModifier must be activated via Activate(context, targetCell).");
    }

    public IReadOnlyList<Vector3Int> GetTargetCells(PieceModel owner, Vector3Int gridSize)
    {
        var result = new List<Vector3Int>();
        var center = owner.PositionInGrid;
        for (int dx = -_radius; dx <= _radius; dx++)
            for (int dy = -_radius; dy <= _radius; dy++)
                for (int dz = -_radius; dz <= _radius; dz++)
                {
                    if (dx == 0 && dy == 0 && dz == 0) continue;
                    int cheb = Math.Max(Math.Abs(dx), Math.Max(Math.Abs(dy), Math.Abs(dz)));
                    if (cheb > _radius) continue;
                    var cell = center + new Vector3Int(dx, dy, dz);
                    if (cell.x < 0 || cell.x >= gridSize.x) continue;
                    if (cell.y < 0 || cell.y >= gridSize.y) continue;
                    if (cell.z < 0 || cell.z >= gridSize.z) continue;
                    result.Add(cell);
                }
        return result;
    }

    public void Activate(ModifierActivationContext context, Vector3Int targetCell)
    {
        var target = context.Registry.GetAt(targetCell);
        if (target != null)
        {
            int dmg = context.Host.GetEffectiveAttackDamage(context.Owner);
            target.TakeDamage(dmg);
        }
        context.EventBus.PublishAttacked(context.Owner, targetCell, target, isMelee: false);
    }
}