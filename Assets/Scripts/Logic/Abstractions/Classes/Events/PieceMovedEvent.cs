#nullable enable

public abstract record PieceMovedEventBase(IPiece Piece, object FromPositionObject, object ToPositionObject, IRoom Room);
public sealed record PieceMovedEvent<TPos>(IPiece Piece, TPos FromPosition, TPos ToPosition, IRoom Room)
    : PieceMovedEventBase(Piece, FromPosition, ToPosition, Room) where TPos : struct;