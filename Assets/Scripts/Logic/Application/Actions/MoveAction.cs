#nullable enable

using System;

public sealed class MoveAction : IGameAction
{
    private readonly IPiece _piece;
    private readonly IPath<Square> _path;
    private readonly IFlow _flow;

    public MoveAction(IPiece piece, IPath<Square> path, IFlow flow)
    {
        _piece = piece ?? throw new ArgumentNullException(nameof(piece));
        _path = path ?? throw new ArgumentNullException(nameof(path));
        _flow = flow ?? throw new ArgumentNullException(nameof(flow));
    }

    public IdOfPlayer Owner => _piece.Owner;
    public IPiece? FocusPiece => _piece;
    public IPath<Square> Path => _path;

    public bool CanExecute()
    {
        if (_piece.Health <= 0) return false;
        if (_piece.HasMovedThisTurn) return false;
        if (_path.Positions.Count < 2) return false;
        return true;
    }

    public void Execute()
    {
        var result = _flow.MovePiece<Board, Square>(_piece.Owner, _path);
        if (!result.IsSuccess)
            throw new InvalidOperationException(
                $"MoveAction.Execute failed: {result.Error}");

        _piece.MarkMovedThisTurn();
    }
}