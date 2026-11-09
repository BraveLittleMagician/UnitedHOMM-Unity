
#nullable enable
using System;

public sealed class EnPassantModifier : IModifier
{
    private int _maxCharges;
    private int _currentCharges;

    public string ModifierName => "EnPassant";
    public int MaxDuration => -1;
    public int CurrentDuration => -1;
    public int MaxCharges => _maxCharges;
    public int CurrentCharges => _currentCharges;

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
}