#nullable enable

public record PieceInitiativeEventBase(IPiece Piece, IRoom Room);
public record PieceInitiativeEvent<TPos>(IPiece Piece, TPos Position, IRoom Room) : PieceInitiativeEventBase(Piece, Room) where TPos : struct;