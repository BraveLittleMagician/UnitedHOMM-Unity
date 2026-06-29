#nullable enable

public record PieceAttackedEvent(IPiece Attacker, IPiece Target, IRoom Room, string AttackType);