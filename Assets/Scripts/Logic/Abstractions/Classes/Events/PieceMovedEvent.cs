#nullable enable

public sealed record PieceMovedEvent<TPos>(IPiece Piece, TPos FromPosition, TPos ToPosition, IRoom Room) where TPos : struct;