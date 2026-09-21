#nullable enable

using System;

public sealed class Health : Property, INumericProperty, ISettableProperty, IRecoverableProperty, IResettableProperty, ISettableNormalProperty
{
    public int NormalValue { get; private set; }

    public Health(int startValue) : base(startValue)
    {
        SetNormal(startValue);
    }

    public override void Apply(IOperation operation) => operation.ApplyTo(this);

    public void Add(int amount) => Value += Math.Abs(amount);
    public void Subtract(int amount) => Value -= Math.Abs(amount);
    public void Recover(int amount)
    {
        if (Value >= NormalValue) return;
        int resultValue = amount + Value;
        if (resultValue > NormalValue) amount = NormalValue - Value;
        Add(amount);
    }
    public void Reset() => Value = NormalValue;
    public void Set(int value) => Value = Math.Max(0, value);
    public void SetNormal(int value) => NormalValue = Math.Max(value, 1);

    public void TakeBoost(int amount) => Add(amount);
    public void TakeDamage(int amount) => Subtract(amount);
    public void TakeHeal(int amount) => Recover(amount);
    public void TakeReset() => Reset();
}