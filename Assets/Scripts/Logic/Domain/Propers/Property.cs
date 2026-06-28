#nullable enable


using System;

public abstract class Property : IProperty
{
    private int _value;

    public Property(int startValue)
    {
        _value = Math.Max(0, startValue);
    }
    
    public int Value
    {
        get => _value;
        protected set
        {
            int oldValue = _value;
            _value = Math.Max(0, value);
            if (oldValue != _value)
                OnChanged(oldValue);
        }
    }
    
    protected virtual void OnChanged(int oldValue) => Changed?.Invoke(this, oldValue);

    public abstract void Apply(IOperation operation);
    public virtual void Dispose()
    {
        Changed = null;
        GC.SuppressFinalize(this);
    }
    public override string ToString() => $"{GetType().Name}: {Value}";

    public event Action<Property, int>? Changed;
}