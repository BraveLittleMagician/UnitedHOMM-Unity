
#nullable enable
public sealed record FacingChangedEvent(IPiece Piece, FacingDirection OldFacing, FacingDirection NewFacing);