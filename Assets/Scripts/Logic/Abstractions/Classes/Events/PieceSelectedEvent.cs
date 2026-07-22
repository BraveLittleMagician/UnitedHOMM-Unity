#nullable enable

public record PieceSelectedEvent<TPos>(IPiece Piece, TPos position) where TPos : struct;