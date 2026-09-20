#nullable enable

public abstract record PieceSelectedEventBase(IPiece Piece);
public record PieceSelectedEvent<TPos>(IPiece Piece, TPos Position)
    : PieceSelectedEventBase(Piece) where TPos : struct;