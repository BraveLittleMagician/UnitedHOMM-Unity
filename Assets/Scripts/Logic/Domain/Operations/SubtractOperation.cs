#nullable enable


public record SubtractOperation(int Value) : Operation(Value)
{
    public override void ApplyTo(IProperty property)
    {
        if (property is INumericProperty numeric)
            numeric.Subtract(Value);
    }
}