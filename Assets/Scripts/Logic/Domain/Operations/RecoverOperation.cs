#nullable enable

public record RecoverOperation(int Value) : Operation(Value)
{
    public override void ApplyTo(IProperty property)
    {
        if (property is IRecoverableProperty recoverable)
            recoverable.Recover(Value);
    }
}