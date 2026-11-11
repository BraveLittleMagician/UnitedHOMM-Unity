#nullable enable
using System;

public sealed class Graveyard : RoomT<int>, IIntPositionRoom
{
    public IdOfPlayer Owner { get; }

    public Graveyard(IdOfPlayer owner, Seats seats, IEventBus eventBus, ILogger logger)
        : base(seats, eventBus, logger)
    {
        Owner = owner;
        eventBus.Subscribe<PieceDiedEvent>(OnPieceDied);
        eventBus.Subscribe<PieceRevivedEvent>(OnPieceRevived);
    }

    protected override bool AllowDeadPiece => true;
    protected override bool IncludeDeadInLookup => true;
    protected override bool FilterDeadInLookup => false;

    private void OnPieceDied(PieceDiedEvent e)
    {
        if (!e.Piece.Owner.Equals(Owner)) return;
        if (_pieces.ContainsValue(e.Piece)) return;
        if (!Add(e.Piece, NextPosition(), false, out var err))
            Logger.LogWarning($"Graveyard({Owner}) failed to add {e.Piece}: {err}");
    }

    private void OnPieceRevived(PieceRevivedEvent e)
    {
        if (!e.Piece.Owner.Equals(Owner)) return;
        Remove(e.Piece.IndexInHouse);
    }

    private int NextPosition()
    {
        int max = -1;
        foreach (var k in _pieces.Keys)
            if (k > max) max = k;
        return max + 1;
    }

    protected override bool ValidateAdd(IPiece piece, int position, out string error)
    {
        if (!piece.Owner.Equals(Owner)) { error = $"Piece {piece} does not belong to {Owner}"; return false; }
        if (!piece.IsDead) { error = $"Piece {piece} is not dead"; return false; }
        if (position < 0) { error = "Position must be non-negative"; return false; }
        error = "";
        return true;
    }

    protected override bool ValidateDisplace(IPath<int> path, IPiece piece, out string error)
    {
        error = "Graveyard does not support displacement";
        return false;
    }

    public override void Dispose()
    {
        EventBus.Unsubscribe<PieceDiedEvent>(OnPieceDied);
        EventBus.Unsubscribe<PieceRevivedEvent>(OnPieceRevived);
        base.Dispose();
    }
}