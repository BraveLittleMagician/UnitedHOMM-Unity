#nullable enable

using System;

public sealed class AttackChargesProperty : Property, IPropertyWithNormal, ISettableNormalProperty
{
    private int _normalValue;

    public AttackChargesProperty(int startValue) : base(startValue)
    {
        _normalValue = Math.Max(0, startValue);
    }

    public int NormalValue => _normalValue;

    public override void Apply(IOperation operation)
    {
        // Заряды меняются явно через TryConsume/Restore/SetNormal,
        // не через IOperation. Этот метод — no-op.
    }

    public void SetNormal(int value) => _normalValue = Math.Max(0, value);

    public bool TryConsume()
    {
        if (Value <= 0) return false;
        Value--;
        return true;
    }

    public void Restore(int value)
    {
        if (value < 0 || value > _normalValue)
            throw new ArgumentOutOfRangeException(nameof(value),
                $"AttackCharges must be in [0..{_normalValue}].");
        Value = value;
    }
}