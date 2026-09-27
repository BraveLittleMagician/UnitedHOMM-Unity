#nullable enable

using System;

public sealed class Health : Property, IPropertyWithNormal, ISettableNormalProperty, INumericProperty, ISettableProperty, IRecoverableProperty, IResettableProperty
{
    private int _normalValue;

    public Health(int startValue) : base(startValue)
    {
        SetNormal(startValue);
    }

    public int NormalValue
    {
        get => _normalValue;
        private set
        {
            int oldValue = _normalValue;
            _normalValue = Math.Max(1, value);
            if (oldValue != _normalValue)
                OnNormalChanged(oldValue);
        }
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
    public void Set(int value) => Value = value;
    public void SetNormal(int value) => NormalValue = value;

    private void OnNormalChanged(int oldValue) => NormalChanged?.Invoke(this, oldValue);

    public override void Dispose()
    {
        base.Dispose();
        NormalChanged = null;
    }

    public event Action<Property, int>? NormalChanged;
}