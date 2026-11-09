#nullable enable
using System;
using UnityEngine;

public sealed class CounterAttackModifier : IModifier
{
    private PieceModel _owner = null!;
    private PieceEventBus _eventBus = null!;
    private int _maxCharges;
    private int _currentCharges;

    public string ModifierName => "CounterAttack";
    public int MaxDuration => -1;
    public int CurrentDuration => -1;
    public int MaxCharges => _maxCharges;
    public int CurrentCharges => _currentCharges;

    public void Attach(ModifierContext context)
    {
        _owner = context.Owner;
        _eventBus = context.EventBus;
        _maxCharges = context.InitialCharges;
        _currentCharges = context.InitialCharges;
        _eventBus.PieceAttacked += OnPieceAttacked;
    }

    public void Detach()
    {
        if (_eventBus != null)
            _eventBus.PieceAttacked -= OnPieceAttacked;
    }

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

    private void OnPieceAttacked(PieceModel attacker, Vector3Int cell, PieceModel? targetBefore, bool isMelee)
    {
        if (!isMelee) return;
        if (_eventBus.PieceAttackedDepth > 1) return;
        if (targetBefore != _owner) return;
        if (_owner.IsDead) return;
        if (attacker == _owner) return;
        if (attacker.Player == _owner.Player) return;
        if (!TryConsumeCharge()) return;
        attacker.TakeDamage(1);
        _eventBus.PublishAttacked(_owner, attacker.PositionInGrid, attacker, isMelee: true);
    }
}