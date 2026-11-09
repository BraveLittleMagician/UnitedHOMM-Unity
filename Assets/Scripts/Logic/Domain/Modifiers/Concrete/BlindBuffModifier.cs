#nullable enable
using System;

public sealed class BlindBuffModifier : IModifier, IAttackBlocker
{
    private int _currentDuration;

    public string ModifierName => "BlindBuff";
    public int MaxDuration => 2;
    public int CurrentDuration => _currentDuration;
    public int MaxCharges => -1;
    public int CurrentCharges => -1;

    public void Attach(ModifierContext context)
    {
        _currentDuration = context.InitialDuration;
    }

    public void Detach() { }

    public bool TryConsumeCharge() => false;
    public void RestoreCharges(int charges) { }

    public void RestoreDuration(int duration)
    {
        if (duration < 0 || duration > MaxDuration)
            throw new ArgumentOutOfRangeException(nameof(duration), $"Duration must be in [0..{MaxDuration}].");
        _currentDuration = duration;
    }

    public bool CanAttack(PieceModel piece) => false;
}