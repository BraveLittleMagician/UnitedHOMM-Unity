#nullable enable

public interface IEffect
{
    void Execute(IPiece piece, IRoom iroom);
    void ExecuteWithoutRoom(IPiece piece);
}