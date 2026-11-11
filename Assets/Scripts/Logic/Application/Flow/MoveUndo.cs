#nullable enable

using System;

internal sealed class MoveUndo : IUndoableOperation
{
    private readonly IPiece _piece;
    private readonly IRoom _room;
    private readonly IPath<Square> _path;
    private readonly Square _fromPos;
    private readonly Square _toPos;
    private readonly PieceActionFlags _flagsBefore;
    private readonly PieceActionFlags _flagsAfter;
    private readonly IEventBus _eventBus;

    public MoveUndo(
        IPiece piece,
        IRoom room,
        IPath<Square> path,
        Square fromPos,
        Square toPos,
        PieceActionFlags flagsBefore,
        PieceActionFlags flagsAfter,
        IEventBus eventBus)
    {
        _piece = piece ?? throw new ArgumentNullException(nameof(piece));
        _room = room ?? throw new ArgumentNullException(nameof(room));
        _path = path ?? throw new ArgumentNullException(nameof(path));
        _fromPos = fromPos;
        _toPos = toPos;
        _flagsBefore = flagsBefore;
        _flagsAfter = flagsAfter;
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
    }

    public bool Undo(Flow flow)
    {
        var reversePath = BuildReversePath();
        if (!_room.Displace(reversePath, _piece.Owner, out _)) return false;

        _piece.RestoreMovedThisTurn(_flagsBefore.HasMovedThisTurn);
        _piece.RestoreFirstActionOfGameTaken(_flagsBefore.HasTakenFirstActionOfGame);

        _eventBus.Publish(new PieceMovedEvent<Square>(_piece, _toPos, _fromPos, _room));
        return true;
    }

    public bool Redo(Flow flow)
    {
        if (!_room.Displace(_path, _piece.Owner, out _)) return false;

        _piece.RestoreMovedThisTurn(_flagsAfter.HasMovedThisTurn);
        _piece.RestoreFirstActionOfGameTaken(_flagsAfter.HasTakenFirstActionOfGame);

        _eventBus.Publish(new PieceMovedEvent<Square>(_piece, _fromPos, _toPos, _room));
        return true;
    }

    private IPath<Square> BuildReversePath()
    {
        var positions = new Square[_path.Positions.Count];
        for (int i = 0; i < _path.Positions.Count; i++)
            positions[i] = _path.Positions[_path.Positions.Count - 1 - i];
        return new PathSequence<Square>(positions);
    }
}