#nullable enable

public interface IGameAction
{
    IdOfPlayer Owner { get; }
    IPiece? FocusPiece { get; }
    bool CanExecute();
    void Execute();
}