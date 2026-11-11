#nullable enable

using System;

public sealed class AttackDamageProperty : Property, IPropertyWithNormal, ISettableNormalProperty
{
    private int _normalValue;

    public AttackDamageProperty(int startValue) : base(startValue)
    {
        _normalValue = Math.Max(0, startValue);
    }

    public int NormalValue => _normalValue;

    public override void Apply(IOperation operation)
    {
        // AttackDamage меняется только через IAttackDamageModifier (см. фазу 7),
        // не через IOperation. Этот метод — no-op для единообразия с Property.
    }

    public void SetNormal(int value) => _normalValue = Math.Max(0, value);
}