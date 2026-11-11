
#nullable enable

public sealed record PendingPromotion(IPiece Piece, PromotionResult Result) : PendingModal;