#nullable enable
using System;

public sealed class CounterAttackModifier : IModifier
{
    private IPiece _owner = null!;
    private IEventBus _eventBus = null!;
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
        _eventBus.Subscribe<PieceAttackedEvent>(OnPieceAttacked);
    }

    public void Detach()
    {
        if (_eventBus != null)
            _eventBus.Unsubscribe<PieceAttackedEvent>(OnPieceAttacked);
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

    private void OnPieceAttacked(PieceAttackedEvent e)
    {
        if (e.Depth > 1) return;
        if (e.Kind != AttackKind.Melee) return;
        if (!ReferenceEquals(e.Target, _owner)) return;
        if (_owner.IsDead) return;
        if (ReferenceEquals(e.Attacker, _owner)) return;
        if (e.Attacker.Owner.Equals(_owner.Owner)) return;
        if (!TryConsumeCharge()) return;

        e.Attacker.TakeDamage(1);
        _eventBus.Publish(new PieceAttackedEvent(
            _owner, e.Attacker, e.Room, AttackKind.Melee, e.Depth + 1));
    }
}