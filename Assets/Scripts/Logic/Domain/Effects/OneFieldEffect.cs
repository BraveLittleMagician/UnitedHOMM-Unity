#nullable enable

public abstract class OneFieldEffect : IEffect
{
    protected readonly Operation _operation;

    public OneFieldEffect(Operation operation) => _operation = operation;

    public void Execute(IPiece piece, IRoom context) { }
    public void ExecuteWithoutRoom(IPiece piece) => piece.ApplyOperation(_operation);
}