#nullable enable

public sealed record PieceAttackedEvent(IPiece Attacker, IPiece Target, IRoom Room, AttackKind Kind);