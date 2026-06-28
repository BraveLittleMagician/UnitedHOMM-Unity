#nullable enable


using System;

public record ModifiersUpdatedEvent(IPiece Piece, bool IsAttack, Type SequenceType);