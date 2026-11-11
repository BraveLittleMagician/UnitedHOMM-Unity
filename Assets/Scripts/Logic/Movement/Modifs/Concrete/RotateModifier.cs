#nullable enable
using System;

public sealed class RotateModifier : IActiveModifier
{
    private readonly bool _isRight;
    private int _maxCharges;
    private int _currentCharges;

    public RotateModifier(bool isRight) => _isRight = isRight;

    public string ModifierName => _isRight ? "RotateRight" : "RotateLeft";
    public int MaxDuration => -1;
    public int CurrentDuration => -1;
    public int MaxCharges => _maxCharges;
    public int CurrentCharges => _currentCharges;
    public bool TriggersStartedAction => false;

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

    public bool CanActivate(ModifierActivationContext context)
    {
        if (context.Owner.IsDead) return false;
        if (context.Owner.TotalRotationDegreesThisTurn + 90 > 180) return false;
        if (!IsInPlaneXY(context.Owner.Facing)) return false;
        return true;
    }

    public void Activate(ModifierActivationContext context)
    {
        var newFacing = Rotate(context.Owner.Facing, _isRight);
        context.Owner.RestoreFacing(newFacing);
        context.Owner.AddRotationDegreesThisTurn(90);
    }

    private static FacingDirection Rotate(FacingDirection current, bool isRight) => (current, isRight) switch
    {
        (FacingDirection.PlusY, true) => FacingDirection.PlusX,
        (FacingDirection.PlusX, true) => FacingDirection.MinusY,
        (FacingDirection.MinusY, true) => FacingDirection.MinusX,
        (FacingDirection.MinusX, true) => FacingDirection.PlusY,
        (FacingDirection.PlusY, false) => FacingDirection.MinusX,
        (FacingDirection.MinusX, false) => FacingDirection.MinusY,
        (FacingDirection.MinusY, false) => FacingDirection.PlusX,
        (FacingDirection.PlusX, false) => FacingDirection.PlusY,
        _ => current,
    };

    private static bool IsInPlaneXY(FacingDirection facing) =>
        facing == FacingDirection.PlusX || facing == FacingDirection.MinusX ||
        facing == FacingDirection.PlusY || facing == FacingDirection.MinusY;
}