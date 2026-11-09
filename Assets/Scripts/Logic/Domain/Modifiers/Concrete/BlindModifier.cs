#nullable enable
using System;

public sealed class BlindModifier : IActiveModifier
{
    private int _maxCharges;
    private int _currentCharges;

    public string ModifierName => "Blind";
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
        if (_maxCharges < 0)
        {
            _currentCharges = -1;
            return;
        }
        if (charges < 0 || charges > _maxCharges)
            throw new ArgumentOutOfRangeException(nameof(charges), $"Charges must be in [0..{_maxCharges}].");
        _currentCharges = charges;
    }

    public void RestoreDuration(int duration) { }

    public bool CanActivate(ModifierActivationContext context)
    {
        return !context.Owner.IsDead;
    }

    public void Activate(ModifierActivationContext context)
    {
        var buff = new BlindBuffModifier();
        var ctx = new ModifierContext(
            context.Owner,
            context.EventBus,
            context.Host,
            selfRemove: m => context.Host.RemoveModifier(context.Owner.PieceId, m),
            initialDuration: 2,
            initialCharges: -1);
        context.Host.AddModifier(context.Owner.PieceId, buff, ctx);
    }
}