#nullable enable

public record AddOperation(int Value) : Operation(Value)
{
    public override void ApplyTo(IProperty property)
    {
        if (property is INumericProperty numeric)
            numeric.Add(Value);
    }
}