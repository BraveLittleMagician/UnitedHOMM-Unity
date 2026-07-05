#nullable enable

public record PieceDeployedEvent<TPos>(IPiece Piece, TPos Position, IRoom Room) where TPos : struct;