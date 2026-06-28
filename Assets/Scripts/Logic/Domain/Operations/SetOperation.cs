#nullable enable


public record SetOperation(int Value) : Operation(Value)
{
    public override void ApplyTo(IProperty property)
    {
        if (property is ISettableProperty settable)
            settable.Set(Value);
    }
}
