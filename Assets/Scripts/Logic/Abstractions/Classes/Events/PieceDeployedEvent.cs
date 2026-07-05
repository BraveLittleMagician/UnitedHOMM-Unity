#nullable enable

public abstract record PieceDeployedEventBase(IPiece Piece, IRoom Room);
public record PieceDeployedEvent<TPos>(IPiece Piece, TPos Position, IRoom Room) : PieceDeployedEventBase(Piece, Room) where TPos : struct;