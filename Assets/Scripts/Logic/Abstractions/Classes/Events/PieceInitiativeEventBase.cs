#nullable enable

public abstract record PieceInitiativeEventBase(IPiece Piece, IRoom Room);
public sealed record PieceInitiativeEvent<TPos>(IPiece Piece, TPos Position, IRoom Room) : PieceInitiativeEventBase(Piece, Room) where TPos : struct;