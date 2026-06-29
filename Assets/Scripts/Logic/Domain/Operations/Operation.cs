#nullable enable

public abstract record Operation : IOperation
{
    protected Operation(int value) => Value = value;
    public int Value { get; }
    public abstract void ApplyTo(IProperty property);
}